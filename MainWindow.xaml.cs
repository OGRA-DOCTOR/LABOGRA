using LABOGRA.Views.Patients; // لاستخدام PatientsView
using LABOGRA.Views.Results; // لاستخدام ResultsView
using LABOGRA.Views.Print; // لاستخدام PrintView - تأكد من وجود هذا الـ using
using LABOGRA.Views.TestsManagement; // لاستخدام TestsManagementView
using System.Windows; // لاستخدام Window و MessageBox و RoutedEventArgs
using LABOGRA.Views.Login; // لاستخدام LoginView
using System.Windows.Controls; // لاستخدام UserControl (وهو نوع MainContentArea.Content)

namespace LABOGRA
{
    public partial class MainWindow : Window
    {
        private LoginView _loginView;

        // مُنشئ النافذة الرئيسية الذي يستقبل نافذة تسجيل الدخول لإخفائها
        public MainWindow(LoginView loginView)
        {
            InitializeComponent();
            _loginView = loginView;
            // عرض نافذة إدارة المرضى افتراضياً عند فتح النافذة الرئيسية
            MainContentArea.Content = new PatientsView();
        }

        // معالج حدث النقر على زر "إدارة المرضى"
        private void AddPatientsButton_Click(object sender, RoutedEventArgs e)
        {
            // عرض PatientsView في منطقة المحتوى الرئيسية
            MainContentArea.Content = new PatientsView();
        }

        // معالج حدث النقر على زر "تسجيل النتائج"
        private void RecordResultsButton_Click(object sender, RoutedEventArgs e)
        {
            // عرض ResultsView في منطقة المحتوى الرئيسية
            MainContentArea.Content = new ResultsView();
        }

        // معالج حدث النقر على زر "طباعة النتائج"
        private void PrintResultsButton_Click(object sender, RoutedEventArgs e)
        {
            // عرض PrintView الجديدة في منطقة المحتوى الرئيسية بدلاً من الرسالة القديمة
            MainContentArea.Content = new PrintView();
        }

        // معالج حدث النقر على زر "بحث وتعديل" (ما زالت غير مفعلة)
        private void SearchEditButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("وظيفة بحث وتعديل غير مفعلة حاليًا.", "بحث وتعديل", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // معالج حدث النقر على زر "الإعدادات" (يعرض حالياً TestsManagementView)
        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            // يمكنك اختيار عرض TestsManagementView أو أي لوحة إعدادات أخرى هنا
            MainContentArea.Content = new TestsManagementView();
        }
    }
}