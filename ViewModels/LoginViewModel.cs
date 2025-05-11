// الإصدار 2: LoginViewModel.cs
// الوصف: ViewModel الخاص بنافذة تسجيل الدخول، مع تهيئة Usernames وتصحيحات.
using LABOGRA.Core; // لاستخدام BaseViewModel و RelayCommand
using System;
using System.Collections.ObjectModel;
using System.Linq; // لاستخدام FirstOrDefault
using System.Windows; // لاستخدام Visibility و MessageBoxButton و MessageBoxImage
using System.Windows.Input;

namespace LABOGRA.ViewModels.Login
{
    public class LoginViewModel : BaseViewModel
    {
        private readonly Action _showMainWindowAction;
        private readonly Action _closeLoginWindowAction;
        private readonly Action _minimizeLoginWindowAction;
        private readonly Action<string, string, MessageBoxButton, MessageBoxImage> _showMessageAction;

        private string _selectedUsername = "admin";
        public string SelectedUsername
        {
            get => _selectedUsername;
            set => SetProperty(ref _selectedUsername, value);
        }

        private ObservableCollection<string> _usernames = new ObservableCollection<string> { "admin", "user" };
        public ObservableCollection<string> Usernames
        {
            get => _usernames;
            set => SetProperty(ref _usernames, value);
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
                    {
                        Password = value; // تزامن مع Password عند تغيير النص الظاهر
                    }
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
                    if (_isPasswordVisible)
                    {
                        VisiblePasswordText = Password; // عرض كلمة المرور المخزنة
                    }
                    // لا حاجة لتغيير Password عند الإخفاء، هي تبقى كما هي في PasswordBox
                    OnPropertyChanged(nameof(PasswordBoxVisibility));
                    OnPropertyChanged(nameof(PasswordTextVisibility));
                }
            }
        }

        public Visibility PasswordBoxVisibility => IsPasswordVisible ? Visibility.Collapsed : Visibility.Visible;
        public Visibility PasswordTextVisibility => IsPasswordVisible ? Visibility.Visible : Visibility.Collapsed;

        private bool _rememberMe;
        public bool RememberMe
        {
            get => _rememberMe;
            set => SetProperty(ref _rememberMe, value);
        }

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

            // Usernames تم تهيئتها عند التصريح
            // SelectedUsername = Usernames.FirstOrDefault() ?? "admin"; // تم تعيينها كقيمة افتراضية عند التصريح

            LoginCommand = new RelayCommand(ExecuteLogin, CanExecuteLogin);
            MinimizeCommand = new RelayCommand(ExecuteMinimize);
            CloseCommand = new RelayCommand(ExecuteClose);
        }

        private bool CanExecuteLogin(object? parameter)
        {
            return !string.IsNullOrWhiteSpace(SelectedUsername);
        }

        private void ExecuteLogin(object? parameter)
        {
            // الحصول على كلمة المرور الصحيحة سواء كانت ظاهرة أو من PasswordBox
            string currentPasswordToValidate = IsPasswordVisible ? VisiblePasswordText : Password;

            if ((SelectedUsername == "admin" || SelectedUsername == "user") && currentPasswordToValidate == "0000")
            {
                _showMainWindowAction.Invoke();
                _closeLoginWindowAction.Invoke();
            }
            else
            {
                _showMessageAction.Invoke("اسم المستخدم أو كلمة المرور غير صحيحة", "خطأ في الدخول", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteMinimize(object? parameter)
        {
            _minimizeLoginWindowAction.Invoke();
        }

        private void ExecuteClose(object? parameter)
        {
            _closeLoginWindowAction.Invoke();
        }
    }
}