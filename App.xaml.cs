// الإصدار: 2 (لهذا الملف)
// اسم الملف: App.xaml.cs
// تاريخ التحديث: 2023-10-30
// الوصف: إضافة سطر تطبيق الترخيص المجتمعي لـ QuestPDF.

using System.Configuration;
using System.Data;
using System.Windows;
using QuestPDF.Infrastructure; // إضافة using لـ QuestPDF

namespace LABOGRA
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            // تطبيق الترخيص المجتمعي (المجاني) لـ QuestPDF
            // يجب استدعاء هذا السطر مرة واحدة عند بدء تشغيل التطبيق
            QuestPDF.Settings.License = LicenseType.Community;

            // InitializeComponent(); // إذا كانت موجودة، يجب أن تبقى.
            // Visual Studio عادة يضيف InitializeComponent() في ملف App.xaml (وليس في App.xaml.cs مباشرة)
            // تأكد من أن دالة InitializeComponent() (إذا كانت ضرورية هنا) لا تتعارض أو يتم استدعاؤها بشكل صحيح.
            // بشكل عام، للمشاريع الجديدة، يتم استدعاء InitializeComponent من مُنشئ UserControls/Windows وليس من App() constructor.
        }

        // يمكنك أيضاً وضع سطر الترخيص في OnStartup إذا كنت تفضل ذلك
        /*
        protected override void OnStartup(StartupEventArgs e)
        {
            QuestPDF.Settings.License = LicenseType.Community;
            base.OnStartup(e);
        }
        */
    }
}