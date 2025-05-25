using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using LABOGRA.Services.Database.Data;
using LABOGRA.Services;
using LABOGRA.ViewModels; // General ViewModels namespace
using LABOGRA.ViewModels.Login; // <<== THIS WAS MISSING (UNCOMMENTED)
using LABOGRA.Views.Login;     // For LoginView
using System;                   // For IServiceProvider and IDisposable

namespace LABOGRA
{
    public partial class App : Application
    {
        public IServiceProvider? ServiceProvider { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            var services = new ServiceCollection();
            ConfigureServices(services);
            ServiceProvider = services.BuildServiceProvider();

            using (var scope = ServiceProvider.CreateScope())
            {
                var dbService = scope.ServiceProvider.GetRequiredService<IDatabaseService>();
                _ = dbService.EnsureDatabaseCreatedAsync();
            }

            var loginViewInstance = ServiceProvider.GetRequiredService<LoginView>();
            var result = loginViewInstance.ShowDialog();

            if (result == true)
            {
                var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
                // Before showing MainWindow, ensure its DataContext is set if it has one (e.g., MainViewModel)
                // Example: mainWindow.DataContext = ServiceProvider.GetRequiredService<MainViewModel>();
                mainWindow.Show();
            }
            else
            {
                Application.Current.Shutdown();
            }

            base.OnStartup(e);
        }

        private void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<LabDbContext>(options =>
                options.UseSqlite("Data Source=labogra.db"));

            services.AddTransient<IUserService, UserService>();
            services.AddTransient<IDatabaseService, DatabaseService>();

            services.AddTransient<PatientsViewModel>();
            services.AddTransient<UsersManagementViewModel>();
            services.AddTransient<ResultsViewModel>();
            services.AddTransient<PrintViewModel>();
            services.AddTransient<SearchEditViewModel>();
            services.AddTransient<SettingsMenuViewModel>();
            services.AddTransient<TestsManagementViewModel>();
            // Add LabOrderItemViewModel if it needs DI
            // services.AddTransient<LabOrderItemViewModel>();


            // Factory for LoginViewModel
            services.AddTransient<LoginViewModel>(sp =>
            {
                Action minimizeAction = () => { /* View should handle its own minimize */ };
                Action<string, string, MessageBoxButton, MessageBoxImage> showMessageAction =
                    (message, caption, button, icon) => MessageBox.Show(message, caption, button, icon);

                return new LoginViewModel(
                    sp.GetRequiredService<IUserService>(),
                    minimizeAction,
                    showMessageAction
                );
            });

            services.AddTransient<LoginView>();
            services.AddSingleton<MainWindow>();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            // Correct way to dispose the ServiceProvider
            if (ServiceProvider is IDisposable disposableProvider)
            {
                disposableProvider.Dispose();
            }
            base.OnExit(e);
        }
    }
}