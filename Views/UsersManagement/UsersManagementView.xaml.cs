using LABOGRA.ViewModels;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using LABOGRA.Services.Database.Data; // تعديل: استدعاء LabDbContext
using Microsoft.EntityFrameworkCore;

namespace LABOGRA.Views.UsersManagement
{
    public partial class UsersManagementView : UserControl
    {
        private UsersManagementViewModel? _viewModel;

        public UsersManagementView()
        {
            InitializeComponent();

            // إذا لم يتم تعيين DataContext خارجيًا، ننشئه مع DbContext جديد
            if (DataContext is UsersManagementViewModel vm)
            {
                _viewModel = vm;
                SubscribeToViewModelChanges();
                UpdateDynamicTexts();
            }
            else
            {
                var optionsBuilder = new DbContextOptionsBuilder<LabDbContext>();
                optionsBuilder.UseSqlite("Data Source=labdatabase.db"); // نفس مسار القاعدة
                var dbContext = new LabDbContext(optionsBuilder.Options);

                _viewModel = new UsersManagementViewModel(dbContext);
                DataContext = _viewModel;
                SubscribeToViewModelChanges();
                UpdateDynamicTexts();
            }
        }

        private void SubscribeToViewModelChanges()
        {
            if (_viewModel != null)
            {
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

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            if (_viewModel != null)
            {
                _viewModel.PropertyChanged -= ViewModel_PropertyChanged;
            }
        }
    }
}