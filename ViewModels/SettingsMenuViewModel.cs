// SettingsMenuViewModel.cs - تفعيل التنقل إلى UsersManagementViewModel
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Windows;
// using LABOGRA.ViewModels; // غير ضروري لأننا داخل نفس الـ namespace

namespace LABOGRA.ViewModels
{
    public partial class SettingsMenuViewModel : ObservableObject
    {
        public event EventHandler<Type>? RequestNavigate;

        public List<string> SettingsOptions { get; } = new List<string>
        {
            "إدارة أنواع التحاليل",
            "إدارة المستخدمين"
        };

        [RelayCommand]
        private void NavigateToSetting(string? settingName)
        {
            if (string.IsNullOrEmpty(settingName))
            {
                return;
            }

            Type? targetViewModelType = null;

            switch (settingName)
            {
                case "إدارة أنواع التحاليل":
                    targetViewModelType = typeof(TestsManagementViewModel);
                    break;
                case "إدارة المستخدمين":
                    targetViewModelType = typeof(UsersManagementViewModel); // تم تفعيل هذا السطر
                    break;
                    // أضف هنا حالات أخرى لخيارات الإعدادات المستقبلية
            }

            if (targetViewModelType != null)
            {
                RequestNavigate?.Invoke(this, targetViewModelType);
            }
            else
            {
                MessageBox.Show($"الخيار '{settingName}' غير مدعوم حالياً أو لم يتم تحديد ViewModel له.", "خطأ في التنقل", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        public SettingsMenuViewModel()
        {
            // لا تغييرات هنا
        }
    }
}