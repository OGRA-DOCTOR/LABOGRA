// الإصدار: 1 (لهذا الملف)
// اسم الملف: SettingsMenuViewModel.cs
// الوصف: ViewModel لقائمة خيارات الإعدادات.
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Windows; // لاستخدام MessageBox مؤقتاً
using LABOGRA.ViewModels; // لتضمين ViewModels الأخرى

namespace LABOGRA.ViewModels
{
    // لا يوجد مجلد فرعي ViewModels/Settings، الفئة مباشرة تحت ViewModels

    public partial class SettingsMenuViewModel : ObservableObject
    {
        // حدث لإخبار الـ View (أو أي مشترك آخر) بنوع الـ ViewModel الذي يجب عرضه
        public event EventHandler<Type>? RequestNavigate;

        // قائمة بخيارات الإعدادات التي ستظهر للمستخدم
        public List<string> SettingsOptions { get; } = new List<string>
        {
            "إدارة أنواع التحاليل",
            // أضف خيارات إعدادات أخرى هنا لاحقاً
            // "إدارة المستخدمين",
            // "إدارة التقارير",
            // "إعدادات الطباعة",
            // ...
        };

        [RelayCommand]
        private void NavigateToSetting(string? settingName)
        {
            if (string.IsNullOrEmpty(settingName))
            {
                return;
            }

            Type? targetViewModelType = null;

            // تحديد نوع الـ ViewModel المطلوب بناءً على اسم الخيار المحدد
            switch (settingName)
            {
                case "إدارة أنواع التحاليل":
                    targetViewModelType = typeof(TestsManagementViewModel);
                    break;
                    // أضف هنا حالات أخرى لخيارات الإعدادات المستقبلية
                    // case "إدارة المستخدمين":
                    //     targetViewModelType = typeof(UsersManagementViewModel); // افترض وجود UsersManagementViewModel
                    //     break;
                    // case "إدارة التقارير":
                    //     targetViewModelType = typeof(ReportsSettingsViewModel); // افترض وجود ReportsSettingsViewModel
                    //     break;
                    // ...
            }

            // إذا تم تحديد نوع ViewModel مستهدف، قم بإطلاق الحدث
            if (targetViewModelType != null)
            {
                RequestNavigate?.Invoke(this, targetViewModelType);
            }
            else
            {
                // رسالة خطأ إذا كان الخيار غير معروف (للتطوير)
                MessageBox.Show($"الخيار '{settingName}' غير مدعوم حالياً.", "خطأ في التنقل", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        public SettingsMenuViewModel()
        {
            // يمكن هنا تحميل الخيارات ديناميكياً إذا لزم الأمر، لكن حالياً هي ثابتة.
        }
    }
}