using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System; // لـ System.Environment
using System.IO; // لـ Path

namespace LABOGRA.Services.Database.Data
{
    // هذا الكلاس خاص بأدوات Entity Framework Core لإدارة الهجرات (Migrations).
    // هو يخبر الأدوات كيف تقوم بإنشاء نسخة من LabDbContext.
    public class LabDbContextFactory : IDesignTimeDbContextFactory<LabDbContext>
    {
        // هذه هي الدالة التي ستستدعيها أدوات Entity Framework Core.
        public LabDbContext CreateDbContext(string[] args)
        {
            // هذا الكود يقوم بتكوين الخيارات (Options) التي يحتاجها LabDbContext.
            var optionsBuilder = new DbContextOptionsBuilder<LabDbContext>();

            // هنا نحدد أين سيتم حفظ ملف قاعدة بيانات SQLite.
            // نستخدم مسار قياسي لمجلد بيانات التطبيقات المحلية للمستخدم.
            // هذا المسار يكون عادةً في مجلد مخفي داخل مجلد المستخدم على الويندوز.
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var dbDirectory = Path.Combine(appDataPath, "LABOGRA"); // إنشاء مجلد باسم مشروعك
            var dbPath = Path.Combine(dbDirectory, "LABOGRA.db"); // اسم ملف قاعدة البيانات

            // تأكد أن المجلد الذي سيتم حفظ قاعدة البيانات فيه موجود، وإلا قم بإنشائه.
            Directory.CreateDirectory(dbDirectory);

            // أخبر Entity Framework Core باستخدام موفر SQLite
            // وحدد مسار ملف قاعدة البيانات باستخدام Connection String.
            optionsBuilder.UseSqlite($"Data Source={dbPath}");

            // قم بإنشاء وإرجاع نسخة جديدة من LabDbContext باستخدام الخيارات التي جهزناها.
            return new LabDbContext(optionsBuilder.Options);
        }
    }
}