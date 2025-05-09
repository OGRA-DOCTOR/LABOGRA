// الإصدار: 4 (لهذا الملف)
// اسم الملف: LABOGRA/Views/Results/ResultsView.xaml.cs
// تاريخ التحديث: 2023-10-30
// الوصف:
// 1. تحسين منطق انتقال التركيز الفعلي للكتابة إلى TextBox الصحيح عند استخدام Enter أو الأسهم.
// 2. ضمان أن ListViewItem مُهيأ قبل محاولة التركيز.
using LABOGRA.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading; // لإضافة DispatcherPriority

namespace LABOGRA.Views.Results
{
    public partial class ResultsView : UserControl
    {
        public ResultsView()
        {
            InitializeComponent();
        }

        private void ResultsListView_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            var listView = sender as ListView;
            if (listView == null || listView.Items.Count == 0) return;

            var currentItemViewModel = listView.SelectedItem as LabOrderItemViewModel;
            int currentIndex = listView.SelectedIndex;

            // إذا لم يكن هناك عنصر محدد وكان هناك عناصر، حاول تحديد الأول
            if (currentIndex == -1 && listView.Items.Count > 0)
            {
                listView.SelectedIndex = 0;
                currentIndex = 0; // تحديث المؤشر
                // لا حاجة لتحديث currentItemViewModel هنا مباشرة، سيتم تحديثه عند تغيير SelectedIndex
            }

            // تحديث currentItemViewModel بعد التأكد من وجود تحديد
            currentItemViewModel = listView.SelectedItem as LabOrderItemViewModel;

            if (e.Key == Key.Enter)
            {
                if (currentItemViewModel != null && currentItemViewModel.SaveResultCommand.CanExecute(null))
                {
                    currentItemViewModel.SaveResultCommand.Execute(null);
                    e.Handled = true;

                    if (currentIndex < listView.Items.Count - 1)
                    {
                        MoveFocusToNextItem(listView, currentIndex + 1);
                    }
                }
            }
            else if (e.Key == Key.Down)
            {
                if (currentIndex < listView.Items.Count - 1)
                {
                    MoveFocusToNextItem(listView, currentIndex + 1);
                    e.Handled = true;
                }
            }
            else if (e.Key == Key.Up)
            {
                if (currentIndex > 0)
                {
                    MoveFocusToNextItem(listView, currentIndex - 1);
                    e.Handled = true;
                }
            }
        }

        private void MoveFocusToNextItem(ListView listView, int nextIndex)
        {
            listView.SelectedIndex = nextIndex;
            // التأكد من أن العنصر الجديد مرئي في ListView
            listView.ScrollIntoView(listView.SelectedItem);

            // استخدام Dispatcher للتأكد من أن الواجهة قد تم تحديثها
            // وأن ItemContainerGenerator يمكنه العثور على الحاوية
            listView.Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new System.Action(() =>
            {
                var nextListViewItem = listView.ItemContainerGenerator.ContainerFromIndex(nextIndex) as ListViewItem;
                FocusResultTextBoxInItem(nextListViewItem);
            }));
        }

        private void FocusResultTextBoxInItem(ListViewItem item)
        {
            if (item == null) return;

            // التأكد من أن العنصر مُهيأ ومرئي
            if (!item.IsLoaded)
            {
                // إذا لم يكن العنصر قد تم تحميله بعد (بسبب الـ Virtualization مثلاً)
                // قد نحتاج إلى انتظار تحميله أو التمرير إليه بشكل صريح
                // لكن ScrollIntoView في MoveFocusToNextItem يجب أن يساعد
                item.ApplyTemplate();
            }

            // استخدام Dispatcher مرة أخرى للتأكد من أن كل شيء جاهز للتركيز
            item.Dispatcher.BeginInvoke(DispatcherPriority.Input, new System.Action(() =>
            {
                // محاولة العثور على TextBox داخل DataTemplate
                TextBox resultTextBox = FindVisualChildByName<TextBox>(item, "ResultTextBox");
                if (resultTextBox != null)
                {
                    resultTextBox.Focus();
                    resultTextBox.SelectAll();
                }
                else
                {
                    // إذا لم يتم العثور عليه مباشرة، حاول البحث في ContentPresenter
                    ContentPresenter contentPresenter = FindVisualChild<ContentPresenter>(item);
                    if (contentPresenter != null)
                    {
                        // تأكد أن الـ DataTemplate تم تحميله على ContentPresenter
                        contentPresenter.ApplyTemplate();
                        resultTextBox = contentPresenter.ContentTemplate?.FindName("ResultTextBox", contentPresenter) as TextBox;
                        if (resultTextBox != null)
                        {
                            resultTextBox.Focus();
                            resultTextBox.SelectAll();
                        }
                    }
                }
            }));
        }

        // دالة مساعدة للبحث عن عنصر ابن بالاسم
        public static T FindVisualChildByName<T>(DependencyObject parent, string name) where T : FrameworkElement
        {
            if (parent == null) return null;
            T foundChild = null;
            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i) as FrameworkElement;
                if (child == null) continue;

                if (child is T && child.Name == name)
                {
                    foundChild = (T)child;
                    break;
                }
                foundChild = FindVisualChildByName<T>(child, name);
                if (foundChild != null) break;
            }
            return foundChild;
        }


        // دالة مساعدة عامة للبحث عن عنصر ابن من نوع معين داخل شجرة العناصر المرئية
        public static T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            if (parent == null) return null;
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);
                if (child != null && child is T)
                    return (T)child;
                else
                {
                    T childOfChild = FindVisualChild<T>(child);
                    if (childOfChild != null)
                        return childOfChild;
                }
            }
            return null;
        }

        private void ResultsView_Loaded(object sender, RoutedEventArgs e)
        {
            // عند تحميل الواجهة، إذا كان هناك عناصر، حاول التركيز على أول TextBox
            // تأكد من أن ComboBox لا يسرق التركيز
            this.Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, new System.Action(() =>
            {
                if (ResultsListView.Items.Count > 0 && ResultsListView.IsEnabled)
                {
                    if (ResultsListView.SelectedIndex == -1) // إذا لم يكن هناك شيء محدد
                    {
                        ResultsListView.SelectedIndex = 0;
                    }
                    ResultsListView.ScrollIntoView(ResultsListView.SelectedItem);
                    var firstItemContainer = ResultsListView.ItemContainerGenerator.ContainerFromIndex(ResultsListView.SelectedIndex) as ListViewItem;
                    FocusResultTextBoxInItem(firstItemContainer);
                }
            }));
        }
    }
}