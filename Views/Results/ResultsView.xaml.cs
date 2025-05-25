// بداية الكود لملف Views/Results/ResultsView.xaml.cs
using LABOGRA.ViewModels; // قد نحتاجه إذا كان LabOrderItemViewModel لا يزال يُستخدم هنا
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace LABOGRA.Views.Results
{
    public partial class ResultsView : UserControl
    {
        public ResultsView()
        {
            InitializeComponent();
            // لا نقوم بتعيين DataContext هنا لـ ResultsViewModel.
            // سيتم تعيينه من MainWindow.xaml.cs
            // الكود الخاص بالتركيز والـ KeyDown سيعتمد على الـ DataContext الذي تم تعيينه من الخارج
        }

        // باقي الدوال (ResultsListView_PreviewKeyDown, MoveFocusToNextItem, ...) تبقى كما هي
        // لأنها تتعامل مع الواجهة والـ DataContext الذي يُفترض أن يكون قد تم تعيينه.

        private void ResultsListView_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (sender is not ListView listView || listView.Items.Count == 0) return;

            // هنا يتم استخدام LabOrderItemViewModel، الآن سيتم العثور عليه
            var currentItemViewModel = listView.SelectedItem as LabOrderItemViewModel;
            int currentIndex = listView.SelectedIndex;

            if (currentIndex == -1 && listView.Items.Count > 0)
            {
                listView.SelectedIndex = 0;
                currentIndex = 0;
            }

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
            listView.ScrollIntoView(listView.SelectedItem);

            listView.Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new System.Action(() =>
            {
                if (listView.ItemContainerGenerator.ContainerFromIndex(nextIndex) is ListViewItem nextListViewItem)
                {
                    FocusResultTextBoxInItem(nextListViewItem);
                }
            }));
        }

        private void FocusResultTextBoxInItem(ListViewItem? item)
        {
            if (item == null) return;

            if (!item.IsLoaded)
            {
                item.ApplyTemplate(); // Ensure the template is applied
            }

            item.Dispatcher.BeginInvoke(DispatcherPriority.Input, new System.Action(() =>
            {
                TextBox? resultTextBox = FindVisualChildByName<TextBox>(item, "ResultTextBox");
                if (resultTextBox != null)
                {
                    resultTextBox.Focus();
                    resultTextBox.SelectAll();
                }
                else
                {
                    // Fallback if template is complex
                    ContentPresenter? contentPresenter = FindVisualChild<ContentPresenter>(item);
                    if (contentPresenter != null)
                    {
                        contentPresenter.ApplyTemplate(); // Ensure template for content presenter
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

        public static T? FindVisualChildByName<T>(DependencyObject? parent, string name) where T : FrameworkElement
        {
            if (parent == null) return null;
            T? foundChild = null;
            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                if (VisualTreeHelper.GetChild(parent, i) is FrameworkElement child)
                {
                    if (child is T typedChild && child.Name == name)
                    {
                        foundChild = typedChild;
                        break;
                    }
                    foundChild = FindVisualChildByName<T>(child, name);
                    if (foundChild != null) break;
                }
            }
            return foundChild;
        }

        public static T? FindVisualChild<T>(DependencyObject? parent) where T : DependencyObject
        {
            if (parent == null) return null;
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                DependencyObject? child = VisualTreeHelper.GetChild(parent, i);
                if (child is T typedChild)
                    return typedChild;

                T? childOfChild = FindVisualChild<T>(child);
                if (childOfChild != null)
                    return childOfChild;
            }
            return null;
        }

        private void ResultsView_Loaded(object sender, RoutedEventArgs e)
        {
            // تأكد من أن هذا الكود لا يسبب مشكلة إذا كان DataContext لم يتم تعيينه بعد
            // أو إذا كان ResultsListView فارغاً
            this.Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, new System.Action(() =>
            {
                if (ResultsListView.Items.Count > 0 && ResultsListView.IsEnabled)
                {
                    if (ResultsListView.SelectedIndex == -1)
                    {
                        ResultsListView.SelectedIndex = 0;
                    }
                    ResultsListView.ScrollIntoView(ResultsListView.SelectedItem);
                    if (ResultsListView.ItemContainerGenerator.ContainerFromItem(ResultsListView.SelectedItem) is ListViewItem firstItemContainer) // تعديل هنا
                    {
                        FocusResultTextBoxInItem(firstItemContainer);
                    }
                }
            }));
        }
    }
}
// نهاية الكود لملف Views/Results/ResultsView.xaml.cs