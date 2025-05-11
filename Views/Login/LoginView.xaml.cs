// الإصدار 4: LoginView.xaml.cs
// الوصف: إضافة دالة لسحب النافذة من شريط العنوان المخصص.
using LABOGRA.ViewModels.Login;
using System.Windows;
using System.Windows.Input; // لاستخدام MouseButtonEventArgs

namespace LABOGRA.Views.Login
{
    public partial class LoginView : Window
    {
        private readonly LoginViewModel _viewModel;

        public LoginView()
        {
            InitializeComponent();

            _viewModel = new LoginViewModel(
                ShowMainWindow,
                this.Close,
                () => this.WindowState = WindowState.Minimized,
                (message, caption, button, icon) => MessageBox.Show(this, message, caption, button, icon)
            );
            this.DataContext = _viewModel;
        }

        private void ShowMainWindow()
        {
            var mainWindow = new MainWindow(); // تأكد أن مُنشئ MainWindow لا يتطلب LoginView
            mainWindow.Show();
        }

        // دالة لسحب النافذة عند الضغط على شريط العنوان المخصص
        private void Window_MouseDown_Drag(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && e.ButtonState == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }
    }
}