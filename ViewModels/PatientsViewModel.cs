// نفس كود الإصدار 3 الذي تم تقديمه سابقاً لملف ViewModels\PatientsViewModel.cs
// لم يتم إجراء تغييرات تتطلب إصداراً جديداً في هذه المرحلة.
// الرجاء لصق محتوى الإصدار 3 هنا بالكامل.
// إليك المحتوى مجدداً للتسهيل:

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LABOGRA.Models;
using LABOGRA.Services.Database.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

// تحتاج للتأكد أنك قمت بتثبيت حزمة NuGet: CommunityToolkit.Mvvm

namespace LABOGRA.ViewModels
{
    public partial class PatientsViewModel : ObservableObject
    {
        private readonly LabDbContext _context;

        // الخصائص (Properties) لربط البيانات
        [ObservableProperty]
        private string selectedTitle = "السيد";

        [ObservableProperty]
        private string patientName = string.Empty;

        // خصائص RadioButtons للنوع (Gender)
        [ObservableProperty]
        private bool isMaleSelected = true;

        [ObservableProperty]
        private bool isFemaleSelected = false;

        // هذه الخاصية ستقوم بإرجاع القيمة الصحيحة للنوع بناءً على الـ RadioButtons
        public string Gender
        {
            get => IsMaleSelected ? "Male" : "Female";
            // لا نحتاج لـ setter في هذه الواجهة لأنها للإدخال فقط
            // set { ... }
        }

        [ObservableProperty]
        private int ageValueInput;

        [ObservableProperty]
        private string selectedAgeUnit = "سنة";

        [ObservableProperty]
        private string? medicalRecordNumber;

        [ObservableProperty]
        private string? phoneNumber;

        [ObservableProperty]
        private string? email;

        [ObservableProperty]
        private ReferringPhysician? selectedReferringPhysician;

        [ObservableProperty]
        private Test? selectedAvailableTest; // التحليل المختار في قائمة التحاليل المتاحة

        [ObservableProperty]
        private Test? selectedTestForRemoval; // التحليل المختار في قائمة التحاليل المختارة للحذف

        // مجموعات البيانات
        public ObservableCollection<string> Titles { get; } = new ObservableCollection<string>
        {
            "السيد", "السيدة", "الطفل", "الطفلة", "الأستاذ", "الأستاذة",
            "الحاج", "الحاجة", "الدكتور", "الدكتورة", "الآنسة", "مدام"
        };

        public ObservableCollection<string> AgeUnits { get; } = new ObservableCollection<string> { "يوم", "شهر", "سنة" };

        public ObservableCollection<ReferringPhysician> ReferringPhysicians { get; } = new ObservableCollection<ReferringPhysician>();

        public ObservableCollection<Test> AvailableTests { get; } = new ObservableCollection<Test>(); // ستبدأ فارغة حالياً إذا لم يتم إضافة تحاليل

        public ObservableCollection<Test> SelectedTests { get; } = new ObservableCollection<Test>();

        // الأوامر (Commands)
        [RelayCommand]
        private async Task SavePatientAsync()
        {
            // منطق التحقق من صحة البيانات الأساسية
            if (string.IsNullOrWhiteSpace(PatientName) || string.IsNullOrWhiteSpace(Gender) || AgeValueInput <= 0 || string.IsNullOrWhiteSpace(SelectedAgeUnit) || string.IsNullOrWhiteSpace(SelectedTitle))
            {
                MessageBox.Show("الرجاء إدخال البيانات الإلزامية: الاسم، اللقب، النوع، العمر ووحدته.", "خطأ في الإدخال", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (SelectedTests.Count == 0)
            {
                MessageBox.Show("الرجاء اختيار نوع تحليل واحد على الأقل للمريض.", "خطأ في الإدخال", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // إنشاء كائن Patient جديد وتعبئته بالبيانات
            var newPatient = new Patient
            {
                Title = SelectedTitle,
                Name = PatientName,
                Gender = Gender,
                AgeValue = AgeValueInput,
                AgeUnit = SelectedAgeUnit,
                MedicalRecordNumber = MedicalRecordNumber,
                PhoneNumber = PhoneNumber,
                Email = Email,
                RegistrationDateTime = DateTime.Now,
                ReferringPhysicianId = SelectedReferringPhysician?.Id
            };

            // إنشاء كائن LabOrder جديد لربطه بالمريض والتحاليل
            var labOrder = new LabOrder
            {
                RegistrationDateTime = newPatient.RegistrationDateTime,
                ReferringPhysicianId = newPatient.ReferringPhysicianId // يمكن استخدام نفس الطبيب المحول للمريض أو اختيار طبيب خاص بالطلب
            };

            // إضافة عناصر LabOrderItem للطلب بناءً على التحاليل التي تم اختيارها
            foreach (var test in SelectedTests)
            {
                labOrder.Items.Add(new LabOrderItem
                {
                    TestId = test.Id,
                    IsPrinted = false
                });
            }

            // ربط الطلب بالمريض
            newPatient.LabOrders.Add(labOrder);

            try
            {
                _context.Patients.Add(newPatient);
                await _context.SaveChangesAsync();

                // إضافة اللقب الجديد إلى القائمة إذا لم يكن موجوداً
                // هذه العملية بسيطة جداً حالياً وتحفظ اللقب في الذاكرة فقط للـ ViewModel الحالي
                // لكي يتم حفظها بين مرات تشغيل البرنامج، نحتاج لآلية حفظ وتحميل للألقاب
                if (!Titles.Contains(SelectedTitle))
                {
                    Titles.Add(SelectedTitle);
                }


                MessageBox.Show("تم حفظ بيانات المريض وتحاليله بنجاح!", "نجاح", MessageBoxButton.OK, MessageBoxImage.Information);

                ResetForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء حفظ البيانات: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void AddSelectedTest()
        {
            // إضافة التحليل المختار من قائمة التحاليل المتاحة إلى قائمة التحاليل المختارة.
            // يتم التحقق للتأكد من أن العنصر المحدد ليس Null وأنه غير موجود بالفعل في القائمة المختارة.
            if (SelectedAvailableTest != null && !SelectedTests.Any(t => t.Id == SelectedAvailableTest.Id))
            {
                SelectedTests.Add(SelectedAvailableTest);
            }
        }

        [RelayCommand]
        private void RemoveSelectedTest()
        {
            // إزالة التحليل المختار من قائمة التحاليل المختارة.
            // يتم التحقق للتأكد من أن العنصر المحدد ليس Null.
            if (SelectedTestForRemoval != null)
            {
                SelectedTests.Remove(SelectedTestForRemoval);
                SelectedTestForRemoval = null; // مسح التحديد في الواجهة بعد الإزالة
            }
        }

        [RelayCommand]
        private void AddAllTests()
        {
            // إضافة جميع التحاليل المتاحة إلى قائمة التحاليل المختارة.
            // يمكن مسح القائمة المختارة الحالية قبل الإضافة أو إضافة العناصر غير الموجودة فقط.
            // هنا نقوم بمسح القائمة وإضافة الكل.
            SelectedTests.Clear();
            foreach (var test in AvailableTests)
            {
                SelectedTests.Add(test);
            }
        }

        // مُنشئ الـ ViewModel
        public PatientsViewModel()
        {
            var contextFactory = new LabDbContextFactory();
            _context = contextFactory.CreateDbContext(Array.Empty<string>());

            LoadInitialData();
        }

        // دالة لتحميل البيانات الأولية (الأطباء والتحاليل)
        private async void LoadInitialData()
        {
            try
            {
                // تحميل قائمة الأطباء المحولين
                var physicians = await _context.ReferringPhysicians.ToListAsync();
                ReferringPhysicians.Clear();
                foreach (var physician in physicians)
                {
                    ReferringPhysicians.Add(physician);
                }

                // تحميل قائمة أنواع التحاليل المتاحة
                // هذه القائمة ستكون فارغة إذا لم يتم إضافة تحاليل للجدول بعد
                var tests = await _context.Tests.ToListAsync();
                AvailableTests.Clear();
                foreach (var test in tests)
                {
                    AvailableTests.Add(test);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء تحميل البيانات الأولية: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // دالة لإعادة تهيئة النموذج بعد الحفظ الناجح
        private void ResetForm()
        {
            SelectedTitle = "السيد";
            PatientName = string.Empty;
            IsMaleSelected = true;
            IsFemaleSelected = false;
            AgeValueInput = 0;
            SelectedAgeUnit = "سنة";
            MedicalRecordNumber = null;
            PhoneNumber = null;
            Email = null;
            SelectedReferringPhysician = null;
            SelectedAvailableTest = null;
            SelectedTestForRemoval = null;
            SelectedTests.Clear();
        }
    }
}