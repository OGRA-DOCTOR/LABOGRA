using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using LABOGRA.Services.Database.Data;
using LABOGRA.Services;
using LABOGRA.ViewModels;
using LABOGRA.ViewModels.Login;

namespace LABOGRA
{
    public partial class App : Application
    {
        private ServiceProvider? _serviceProvider;

        protected override void OnStartup(StartupEventArgs e)
        {
            var services = new ServiceCollection();

            // تسجيل DbContext with proper connection string
            services.AddDbContext<LabDbContext>(options =>
                options.UseSqlite("Data Source=labogra.db"));

            // تسجيل Services
            services.AddTransient<IUserService, UserService>();
            services.AddTransient<IDatabaseService, DatabaseService>();

            // تسجيل ViewModels
            services.AddTransient<PatientsViewModel>();
            services.AddTransient<UsersManagementViewModel>();
            services.AddTransient<ResultsViewModel>();
            services.AddTransient<PrintViewModel>();
            services.AddTransient<SearchEditViewModel>();
            services.AddTransient<SettingsMenuViewModel>();
            services.AddTransient<TestsManagementViewModel>();
            services.AddTransient<LoginViewModel>();

            // تسجيل Windows
            services.AddSingleton<MainWindow>();

            _serviceProvider = services.BuildServiceProvider();

            // Ensure database is created
            using (var scope = _serviceProvider.CreateScope())
            {
                var dbService = scope.ServiceProvider.GetRequiredService<IDatabaseService>();
                _ = dbService.EnsureDatabaseCreatedAsync();
            }

            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();

            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _serviceProvider?.Dispose();
            base.OnExit(e);
        }
    }
}