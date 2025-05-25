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
using Microsoft.Extensions.DependencyInjection;
using System;

namespace LABOGRA
{
    public partial class MainWindow : Window
    {
        private readonly IServiceProvider _serviceProvider;

        public MainWindow(IServiceProvider serviceProvider)
        {
            InitializeComponent();
            _serviceProvider = serviceProvider;

            // تعيين PatientsView كمحتوى افتراضي عند بدء التشغيل
            var patientsView = new PatientsView();
            patientsView.DataContext = _serviceProvider.GetRequiredService<PatientsViewModel>();
            MainContentArea.Content = patientsView;
        }

        private void AddPatientsButton_Click(object sender, RoutedEventArgs e)
        {
            var patientsView = new PatientsView();
            patientsView.DataContext = _serviceProvider.GetRequiredService<PatientsViewModel>();
            MainContentArea.Content = patientsView;
        }

        private void RecordResultsButton_Click(object sender, RoutedEventArgs e)
        {
            var resultsView = new ResultsView();
            resultsView.DataContext = _serviceProvider.GetRequiredService<ResultsViewModel>();
            MainContentArea.Content = resultsView;
        }

        private void PrintResultsButton_Click(object sender, RoutedEventArgs e)
        {
            var printView = new PrintView();
            printView.DataContext = _serviceProvider.GetRequiredService<PrintViewModel>();
            MainContentArea.Content = printView;
        }

        private void SearchEditButton_Click(object sender, RoutedEventArgs e)
        {
            var searchEditView = new SearchEditWindow();
            searchEditView.DataContext = _serviceProvider.GetRequiredService<SearchEditViewModel>();
            MainContentArea.Content = searchEditView;
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            var settingsMenuView = new SettingsMenuView();
            var settingsMenuViewModel = _serviceProvider.GetRequiredService<SettingsMenuViewModel>();
            settingsMenuViewModel.RequestNavigate += SettingsMenuViewModel_RequestNavigate;
            settingsMenuView.DataContext = settingsMenuViewModel;
            MainContentArea.Content = settingsMenuView;
        }

        private void SettingsMenuViewModel_RequestNavigate(object? sender, Type viewModelType)
        {
            UserControl? targetView = null;
            object? targetViewModel = null;

            if (viewModelType == typeof(TestsManagementViewModel))
            {
                targetViewModel = _serviceProvider.GetRequiredService<TestsManagementViewModel>();
                targetView = new TestsManagementView();
            }
            else if (viewModelType == typeof(UsersManagementViewModel))
            {
                targetViewModel = _serviceProvider.GetRequiredService<UsersManagementViewModel>();
                targetView = new UsersManagementView();
            }

            if (targetView != null && targetViewModel != null)
            {
                targetView.DataContext = targetViewModel;
                MainContentArea.Content = targetView;
            }
            else
            {
                MessageBox.Show("لا يمكن الانتقال إلى نوع الإعدادات المطلوب.", "خطأ في التنقل", MessageBoxButton.OK, MessageBoxImage.Error);
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