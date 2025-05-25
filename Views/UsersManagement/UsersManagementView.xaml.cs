// بداية الكود لملف Views/UsersManagement/UsersManagementView.xaml.cs
using LABOGRA.ViewModels;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
// لم نعد بحاجة لـ LABOGRA.Services.Database.Data أو Microsoft.EntityFrameworkCore هنا

namespace LABOGRA.Views.UsersManagement
{
    public partial class UsersManagementView : UserControl
    {
        private UsersManagementViewModel? _viewModel;

        public UsersManagementView()
        {
            InitializeComponent();
            // نعتمد على أن DataContext سيتم تعيينه من الخارج (من MainWindow)
            // ونقوم بالاشتراك في الأحداث عند تحميل الواجهة
            this.Loaded += UsersManagementView_Loaded;
            this.Unloaded += UsersManagementView_Unloaded;
        }

        private void UsersManagementView_Loaded(object sender, RoutedEventArgs e)
        {
            // يتم تعيين DataContext من MainWindow
            if (DataContext is UsersManagementViewModel vm)
            {
                _viewModel = vm;
                SubscribeToViewModelChanges();
                UpdateDynamicTexts(); // استدعاء أولي لتحديث النصوص
            }
            // لا يوجد 'else' لإنشاء ViewModel هنا، لأنه يجب أن يتم تمريره
            // هذا يضمن أننا نستخدم دائماً الـ ViewModel الذي تم إنشاؤه بـ DbContext الصحيح
        }

        private void SubscribeToViewModelChanges()
        {
            if (_viewModel != null)
            {
                // التأكد من إزالة أي اشتراك سابق لتجنب التكرار إذا تم تحميل الواجهة عدة مرات
                _viewModel.PropertyChanged -= ViewModel_PropertyChanged;
                _viewModel.PropertyChanged += ViewModel_PropertyChanged;
            }
        }

        private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(UsersManagementViewModel.IsEditing))
            {
                UpdateDynamicTexts();
            }
        }

        private void UpdateDynamicTexts()
        {
            if (_viewModel == null) return;

            // تأكد أن عناصر XAML المسماة FormTitleTextBlock, PasswordHelpTextBlock, SaveButton موجودة في UsersManagementView.xaml
            if (FormTitleTextBlock != null && PasswordHelpTextBlock != null && SaveButton != null)
            {
                if (_viewModel.IsEditing)
                {
                    FormTitleTextBlock.Text = "تعديل المستخدم";
                    PasswordHelpTextBlock.Text = "(اتركها فارغة لعدم التغيير)";
                    SaveButton.Content = "حفظ التعديلات";
                }
                else
                {
                    FormTitleTextBlock.Text = "إضافة مستخدم جديد";
                    PasswordHelpTextBlock.Text = "(إلزامية عند الإضافة)";
                    SaveButton.Content = "إضافة";
                }
            }
        }

        private void UsersManagementView_Unloaded(object sender, RoutedEventArgs e)
        {
            if (_viewModel != null)
            {
                _viewModel.PropertyChanged -= ViewModel_PropertyChanged;
            }
            _viewModel = null; // تحرير المرجع للمساعدة في جمع البيانات المهملة (Garbage Collection)
        }
    }
}
// نهاية الكود لملف Views/UsersManagement/UsersManagementView.xaml.cs