using LABOGRA.Views.Patients;
using LABOGRA.Views.Results;
using LABOGRA.Views.Print;
using LABOGRA.Views.TestsManagement;
using System.Windows;
using LABOGRA.Views.Login;
using System.Windows.Controls;

namespace LABOGRA
{
    public partial class MainWindow : Window
    {
        private LoginView _loginView;

        public MainWindow(LoginView loginView)
        {
            InitializeComponent();
            _loginView = loginView;
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
            MessageBox.Show("وظيفة طباعة النتائج غير مفعلة حاليًا.", "طباعة النتائج", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void SearchEditButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("وظيفة بحث وتعديل غير مفعلة حاليًا.", "بحث وتعديل", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            MainContentArea.Content = new TestsManagementView();
        }
    }
}