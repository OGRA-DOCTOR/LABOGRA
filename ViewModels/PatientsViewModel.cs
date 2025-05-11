using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LABOGRA.Models;
using LABOGRA.Services.Database.Data; // تأكد أن هذا هو المسار الصحيح لـ LabDbContextFactory
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace LABOGRA.ViewModels
{
    public partial class PatientsViewModel : ObservableObject
    {
        private readonly LabDbContext _context;

        // --- خصائص بيانات المريض ---
        [ObservableProperty]
        private string selectedTitle = "السيد";

        [ObservableProperty]
        private string patientName = string.Empty;

        [ObservableProperty]
        private bool isMaleSelected = true;

        [ObservableProperty]
        private bool isFemaleSelected = false;

        public string Gender => IsMaleSelected ? "Male" : "Female";

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
        private string? address;

        [ObservableProperty]
        private ReferringPhysician? selectedReferringPhysician;

        [ObservableProperty]
        private string? customReferringPhysicianName; // اسم الطبيب المحول المخصص

        // --- خصائص متعلقة بالتحاليل ---
        [ObservableProperty]
        private Test? selectedAvailableTest; // التحليل المختار من قائمة التحاليل المتاحة

        [ObservableProperty]
        private Test? selectedTestForRemoval; // التحليل المختار من قائمة التحاليل المختارة (للحذف)

        // --- مجموعات البيانات للواجهة ---
        public ObservableCollection<string> Titles { get; } = new ObservableCollection<string>
        {
            "السيد", "السيدة", "الطفل", "الطفلة", "الأستاذ", "الأستاذة",
            "الحاج", "الحاجة", "الدكتور", "الدكتورة", "الآنسة", "مدام"
        };

        public ObservableCollection<string> AgeUnits { get; } = new ObservableCollection<string> { "يوم", "شهر", "سنة" };
        public ObservableCollection<ReferringPhysician> ReferringPhysicians { get; } = new ObservableCollection<ReferringPhysician>();
        public ObservableCollection<Test> AvailableTests { get; } = new ObservableCollection<Test>();
        public ObservableCollection<Test> SelectedTests { get; } = new ObservableCollection<Test>(); // قائمة التحاليل المختارة للمريض

        public PatientsViewModel()
        {
            // تأكد من أن LabDbContextFactory موجودة في المسار الصحيح ويمكن الوصول إليها
            var contextFactory = new LabDbContextFactory();
            _context = contextFactory.CreateDbContext(Array.Empty<string>());
            LoadInitialData();
        }

        private async void LoadInitialData()
        {
            try
            {
                var physicians = await _context.ReferringPhysicians.OrderBy(p => p.Name).ToListAsync();
                ReferringPhysicians.Clear();
                foreach (var physician in physicians)
                {
                    ReferringPhysicians.Add(physician);
                }

                var tests = await _context.Tests.OrderBy(t => t.Name).ToListAsync();
                AvailableTests.Clear();
                foreach (var test in tests)
                {
                    AvailableTests.Add(test);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء تحميل البيانات الأولية: {ex.Message}", "خطأ تحميل البيانات", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void AddSelectedTest()
        {
            if (SelectedAvailableTest != null)
            {
                // تحقق إذا كان التحليل غير موجود بالفعل في القائمة المختارة
                if (!SelectedTests.Contains(SelectedAvailableTest))
                {
                    SelectedTests.Add(SelectedAvailableTest);
                }
                // اختيارياً: يمكنك إلغاء تحديد العنصر في قائمة التحاليل المتاحة بعد إضافته
                // SelectedAvailableTest = null; // عادةً الـ ListView يهتم بهذا إذا كان Binding ثنائي الاتجاه
            }
            else
            {
                // يمكنك عرض رسالة هنا إذا كنت ترغب، ولكن الأمر سيعمل بدونها
                // MessageBox.Show("الرجاء اختيار تحليل من القائمة المتاحة أولاً.", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        [RelayCommand]
        private void RemoveSelectedTest()
        {
            if (SelectedTestForRemoval != null)
            {
                SelectedTests.Remove(SelectedTestForRemoval);
                // اختيارياً: يمكنك إلغاء تحديد العنصر بعد حذفه
                // SelectedTestForRemoval = null;
            }
            else
            {
                // MessageBox.Show("الرجاء اختيار تحليل من القائمة المختارة لحذفه.", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        [RelayCommand]
        private void AddAllTests()
        {
            foreach (var test in AvailableTests)
            {
                if (!SelectedTests.Contains(test))
                {
                    SelectedTests.Add(test);
                }
            }
        }

        [RelayCommand]
        private async Task SavePatientAsync()
        {
            if (string.IsNullOrWhiteSpace(PatientName) || string.IsNullOrWhiteSpace(SelectedTitle) || string.IsNullOrWhiteSpace(Gender) || AgeValueInput <= 0 || string.IsNullOrWhiteSpace(SelectedAgeUnit))
            {
                MessageBox.Show("الرجاء إدخال البيانات الإلزامية: اللقب، الاسم، النوع، العمر ووحدته.", "خطأ في الإدخال", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (SelectedTests.Count == 0) // هذا التحقق هو الذي كان يظهر لك المشكلة
            {
                MessageBox.Show("الرجاء اختيار نوع تحليل واحد على الأقل للمريض.", "خطأ في الإدخال", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            ReferringPhysician? physicianToSave = SelectedReferringPhysician;
            if (physicianToSave == null && !string.IsNullOrWhiteSpace(CustomReferringPhysicianName))
            {
                physicianToSave = await _context.ReferringPhysicians
                                          .FirstOrDefaultAsync(rp => rp.Name == CustomReferringPhysicianName.Trim());
                if (physicianToSave == null)
                {
                    physicianToSave = new ReferringPhysician { Name = CustomReferringPhysicianName.Trim() };
                    _context.ReferringPhysicians.Add(physicianToSave);
                }
            }

            var newPatient = new Patient
            {
                Title = SelectedTitle,
                Name = PatientName,
                MedicalRecordNumber = MedicalRecordNumber,
                Email = Email,
                Address = Address,
                Gender = Gender,
                AgeValue = AgeValueInput,
                AgeUnit = SelectedAgeUnit,
                PhoneNumber = PhoneNumber,
                RegistrationDateTime = DateTime.Now,
                ReferringPhysicianId = physicianToSave?.Id,
                ReferringPhysician = physicianToSave
            };

            // افترضت أن اسم الموديل هو LabOrderA كما هو موجود في كودك الأصلي
            var labOrder = new LabOrderA
            {
                RegistrationDateTime = newPatient.RegistrationDateTime,
                ReferringPhysicianId = newPatient.ReferringPhysicianId,
                ReferringPhysician = physicianToSave
            };

            foreach (var test in SelectedTests)
            {
                labOrder.Items.Add(new LabOrderItem
                {
                    TestId = test.Id,
                    Test = test, // يفضل إضافة الكائن Test نفسه أيضاً إذا كان النموذج يدعم ذلك للوصول السريع
                    IsPrinted = false
                    // Result, ResultUnit, ReferenceRange سيتم تعبئتهم لاحقاً عند إدخال النتائج
                });
            }

            newPatient.LabOrders.Add(labOrder); // تأكد أن Patient.LabOrders هي ICollection<LabOrderA>

            try
            {
                _context.Patients.Add(newPatient);
                await _context.SaveChangesAsync();

                if (physicianToSave != null && !ReferringPhysicians.Any(rp => rp.Id == physicianToSave.Id && physicianToSave.Id != 0))
                {
                    ReferringPhysicians.Add(physicianToSave);
                    SelectedReferringPhysician = physicianToSave;
                }

                if (!Titles.Contains(SelectedTitle))
                {
                    Titles.Add(SelectedTitle);
                }

                MessageBox.Show("تم حفظ بيانات المريض وتحاليله بنجاح!", "نجاح", MessageBoxButton.OK, MessageBoxImage.Information);
                ResetForm();
            }
            catch (DbUpdateException dbEx)
            {
                var innermostException = dbEx.InnerException ?? dbEx;
                MessageBox.Show($"حدث خطأ في قاعدة البيانات أثناء حفظ البيانات: {innermostException.Message}\nتأكد أن جميع الأعمدة المطلوبة موجودة في قاعدة البيانات وأن العلاقات صحيحة.", "خطأ قاعدة بيانات", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء حفظ البيانات: {ex.Message}", "خطأ عام", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ResetForm()
        {
            SelectedTitle = Titles.FirstOrDefault() ?? "السيد";
            PatientName = string.Empty;
            IsMaleSelected = true;
            IsFemaleSelected = false;
            AgeValueInput = 0;
            SelectedAgeUnit = AgeUnits.FirstOrDefault(u => u == "سنة") ?? AgeUnits.FirstOrDefault();
            MedicalRecordNumber = null;
            PhoneNumber = null;
            Email = null;
            Address = null;
            SelectedReferringPhysician = null;
            CustomReferringPhysicianName = null;
            SelectedAvailableTest = null;
            SelectedTestForRemoval = null;
            SelectedTests.Clear(); // مهم جداً لمسح قائمة التحاليل المختارة للمريض الجديد
        }
    }
}