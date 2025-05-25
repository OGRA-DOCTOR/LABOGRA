// بداية الكود لملف ViewModels/UsersManagementViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input; // For RelayCommand and AsyncRelayCommand
using LABOGRA.Models;
using LABOGRA.Services;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
// using System.Threading.Tasks; // No longer needed if all commands become synchronous
using System.Windows;
using System.Collections.Generic;

namespace LABOGRA.ViewModels
{
    public partial class UsersManagementViewModel : ObservableObject
    {
        private readonly IUserService _userService;

        [ObservableProperty] private ObservableCollection<User> _users = new ObservableCollection<User>();
        [ObservableProperty][NotifyCanExecuteChangedFor(nameof(PrepareEditUserCommand))][NotifyCanExecuteChangedFor(nameof(DeleteUserCommand))] private User? _selectedUser;
        [ObservableProperty][NotifyCanExecuteChangedFor(nameof(SaveUserCommand))] private string _entryUsername = string.Empty;
        [ObservableProperty][NotifyCanExecuteChangedFor(nameof(SaveUserCommand))] private string _entryPassword = string.Empty;
        [ObservableProperty][NotifyCanExecuteChangedFor(nameof(SaveUserCommand))] private string? _selectedEntryRole;
        [ObservableProperty][NotifyCanExecuteChangedFor(nameof(SaveUserCommand))] private bool _isEditing;

        public List<string> AvailableRoles { get; } = new List<string> { "Admin", "User" };

        public UsersManagementViewModel(IUserService userService)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));

            // All commands are now synchronous RelayCommand
            LoadUsersCommand = new RelayCommand(LoadUsers);
            PrepareNewUserCommand = new RelayCommand(PrepareNewUser);
            PrepareEditUserCommand = new RelayCommand(PrepareEditUser, CanEditOrDeleteUser);
            SaveUserCommand = new RelayCommand(SaveUser, CanSaveUser); // Changed to RelayCommand
            DeleteUserCommand = new RelayCommand(DeleteUser, CanEditOrDeleteUser); // Changed to RelayCommand
            CancelCommand = new RelayCommand(CancelOperation);

            SelectedEntryRole = AvailableRoles.FirstOrDefault();
            LoadUsersCommand.Execute(null);
        }

        public IRelayCommand LoadUsersCommand { get; }
        public IRelayCommand PrepareNewUserCommand { get; }
        public IRelayCommand PrepareEditUserCommand { get; }
        public IRelayCommand SaveUserCommand { get; } // Changed type
        public IRelayCommand DeleteUserCommand { get; } // Changed type
        public IRelayCommand CancelCommand { get; }

        private void LoadUsers() { /* ... (same as previous correct version) ... */ try { Debug.WriteLine($"UsersManagementViewModel - Attempting to load users via IUserService."); var usersFromService = _userService.LoadUsers(); Users.Clear(); if (usersFromService == null || !usersFromService.Any()) { if (!_userService.UserExists("Admin")) { var defaultAdmin = new User { Username = "Admin", PasswordHash = _userService.HashPassword("0000"), Role = "Admin" }; _userService.AddUser(defaultAdmin); usersFromService = _userService.LoadUsers(); } } if (usersFromService != null) { foreach (var user in usersFromService.OrderBy(u => u.Username)) Users.Add(user); } } catch (Exception ex) { Debug.WriteLine($"Error in LoadUsers: {ex.ToString()}"); MessageBox.Show($"Error loading users: {ex.Message}\n\nDetails: {ex.InnerException?.Message}", "Load Error", MessageBoxButton.OK, MessageBoxImage.Error); } }
        private void PrepareNewUser() { IsEditing = false; SelectedUser = null; ClearEntryFields(); }
        private bool CanEditOrDeleteUser() { return SelectedUser != null; }
        private void PrepareEditUser() { if (!CanEditOrDeleteUser() || SelectedUser == null) return; IsEditing = true; EntryUsername = SelectedUser.Username ?? string.Empty; EntryPassword = string.Empty; SelectedEntryRole = AvailableRoles.Contains(SelectedUser.Role ?? "") ? SelectedUser.Role : AvailableRoles.First(); }
        private bool CanSaveUser() { return !string.IsNullOrWhiteSpace(EntryUsername) && !string.IsNullOrWhiteSpace(SelectedEntryRole) && (IsEditing || !string.IsNullOrWhiteSpace(EntryPassword)); }

        // Changed to synchronous method (void, no async/await)
        private void SaveUser()
        {
            if (!CanSaveUser()) return;
            try
            {
                if (IsEditing && SelectedUser != null)
                {
                    User userToUpdate = new User { Id = SelectedUser.Id, Username = EntryUsername, Role = SelectedEntryRole!, PasswordHash = !string.IsNullOrWhiteSpace(EntryPassword) ? _userService.HashPassword(EntryPassword) : SelectedUser.PasswordHash };
                    _userService.UpdateUser(userToUpdate);
                    MessageBox.Show($"تم تعديل المستخدم: {userToUpdate.Username}", "نجاح التعديل", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    if (_userService.UserExists(EntryUsername)) { MessageBox.Show($"المستخدم '{EntryUsername}' موجود بالفعل.", "خطأ", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
                    var newUser = new User { Username = EntryUsername, PasswordHash = _userService.HashPassword(EntryPassword), Role = SelectedEntryRole! };
                    _userService.AddUser(newUser);
                    MessageBox.Show($"تمت إضافة المستخدم: {newUser.Username}", "نجاح الإضافة", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex) { MessageBox.Show($"حدث خطأ أثناء حفظ المستخدم: {ex.Message}", "خطأ الحفظ", MessageBoxButton.OK, MessageBoxImage.Error); }
            finally { LoadUsers(); CancelOperation(); }
        }

        // Changed to synchronous method (void, no async/await)
        private void DeleteUser()
        {
            if (!CanEditOrDeleteUser() || SelectedUser == null) return;
            if (string.Equals(SelectedUser.Username, "admin", StringComparison.OrdinalIgnoreCase)) { MessageBox.Show("لا يمكن حذف حساب المسؤول الرئيسي.", "غير مسموح", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
            if (MessageBox.Show($"هل أنت متأكد من حذف المستخدم: {SelectedUser.Username}؟", "تأكيد الحذف", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                try { _userService.DeleteUser(SelectedUser.Username!); MessageBox.Show($"تم حذف المستخدم: {SelectedUser.Username}", "نجاح الحذف", MessageBoxButton.OK, MessageBoxImage.Information); }
                catch (Exception ex) { MessageBox.Show($"حدث خطأ أثناء حذف المستخدم: {ex.Message}", "خطأ الحذف", MessageBoxButton.OK, MessageBoxImage.Error); }
                finally { LoadUsers(); CancelOperation(); }
            }
        }
        private void CancelOperation() { IsEditing = false; SelectedUser = null; ClearEntryFields(); }
        private void ClearEntryFields() { EntryUsername = string.Empty; EntryPassword = string.Empty; SelectedEntryRole = AvailableRoles.FirstOrDefault(); }
    }
}
// نهاية الكود لملف ViewModels/UsersManagementViewModel.cs