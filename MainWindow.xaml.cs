// بداية الكود لملف MainWindow.xaml.cs
using LABOGRA.Views.Patients;
using LABOGRA.Views.Results;
using LABOGRA.Views.Print;
using LABOGRA.Views.TestsManagement;
using LABOGRA.Views.SearchEdit; // هذا using صحيح لـ namespace
using LABOGRA.Views.Settings;
using LABOGRA.Views.UsersManagement;
using System.Windows;
using LABOGRA.ViewModels;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using LABOGRA.Services.Database.Data;
using System;

namespace LABOGRA
{
    public partial class MainWindow : Window
    {
        private readonly LabDbContext _dbContext;

        public MainWindow()
        {
            InitializeComponent();
            var optionsBuilder = new DbContextOptionsBuilder<LabDbContext>();
            optionsBuilder.UseSqlite(DatabasePathHelper.GetConnectionString());
            _dbContext = new LabDbContext(optionsBuilder.Options);

            var patientsViewModel = new PatientsViewModel(_dbContext);
            var patientsView = new PatientsView { DataContext = patientsViewModel };
            MainContentArea.Content = patientsView;
        }

        private void AddPatientsButton_Click(object sender, RoutedEventArgs e)
        {
            var patientsViewModel = new PatientsViewModel(_dbContext);
            var patientsView = new PatientsView { DataContext = patientsViewModel };
            MainContentArea.Content = patientsView;
        }

        private void RecordResultsButton_Click(object sender, RoutedEventArgs e)
        {
            var resultsViewModel = new ResultsViewModel(_dbContext);
            var resultsView = new ResultsView { DataContext = resultsViewModel };
            MainContentArea.Content = resultsView;
        }

        private void PrintResultsButton_Click(object sender, RoutedEventArgs e)
        {
            var printViewModel = new PrintViewModel(_dbContext);
            var printView = new PrintView { DataContext = printViewModel };
            MainContentArea.Content = printView;
        }

        private void SearchEditButton_Click(object sender, RoutedEventArgs e)
        {
            var searchEditViewModel = new SearchEditViewModel(_dbContext);
            // *** التصحيح هنا: استخدام اسم الكلاس الصحيح وهو SearchEditWindow ***
            var searchEditUserControl = new SearchEditWindow { DataContext = searchEditViewModel };
            MainContentArea.Content = searchEditUserControl;
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            var settingsMenuViewModel = new SettingsMenuViewModel();
            settingsMenuViewModel.RequestNavigate += SettingsMenuViewModel_RequestNavigate;
            var settingsMenuView = new SettingsMenuView { DataContext = settingsMenuViewModel };
            MainContentArea.Content = settingsMenuView;
        }

        private void SettingsMenuViewModel_RequestNavigate(object? sender, System.Type viewModelType)
        {
            UserControl? targetView = null;
            object? targetViewModel = null;

            if (viewModelType == typeof(TestsManagementViewModel))
            {
                targetViewModel = new TestsManagementViewModel(_dbContext);
                targetView = new TestsManagementView();
            }
            else if (viewModelType == typeof(UsersManagementViewModel))
            {
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
                MessageBox.Show($"لا يمكن الانتقال إلى نوع الإعدادات المطلوب.", "خطأ في التنقل", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
            var loginView = new LABOGRA.Views.Login.LoginView();
            loginView.Show();
        }

        protected override void OnClosed(EventArgs e)
        {
            _dbContext?.Dispose();
            base.OnClosed(e);
        }
    }
}
// نهاية الكود لملف MainWindow.xaml.cs