using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using LABOGRA.Services.Database.Data;
using LABOGRA.Services;
using LABOGRA.ViewModels;

namespace LABOGRA
{
    public partial class App : Application
    {
        private ServiceProvider? _serviceProvider;

        protected override void OnStartup(StartupEventArgs e)
        {
            var services = new ServiceCollection();

            // تسجيل DbContext
            services.AddDbContext<LabDbContext>(options =>
                options.UseSqlite("connectionString"));

            // تسجيل Services
            services.AddTransient<IUserService, UserService>();
            services.AddTransient<IDatabaseService, DatabaseService>();

            // تسجيل ViewModels
            services.AddTransient<PatientsViewModel>();
            services.AddTransient<UsersManagementViewModel>();
            services.AddTransient<ResultsViewModel>();

            // تسجيل Windows
            services.AddSingleton<MainWindow>();

            _serviceProvider = services.BuildServiceProvider();

            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();

            base.OnStartup(e);
        }
    }
}