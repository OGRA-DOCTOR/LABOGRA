﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LABOGRA.Models;
using LABOGRA.Services.Database.Data; // مسار LabDbContext
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace LABOGRA.ViewModels
{
    public partial class UsersManagementViewModel : ObservableObject
    {
        private readonly LabDbContext _dbContext;

        [ObservableProperty]
        private ObservableCollection<User> _users = new ObservableCollection<User>();

        [ObservableProperty]
        private User? _selectedUser;

        [ObservableProperty]
        private string _entryUsername = string.Empty;

        [ObservableProperty]
        private string _entryPassword = string.Empty;

        [ObservableProperty]
        private string? _selectedEntryRole;

        [ObservableProperty]
        private bool _isEditing;

        public List<string> AvailableRoles { get; } = new List<string> { "Admin", "User" };

        public UsersManagementViewModel(LabDbContext dbContext)
        {
            _dbContext = dbContext;

            LoadUsersCommand = new AsyncRelayCommand(LoadUsersAsync);
            PrepareNewUserCommand = new RelayCommand(PrepareNewUser);
            PrepareEditUserCommand = new RelayCommand(PrepareEditUser, CanEditOrDeleteUser);
            SaveUserCommand = new AsyncRelayCommand(SaveUserAsync, CanSaveUser);
            DeleteUserCommand = new AsyncRelayCommand(DeleteUserAsync, CanEditOrDeleteUser);
            CancelCommand = new RelayCommand(CancelOperation);

            SelectedEntryRole = AvailableRoles.First(); // نضمن إنها دايمًا مش null

            LoadUsersCommand.Execute(null);
        }

        public IAsyncRelayCommand LoadUsersCommand { get; }
        public IRelayCommand PrepareNewUserCommand { get; }
        public IRelayCommand PrepareEditUserCommand { get; }
        public IAsyncRelayCommand SaveUserCommand { get; }
        public IAsyncRelayCommand DeleteUserCommand { get; }
        public IRelayCommand CancelCommand { get; }

        private async Task LoadUsersAsync()
        {
            Users.Clear();

            var usersFromDb = await _dbContext.Users.ToListAsync();

            if (!usersFromDb.Any())
            {
                var defaultAdmin = new User
                {
                    Username = "Admin",
                    PasswordHash = HashPassword("0000"),
                    Role = "Admin"
                };
                _dbContext.Users.Add(defaultAdmin);
                await _dbContext.SaveChangesAsync();

                usersFromDb.Add(defaultAdmin);
            }

            foreach (var user in usersFromDb)
                Users.Add(user);
        }

        private void PrepareNewUser()
        {
            IsEditing = false;
            SelectedUser = null;
            ClearEntryFields();
            SaveUserCommand.NotifyCanExecuteChanged();
        }

        private bool CanEditOrDeleteUser()
        {
            return SelectedUser != null;
        }

        private void PrepareEditUser()
        {
            if (!CanEditOrDeleteUser() || SelectedUser == null) return;

            IsEditing = true;
            EntryUsername = SelectedUser.Username ?? string.Empty;
            EntryPassword = string.Empty;
            SelectedEntryRole = SelectedUser.Role ?? AvailableRoles.First(); // ضمان قيمة افتراضية
            SaveUserCommand.NotifyCanExecuteChanged();
        }

        private bool CanSaveUser()
        {
            return !string.IsNullOrWhiteSpace(EntryUsername) &&
                   !string.IsNullOrWhiteSpace(SelectedEntryRole) &&
                   (IsEditing || !string.IsNullOrWhiteSpace(EntryPassword));
        }

        private async Task SaveUserAsync()
        {
            if (!CanSaveUser()) return;

            if (IsEditing && SelectedUser != null)
            {
                SelectedUser.Username = EntryUsername;
                SelectedUser.Role = SelectedEntryRole!;

                if (!string.IsNullOrWhiteSpace(EntryPassword))
                {
                    SelectedUser.PasswordHash = HashPassword(EntryPassword);
                }

                _dbContext.Users.Update(SelectedUser);
                await _dbContext.SaveChangesAsync();

                MessageBox.Show($"تم تعديل المستخدم: {SelectedUser.Username}", "نجاح التعديل", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                var newUser = new User
                {
                    Username = EntryUsername,
                    PasswordHash = HashPassword(EntryPassword),
                    Role = SelectedEntryRole!
                };

                _dbContext.Users.Add(newUser);
                await _dbContext.SaveChangesAsync();

                Users.Add(newUser);
                MessageBox.Show($"تمت إضافة المستخدم: {newUser.Username}", "نجاح الإضافة", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            await LoadUsersAsync();
            CancelOperation();
        }

        private async Task DeleteUserAsync()
        {
            if (!CanEditOrDeleteUser() || SelectedUser == null) return;

            if (SelectedUser.Username?.ToLower() == "admin")
            {
                MessageBox.Show("لا يمكن حذف حساب المسؤول الرئيسي.", "غير مسموح", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (MessageBox.Show($"هل أنت متأكد من حذف المستخدم: {SelectedUser.Username}؟", "تأكيد الحذف", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                _dbContext.Users.Remove(SelectedUser);
                await _dbContext.SaveChangesAsync();

                MessageBox.Show($"تم حذف المستخدم: {SelectedUser.Username}", "نجاح الحذف", MessageBoxButton.OK, MessageBoxImage.Information);
                await LoadUsersAsync();
                CancelOperation();
            }
        }

        private void CancelOperation()
        {
            IsEditing = false;
            SelectedUser = null;
            ClearEntryFields();
            SaveUserCommand.NotifyCanExecuteChanged();
            PrepareEditUserCommand.NotifyCanExecuteChanged();
            DeleteUserCommand.NotifyCanExecuteChanged();
        }

        private void ClearEntryFields()
        {
            EntryUsername = string.Empty;
            EntryPassword = string.Empty;
            SelectedEntryRole = AvailableRoles.First();
        }

        private string HashPassword(string password)
        {
            using (SHA256 sha = SHA256.Create())
            {
                byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                foreach (var b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }

        partial void OnEntryUsernameChanged(string value) => SaveUserCommand.NotifyCanExecuteChanged();
        partial void OnEntryPasswordChanged(string value) => SaveUserCommand.NotifyCanExecuteChanged();
        partial void OnSelectedEntryRoleChanged(string? value) => SaveUserCommand.NotifyCanExecuteChanged();

        partial void OnSelectedUserChanged(User? value)
        {
            PrepareEditUserCommand.NotifyCanExecuteChanged();
            DeleteUserCommand.NotifyCanExecuteChanged();
            if (value == null && IsEditing)
            {
                CancelOperation();
            }
        }

        partial void OnIsEditingChanged(bool value) => SaveUserCommand.NotifyCanExecuteChanged();
    }
}
