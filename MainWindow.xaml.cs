using LABOGRA.Views.Patients;
using LABOGRA.Views.Results;
using LABOGRA.Views.Print;
using LABOGRA.Views.TestsManagement;
using LABOGRA.Views.SearchEdit; // For SearchEditWindow as UserControl
using LABOGRA.Views.Settings;
using LABOGRA.Views.UsersManagement;
using System.Windows;
using LABOGRA.ViewModels;
using System.Windows.Controls; // For UserControl
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace LABOGRA
{
    public partial class MainWindow : Window
    {
        private readonly IServiceProvider _serviceProvider;

        public MainWindow(IServiceProvider serviceProvider)
        {
            InitializeComponent();
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            InitializeDefaultContentAsync();
        }

        private async void InitializeDefaultContentAsync()
        {
            try
            {
                await Task.Run(() =>
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        var patientsView = new PatientsView();
                        var patientsViewModel = _serviceProvider.GetRequiredService<PatientsViewModel>();
                        patientsView.DataContext = patientsViewModel;
                        MainContentArea.Content = patientsView;
                    });
                });
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show($"خطأ في تهيئة النافذة الرئيسية: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                });
            }
        }

        private async Task SetMainContentAsync<TView, TViewModel>()
            where TView : FrameworkElement, new()
            where TViewModel : class
        {
            try
            {
                var viewModel = _serviceProvider.GetRequiredService<TViewModel>();

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    var view = new TView { DataContext = viewModel };
                    MainContentArea.Content = view;
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في تحميل الواجهة: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void AddPatientsButton_Click(object sender, RoutedEventArgs e)
        {
            await SetMainContentAsync<PatientsView, PatientsViewModel>();
        }

        private async void RecordResultsButton_Click(object sender, RoutedEventArgs e)
        {
            await SetMainContentAsync<ResultsView, ResultsViewModel>();
        }

        private async void PrintResultsButton_Click(object sender, RoutedEventArgs e)
        {
            await SetMainContentAsync<PrintView, PrintViewModel>();
        }

        // *** This is the corrected version for SearchEditWindow as a UserControl ***
        private async void SearchEditButton_Click(object sender, RoutedEventArgs e)
        {
            await SetMainContentAsync<SearchEditWindow, SearchEditViewModel>();
        }

        private async void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var settingsMenuViewModel = _serviceProvider.GetRequiredService<SettingsMenuViewModel>();
                settingsMenuViewModel.RequestNavigate += SettingsMenuViewModel_RequestNavigate;

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    var settingsMenuView = new SettingsMenuView { DataContext = settingsMenuViewModel };
                    MainContentArea.Content = settingsMenuView;
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في تحميل شاشة الإعدادات: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void SettingsMenuViewModel_RequestNavigate(object? sender, Type viewModelType)
        {
            if (MainContentArea.Content is FrameworkElement currentView && currentView.DataContext is SettingsMenuViewModel oldVm)
            {
                oldVm.RequestNavigate -= SettingsMenuViewModel_RequestNavigate;
            }

            try
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
                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        targetView.DataContext = targetViewModel;
                        MainContentArea.Content = targetView;
                    });
                }
                else
                {
                    MessageBox.Show("لا يمكن الانتقال إلى نوع الإعدادات المطلوب.", "خطأ في التنقل", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في التنقل إلى الإعدادات: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
        }
    }
}