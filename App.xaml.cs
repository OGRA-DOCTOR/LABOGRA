// بداية الكود لملف App.xaml.cs
using LABOGRA.Services.Database.Data;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Infrastructure;
using System.Windows;

namespace LABOGRA
{
    public partial class App : Application
    {
        public App()
        {
            QuestPDF.Settings.License = LicenseType.Community;
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            var factory = new LabDbContextFactory();
            using (var context = factory.CreateDbContext(null!))
            {
                try
                {
                    context.Database.Migrate();
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show($"حدث خطأ أثناء تطبيق تحديثات قاعدة البيانات: {ex.Message}", "خطأ في تهيئة قاعدة البيانات", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
// نهاية الكود لملف App.xaml.cs