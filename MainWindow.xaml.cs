using LABOGRA.Views.Patients;
using LABOGRA.Views.Results;
using LABOGRA.Views.Print;
using LABOGRA.Views.TestsManagement;
using LABOGRA.Views.SearchEdit;
using LABOGRA.Views.Settings;
using LABOGRA.Views.UsersManagement;
using System.Windows;
using LABOGRA.ViewModels;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using LABOGRA.Services.Database.Data; // تعديل: استدعاء LabDbContext

namespace LABOGRA
{
    public partial class MainWindow : Window
    {
        private readonly LabDbContext _dbContext;

        public MainWindow()
        {
            InitializeComponent();

            // تهيئة DbContext مع SQLite
            var optionsBuilder = new DbContextOptionsBuilder<LabDbContext>();
            optionsBuilder.UseSqlite("Data Source=labdatabase.db"); // حط المسار الصحيح للقاعدة هنا
            _dbContext = new LabDbContext(optionsBuilder.Options);

            MainContentArea.Content = new PatientsView();
        }

        private void AddPatientsButton_Click(object sender, RoutedEventArgs e)
        {
            MainContentArea.Content = new PatientsView();
        }

        private void RecordResultsButton_Click(object sender, RoutedEventArgs e)
        {
            MainContentArea.Content = new ResultsView();
        }

        private void PrintResultsButton_Click(object sender, RoutedEventArgs e)
        {
            MainContentArea.Content = new PrintView();
        }

        private void SearchEditButton_Click(object sender, RoutedEventArgs e)
        {
            var searchEditWindow = new SearchEditWindow();
            searchEditWindow.DataContext = new SearchEditViewModel();
            MainContentArea.Content = searchEditWindow;
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            var settingsMenuViewModel = new SettingsMenuViewModel();
            settingsMenuViewModel.RequestNavigate += SettingsMenuViewModel_RequestNavigate;
            var settingsMenuView = new SettingsMenuView();
            settingsMenuView.DataContext = settingsMenuViewModel;
            MainContentArea.Content = settingsMenuView;
        }

        private void SettingsMenuViewModel_RequestNavigate(object? sender, System.Type viewModelType)
        {
            UserControl? targetView = null;
            object? targetViewModel = null;

            if (sender is SettingsMenuViewModel menuViewModel)
            {
                menuViewModel.RequestNavigate -= SettingsMenuViewModel_RequestNavigate;
            }

            if (viewModelType == typeof(TestsManagementViewModel))
            {
                targetViewModel = new TestsManagementViewModel();
                targetView = new TestsManagementView();
            }
            else if (viewModelType == typeof(UsersManagementViewModel))
            {
                // هنا نمرر الـ DbContext للفيو موديل المستخدمين
                targetViewModel = new UsersManagementViewModel(_dbContext);
                targetView = new UsersManagementView();
            }

            if (targetView != null && targetViewModel != null)
            {
                targetView.DataContext = targetViewModel;
                MainContentArea.Content = targetView;
            }
            else
            {
                MessageBox.Show($"لا يمكن الانتقال إلى نوع الإعدادات المطلوب أو أن الـ ViewModel/View غير معرف.", "خطأ في التنقل", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
            var loginView = new LABOGRA.Views.Login.LoginView();
            loginView.Show();
        }
    }
}