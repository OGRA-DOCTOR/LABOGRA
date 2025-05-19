// الإصدار: 8 (لهذا الملف - تصحيح أخطاء Nullability والـ Namespace)
// اسم الملف: MainWindow.xaml.cs
// الوصف: الكود الخلفي للنافذة الرئيسية، مع تصحيح أخطاء بناءً على Error List.
using LABOGRA.Views.Patients;
using LABOGRA.Views.Results;
using LABOGRA.Views.Print;
using LABOGRA.Views.TestsManagement; // استخدام View إدارة التحاليل
using LABOGRA.Views.SearchEdit; // استخدام Window البحث والتعديل (ملاحظة: هذه Window وليست UserControl حالياً)
using LABOGRA.Views.Settings; // استخدام للمجلد الجديد لقائمة الإعدادات
using System.Windows;
using LABOGRA.ViewModels; // *** تصحيح هنا: لا يوجد مجلد فرعي Settings داخل ViewModels، الفئة مباشرة هنا ***
// using LABOGRA.ViewModels.Settings; // تم حذف هذا السطر الخاطئ

using System; // لاستخدام Type
using System.Windows.Controls; // لاستخدام UserControl

namespace LABOGRA
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            // يتم عرض شاشة تسجيل المرضى عند بدء التشغيل بشكل افتراضي
            MainContentArea.Content = new PatientsView();
        }

        private void AddPatientsButton_Click(object sender, RoutedEventArgs e)
        {
            MainContentArea.Content = new PatientsView();
        }

        private void RecordResultsButton_Click(object sender, RoutedEventArgs e)
        {
            MainContentArea.Content = new ResultsView();
        }

        private void PrintResultsButton_Click(object sender, RoutedEventArgs e)
        {
            MainContentArea.Content = new PrintView();
        }

        private void SearchEditButton_Click(object sender, RoutedEventArgs e)
        {
            // هذا الكود يعرض نافذة SearchEditWindow.
            // ملاحظة من الكود السابق الذي أرسلته: أنت تقوم هنا بإنشاء Window
            // (SearchEditWindow) وتحاول وضعها في ContentControl (MainContentArea)
            // الذي عادة يستخدم لعرض UserControl. هذا الاستخدام غير صحيح للـ Window
            // التي يجب أن تُعرض باستخدام Show() أو ShowDialog().
            // إذا كان القصد عرض وظيفة البحث والتعديل داخل النافذة الرئيسية
            // فيجب تحويل SearchEditWindow إلى UserControl (مثلاً SearchEditView).
            // سأترك الكود كما هو بناءً على ما قدمته حالياً، مع ملاحظة هذه النقطة.
            // التصحيح السليم لعرضها كـ UserControl سيتطلب إنشاء:
            // 1. SearchEditView : UserControl بدلاً من SearchEditWindow : Window
            // 2. استخدام الكود التالي بدلاً من السطرين أدناه:
            // var searchEditView = new SearchEditView();
            // searchEditView.DataContext = new SearchEditViewModel(); // تأكد من وجود SearchEditViewModel
            // MainContentArea.Content = searchEditView;

            // الكود الأصلي الذي وضع Window داخل ContentControl (غير صحيح تقنياً):
            var searchEditWindow = new SearchEditWindow(); // هذه Window
            searchEditWindow.DataContext = new SearchEditViewModel(); // يفترض وجود SearchEditViewModel
            MainContentArea.Content = searchEditWindow; // هذا السطر هو استخدام غير صحيح للـ Window في ContentControl

        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            // 1. إنشاء الـ ViewModel لقائمة الإعدادات
            var settingsMenuViewModel = new SettingsMenuViewModel(); // الآن SettingsMenuViewModel موجودة مباشرة في LABOGRA.ViewModels

            // 2. الاشتراك في حدث طلب الانتقال من ViewModel القائمة
            // *** تصحيح هنا: توقيع الحدث يتطلب object? sender ***
            settingsMenuViewModel.RequestNavigate += SettingsMenuViewModel_RequestNavigate;

            // 3. إنشاء الـ View لقائمة الإعدادات
            var settingsMenuView = new SettingsMenuView();

            // 4. ربط الـ ViewModel بالـ View
            settingsMenuView.DataContext = settingsMenuViewModel;

            // 5. عرض قائمة الإعدادات في منطقة المحتوى الرئيسية
            MainContentArea.Content = settingsMenuView;
        }

        // *** تصحيح هنا: توقيع الدالة يجب أن يطابق توقيع EventHandler<Type> ***
        // يتطلب object? sender ليتوافق مع المعيار
        private void SettingsMenuViewModel_RequestNavigate(object? sender, Type viewModelType)
        {
            // *** تصحيح هنا: تعريف المتغيرات كـ nullable باستخدام '?' ***
            UserControl? targetView = null;
            object? targetViewModel = null;

            // إدارة الاشتراك في الحدث: يجب إلغاء الاشتراك عندما يتم الانتقال بعيداً عن SettingsMenuView
            if (sender is SettingsMenuViewModel menuViewModel)
            {
                menuViewModel.RequestNavigate -= SettingsMenuViewModel_RequestNavigate;
            }

            // تحديد الـ View والـ ViewModel المطلوب بناءً على الـ viewModelType المرسل
            if (viewModelType == typeof(TestsManagementViewModel))
            {
                targetViewModel = new TestsManagementViewModel();
                targetView = new TestsManagementView();
            }
            // أضف هنا لاحقاً حالات أخرى لأنواع ViewModels الإعدادات الفرعية الأخرى
            // else if (viewModelType == typeof(UsersManagementViewModel))
            // {
            //     targetViewModel = new UsersManagementViewModel();
            //     targetView = new UsersManagementView();
            // }
            // ... إلخ

            // إذا تم تحديد View و ViewModel بنجاح
            if (targetView != null && targetViewModel != null)
            {
                // ربط الـ ViewModel بالـ View المستهدف
                targetView.DataContext = targetViewModel;

                // عرض الـ View المستهدف في منطقة المحتوى الرئيسية
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
    }
}