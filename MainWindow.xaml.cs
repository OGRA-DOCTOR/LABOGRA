// الإصدار 3: MainWindow.xaml.cs
// الوصف: النسخة النهائية (حتى الآن) مع مُنشئ معدل وإعادة فتح LoginView عند الخروج.
using LABOGRA.Views.Patients;
using LABOGRA.Views.Results;
using LABOGRA.Views.Print;
using LABOGRA.Views.TestsManagement;
// using LABOGRA.Views.Login; // لم نعد بحاجة إليه هنا بشكل مباشر
using System.Windows;
using System.Windows.Controls; // لاستخدام UserControl

namespace LABOGRA
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            MainContentArea.Content = new PatientsView();
        }

        private void AddPatientsButton_Click(object sender, RoutedEventArgs e)
            => MainContentArea.Content = new PatientsView();

        private void RecordResultsButton_Click(object sender, RoutedEventArgs e)
            => MainContentArea.Content = new ResultsView();

        private void PrintResultsButton_Click(object sender, RoutedEventArgs e)
            => MainContentArea.Content = new PrintView();

        private void SearchEditButton_Click(object sender, RoutedEventArgs e)
            => MessageBox.Show("وظيفة بحث وتعديل غير مفعلة حاليًا.", "بحث وتعديل", MessageBoxButton.OK, MessageBoxImage.Information);

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
            => MainContentArea.Content = new TestsManagementView();

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close(); // إغلاق النافذة الحالية

            // فتح نافذة تسجيل الدخول مرة أخرى
            var loginView = new LABOGRA.Views.Login.LoginView();
            loginView.Show();
        }
    }
}