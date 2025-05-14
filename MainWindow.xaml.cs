// الإصدار: 6 (لهذا الملف - عرض SearchEditWindow كـ UserControl)
// اسم الملف: MainWindow.xaml.cs
// الوصف: الكود الخلفي للنافذة الرئيسية، مع تعديل لعرض SearchEditWindow كـ UserControl.
using LABOGRA.Views.Patients;
using LABOGRA.Views.Results;
using LABOGRA.Views.Print;
using LABOGRA.Views.TestsManagement;
using LABOGRA.Views.SearchEdit;
using System.Windows;
using LABOGRA.ViewModels; // *** إضافة using للوصول إلى ViewModels ***

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
            // *** بداية التعديل هنا ***
            var searchEditView = new SearchEditWindow();
            // تعيين الـ DataContext للـ UserControl
            searchEditView.DataContext = new SearchEditViewModel(); // افترض أن SearchEditViewModel في LABOGRA.ViewModels
            // عرض الـ UserControl داخل منطقة المحتوى الرئيسية
            MainContentArea.Content = searchEditView;
            // *** نهاية التعديل هنا ***
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            MainContentArea.Content = new TestsManagementView();
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();

            var loginView = new LABOGRA.Views.Login.LoginView();
            loginView.Show();
        }
    }
}