using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

namespace LABOGRA.Views.Login
{
    public partial class LoginView : Window
    {
        private Dictionary<string, string> users = new Dictionary<string, string>
        {
            { "admin", "0000" },
            { "user", "0000" }
        };

        public LoginView()
        {
            InitializeComponent();
            LoadUsernames();

            // إضافة معالج حدث لضغط زر Enter
            this.KeyDown += LoginView_KeyDown;
        }

        private void LoadUsernames()
        {
            // تحميل أسماء المستخدمين في القائمة المنسدلة
            foreach (var username in users.Keys)
            {
                UsernameComboBox.Items.Add(username);
            }
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            PerformLogin();
        }

        private void LoginView_KeyDown(object sender, KeyEventArgs e)
        {
            // التحقق إذا كان المستخدم ضغط على زر Enter
            if (e.Key == Key.Enter)
            {
                PerformLogin();
            }
        }

        private void PerformLogin()
        {
            string username = UsernameComboBox.Text;
            string password = PasswordBox.Password;

            // التحقق من صحة بيانات تسجيل الدخول
            if (users.ContainsKey(username) && users[username] == password)
            {
                // إخفاء نافذة تسجيل الدخول بدلاً من إغلاقها
                this.Hide();

                // فتح النافذة الرئيسية
                MainWindow mainWindow = new MainWindow(this);

                mainWindow.Show();
            }
            else
            {
                MessageBox.Show("Invalid username or password.", "Login Failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
