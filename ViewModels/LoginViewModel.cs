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
        private readonly IUserService _userService;
        private readonly Action _minimizeLoginWindowAction; // Keep if View still provides this
        private readonly Action<string, string, MessageBoxButton, MessageBoxImage> _showMessageAction; // Keep for now

        public event EventHandler<bool?>? RequestCloseDialog; // Event to signal view to close

        public ObservableCollection<string> Usernames { get; set; } = new ObservableCollection<string>();
        // ... (rest of your properties: SelectedUsername, Password, VisiblePasswordText, IsPasswordVisible etc. remain the same) ...
        private string? _selectedUsername = null;
        public string? SelectedUsername { get => _selectedUsername; set => SetProperty(ref _selectedUsername, value); }
        private string _password = string.Empty;
        public string Password { get => _password; set => SetProperty(ref _password, value); }
        private string _visiblePasswordText = string.Empty;
        public string VisiblePasswordText { get => _visiblePasswordText; set { if (SetProperty(ref _visiblePasswordText, value)) { if (IsPasswordVisible) Password = value; } } }
        private bool _isPasswordVisible;
        public bool IsPasswordVisible { get => _isPasswordVisible; set { if (SetProperty(ref _isPasswordVisible, value)) { if (value) VisiblePasswordText = Password; OnPropertyChanged(nameof(PasswordBoxVisibility)); OnPropertyChanged(nameof(PasswordTextVisibility)); } } }
        public Visibility PasswordBoxVisibility => IsPasswordVisible ? Visibility.Collapsed : Visibility.Visible;
        public Visibility PasswordTextVisibility => IsPasswordVisible ? Visibility.Visible : Visibility.Collapsed;


        public ICommand LoginCommand { get; }
        public ICommand MinimizeCommand { get; }
        // CloseCommand might not be needed if closing is handled by RequestCloseDialog or a system button
        // public ICommand CloseCommand { get; } 

        public LoginViewModel(
            IUserService userService,
            Action minimizeLoginWindowAction, // Example: if still needed from view/factory
            Action<string, string, MessageBoxButton, MessageBoxImage> showMessageAction) // Example
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _minimizeLoginWindowAction = minimizeLoginWindowAction; // Store if passed
            _showMessageAction = showMessageAction ?? throw new ArgumentNullException(nameof(showMessageAction));

            EnsureAdminExists();
            LoadUsernames();

            LoginCommand = new RelayCommand(ExecuteLogin, CanExecuteLogin);
            MinimizeCommand = new RelayCommand(ExecuteMinimize); // Uses _minimizeLoginWindowAction
            // CloseCommand = new RelayCommand(() => RequestCloseDialog?.Invoke(this, null)); // Example: close with no specific result (cancel)
        }

        private void EnsureAdminExists() { var users = _userService.LoadUsers(); if (!users.Any(u => u.Username == "admin")) { users.Add(new User { Username = "admin", PasswordHash = _userService.HashPassword("0000"), Role = "Admin" }); _userService.SaveUsers(users); } }
        private void LoadUsernames() { var users = _userService.LoadUsers(); Usernames.Clear(); foreach (var user in users) Usernames.Add(user.Username); }
        private bool CanExecuteLogin(object? parameter) { return !string.IsNullOrWhiteSpace(SelectedUsername) && !string.IsNullOrEmpty(Password); }

        private void ExecuteLogin(object? parameter)
        {
            if (string.IsNullOrWhiteSpace(SelectedUsername) || string.IsNullOrEmpty(Password))
            {
                _showMessageAction.Invoke("الرجاء إدخال اسم المستخدم وكلمة المرور.", "خطأ", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var user = _userService.ValidateUser(SelectedUsername, Password);
            if (user != null)
            {
                RequestCloseDialog?.Invoke(this, true); // Signal success, request dialog close
            }
            else
            {
                _showMessageAction.Invoke("اسم المستخدم أو كلمة المرور غير صحيحة", "خطأ في الدخول", MessageBoxButton.OK, MessageBoxImage.Error);
                // Optionally: RequestCloseDialog?.Invoke(this, false); // If you want to close on failure too
            }
        }
        private void ExecuteMinimize(object? parameter) => _minimizeLoginWindowAction?.Invoke();
    }
}