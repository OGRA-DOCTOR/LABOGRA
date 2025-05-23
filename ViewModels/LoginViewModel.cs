using LABOGRA.Core;
using LABOGRA.Models;
using LABOGRA.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace LABOGRA.ViewModels.Login
{
    public class LoginViewModel : BaseViewModel
    {
        private readonly Action _showMainWindowAction;
        private readonly Action _closeLoginWindowAction;
        private readonly Action _minimizeLoginWindowAction;
        private readonly Action<string, string, MessageBoxButton, MessageBoxImage> _showMessageAction;

        public ObservableCollection<string> Usernames { get; set; } = new ObservableCollection<string>();

        private string? _selectedUsername = null;
        public string? SelectedUsername
        {
            get => _selectedUsername;
            set => SetProperty(ref _selectedUsername, value);
        }

        private string _password = string.Empty;
        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        private string _visiblePasswordText = string.Empty;
        public string VisiblePasswordText
        {
            get => _visiblePasswordText;
            set
            {
                if (SetProperty(ref _visiblePasswordText, value))
                {
                    if (IsPasswordVisible)
                        Password = value;
                }
            }
        }

        private bool _isPasswordVisible;
        public bool IsPasswordVisible
        {
            get => _isPasswordVisible;
            set
            {
                if (SetProperty(ref _isPasswordVisible, value))
                {
                    if (value)
                        VisiblePasswordText = Password;

                    OnPropertyChanged(nameof(PasswordBoxVisibility));
                    OnPropertyChanged(nameof(PasswordTextVisibility));
                }
            }
        }

        public Visibility PasswordBoxVisibility => IsPasswordVisible ? Visibility.Collapsed : Visibility.Visible;
        public Visibility PasswordTextVisibility => IsPasswordVisible ? Visibility.Visible : Visibility.Collapsed;

        public ICommand LoginCommand { get; }
        public ICommand MinimizeCommand { get; }
        public ICommand CloseCommand { get; }

        public LoginViewModel(
            Action showMainWindowAction,
            Action closeLoginWindowAction,
            Action minimizeLoginWindowAction,
            Action<string, string, MessageBoxButton, MessageBoxImage> showMessageAction)
        {
            _showMainWindowAction = showMainWindowAction ?? throw new ArgumentNullException(nameof(showMainWindowAction));
            _closeLoginWindowAction = closeLoginWindowAction ?? throw new ArgumentNullException(nameof(closeLoginWindowAction));
            _minimizeLoginWindowAction = minimizeLoginWindowAction ?? throw new ArgumentNullException(nameof(minimizeLoginWindowAction));
            _showMessageAction = showMessageAction ?? throw new ArgumentNullException(nameof(showMessageAction));

            EnsureAdminExists(); // إضافة تلقائية للمستخدم admin إذا مش موجود

            LoadUsernames();

            LoginCommand = new RelayCommand(ExecuteLogin, CanExecuteLogin);
            MinimizeCommand = new RelayCommand(ExecuteMinimize);
            CloseCommand = new RelayCommand(ExecuteClose);
        }

        private void EnsureAdminExists()
        {
            var users = UserService.LoadUsers();
            bool adminExists = users.Any(u => u.Username == "admin");

            if (!adminExists)
            {
                users.Add(new User
                {
                    Username = "admin",
                    PasswordHash = UserService.HashPassword("0000"),
                    Role = "Admin"
                });

                UserService.SaveUsers(users);
            }
        }

        private void LoadUsernames()
        {
            var users = UserService.LoadUsers();
            Usernames.Clear();
            foreach (var user in users)
                Usernames.Add(user.Username);
        }

        private bool CanExecuteLogin(object? parameter)
        {
            return !string.IsNullOrWhiteSpace(SelectedUsername);
        }

        private void ExecuteLogin(object? parameter)
        {
            if (string.IsNullOrWhiteSpace(SelectedUsername))
            {
                _showMessageAction.Invoke("الرجاء اختيار اسم مستخدم.", "خطأ في الدخول", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string currentPassword = IsPasswordVisible ? VisiblePasswordText : Password;

            var user = UserService.ValidateUser(SelectedUsername, currentPassword);
            if (user != null)
            {
                _showMainWindowAction.Invoke();
                _closeLoginWindowAction.Invoke();
            }
            else
            {
                _showMessageAction.Invoke("اسم المستخدم أو كلمة المرور غير صحيحة", "خطأ في الدخول", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteMinimize(object? parameter) => _minimizeLoginWindowAction.Invoke();

        private void ExecuteClose(object? parameter) => _closeLoginWindowAction.Invoke();
    }
}
