using LABOGRA.ViewModels.Login;
using System.Windows;
using System.Windows.Input;

namespace LABOGRA.Views.Login
{
    public partial class LoginView : Window
    {
        public LoginView(LoginViewModel viewModel) // ViewModel is injected
        {
            InitializeComponent();
            this.DataContext = viewModel;

            if (viewModel != null)
            {
                viewModel.RequestCloseDialog += (sender, dialogResult) =>
                {
                    try
                    {
                        this.DialogResult = dialogResult;
                    }
                    catch (InvalidOperationException)
                    {
                        // Can occur if ShowDialog wasn't called, or window is closing already.
                        // Or if DialogResult is set after the window is already closed.
                        // For now, we assume ShowDialog is used from App.xaml.cs
                    }
                    this.Close();
                };
            }
        }

        private void Window_MouseDown_Drag(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && e.ButtonState == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }
    }
}