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
using System.Threading.Tasks;

namespace LABOGRA
{
    public partial class MainWindow : Window
    {
        private readonly IServiceProvider _serviceProvider;
        private static IServiceProvider? _staticServiceProvider;

        // Static property for global access
        public static IServiceProvider? ServiceProvider
        {
            get => _staticServiceProvider;
            set => _staticServiceProvider = value;
        }

        // Constructor with IServiceProvider for dependency injection
        public MainWindow(IServiceProvider serviceProvider)
        {
            InitializeComponent();
            _serviceProvider = serviceProvider;
            _staticServiceProvider = serviceProvider;

            // Initialize the window with default content
            InitializeDefaultContentAsync();
        }

        // Parameterless constructor for backward compatibility
        public MainWindow()
        {
            InitializeComponent();

            // Use static service provider if available, otherwise create a basic one
            if (_staticServiceProvider != null)
            {
                _serviceProvider = _staticServiceProvider;
            }
            else
            {
                // Create a minimal service provider for basic functionality
                var services = new ServiceCollection();

                // Register essential services
                services.AddTransient<PatientsViewModel>();
                services.AddTransient<ResultsViewModel>();
                services.AddTransient<PrintViewModel>();
                services.AddTransient<SearchEditViewModel>();
                services.AddTransient<SettingsMenuViewModel>();
                services.AddTransient<TestsManagementViewModel>();
                services.AddTransient<UsersManagementViewModel>();

                _serviceProvider = services.BuildServiceProvider();
                _staticServiceProvider = _serviceProvider;
            }

            // Initialize the window with default content
            InitializeDefaultContentAsync();
        }

        private async void InitializeDefaultContentAsync()
        {
            try
            {
                // Set PatientsView as default content on startup
                await Task.Run(() =>
                {
                    // This runs on a background thread to avoid blocking the UI
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
                // Handle any initialization errors gracefully
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show($"خطأ في تهيئة النافذة الرئيسية: {ex.Message}",
                                  "خطأ",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Error);
                });
            }
        }

        private async void AddPatientsButton_Click(object sender, RoutedEventArgs e)
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
                MessageBox.Show($"خطأ في تحميل شاشة المرضى: {ex.Message}",
                              "خطأ",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
            }
        }

        private async void RecordResultsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await Task.Run(() =>
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        var resultsView = new ResultsView();
                        var resultsViewModel = _serviceProvider.GetRequiredService<ResultsViewModel>();
                        resultsView.DataContext = resultsViewModel;
                        MainContentArea.Content = resultsView;
                    });
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في تحميل شاشة النتائج: {ex.Message}",
                              "خطأ",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
            }
        }

        private async void PrintResultsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await Task.Run(() =>
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        var printView = new PrintView();
                        var printViewModel = _serviceProvider.GetRequiredService<PrintViewModel>();
                        printView.DataContext = printViewModel;
                        MainContentArea.Content = printView;
                    });
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في تحميل شاشة الطباعة: {ex.Message}",
                              "خطأ",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
            }
        }

        private async void SearchEditButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await Task.Run(() =>
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        var searchEditView = new SearchEditWindow();
                        var searchEditViewModel = _serviceProvider.GetRequiredService<SearchEditViewModel>();
                        searchEditView.DataContext = searchEditViewModel;
                        MainContentArea.Content = searchEditView;
                    });
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في تحميل شاشة البحث والتحرير: {ex.Message}",
                              "خطأ",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
            }
        }

        private async void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await Task.Run(() =>
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        var settingsMenuView = new SettingsMenuView();
                        var settingsMenuViewModel = _serviceProvider.GetRequiredService<SettingsMenuViewModel>();
                        settingsMenuViewModel.RequestNavigate += SettingsMenuViewModel_RequestNavigate;
                        settingsMenuView.DataContext = settingsMenuViewModel;
                        MainContentArea.Content = settingsMenuView;
                    });
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في تحميل شاشة الإعدادات: {ex.Message}",
                              "خطأ",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
            }
        }

        private async void SettingsMenuViewModel_RequestNavigate(object? sender, Type viewModelType)
        {
            try
            {
                await Task.Run(() =>
                {
                    Application.Current.Dispatcher.Invoke(() =>
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
                            MessageBox.Show("لا يمكن الانتقال إلى نوع الإعدادات المطلوب.",
                                          "خطأ في التنقل",
                                          MessageBoxButton.OK,
                                          MessageBoxImage.Error);
                        }
                    });
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في التنقل إلى الإعدادات: {ex.Message}",
                              "خطأ",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
            }
        }

        private async void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await Task.Run(() =>
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        this.Close();
                        var loginView = new LABOGRA.Views.Login.LoginView();
                        loginView.Show();
                    });
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في تسجيل الخروج: {ex.Message}",
                              "خطأ",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
            }
        }

        // Method to handle window closing
        protected override void OnClosed(EventArgs e)
        {
            try
            {
                // Clean up resources if needed
                if (_serviceProvider is IDisposable disposableProvider)
                {
                    disposableProvider.Dispose();
                }
            }
            catch (Exception ex)
            {
                // Log the error but don't show a message box as the window is closing
                System.Diagnostics.Debug.WriteLine($"Error during window cleanup: {ex.Message}");
            }
            finally
            {
                base.OnClosed(e);
            }
        }

        // Static method to create MainWindow with service provider
        public static MainWindow CreateWithServiceProvider(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
            return new MainWindow(serviceProvider);
        }

        // Method to update service provider
        public void UpdateServiceProvider(IServiceProvider serviceProvider)
        {
            if (_serviceProvider != serviceProvider)
            {
                _staticServiceProvider = serviceProvider;
            }
        }
    }
}