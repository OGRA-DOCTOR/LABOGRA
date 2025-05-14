// اسم الملف: PatientsViewModel.cs - الإصدار المصحح (حل أخطاء LINQ و AlsoNotifyChangeFor ودعم Unknown Gender ووحدات العمر الإنجليزية)

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LABOGRA.Models;
using LABOGRA.Services.Database.Data;
using Microsoft.EntityFrameworkCore; // تأكد من وجود هذا using
using System;
using System.Collections.ObjectModel;
using System.Linq; // تأكد من وجود هذا using لـ LINQ
using System.Threading.Tasks;
using System.Windows;

namespace LABOGRA.ViewModels
{
    // يجب أن يكون الكلاس جزئياً (partial) للسماح لمولِّد الأكواد بالعمل
    public partial class PatientsViewModel : ObservableObject
    {
        private readonly LabDbContextFactory _contextFactory;

        [ObservableProperty] private string selectedTitle = "السيد"; // اللقب لا يزال بالعربية
        [ObservableProperty] private string patientName = string.Empty;

        // خصائص اختيار النوع (Gender)
        // [ObservableProperty] تنشئ backing field و property كاملة و partial methods (مثل OnIsMaleSelectedChanged)
        // نعتمد هنا على الـ partial methods لإرسال إشعار تغيير Gender يدوياً
        [ObservableProperty] private bool isMaleSelected = true;
        [ObservableProperty] private bool isFemaleSelected = false;
        [ObservableProperty] private bool isUnknownSelected = false; // خاصية لـ "Unknown"

        // تعريف الأجزاء الجزئية (partial methods) التي ينشئها ObservableProperty
        // هنا نضيف المنطق الذي سيتم تنفيذه بعد كل تغيير في الخاصية المقابلة
        partial void OnIsMaleSelectedChanged(bool oldValue, bool newValue)
        {
            // عندما يتغير اختيار الذكر، أرسل إشعاراً بتغير خاصية Gender
            OnPropertyChanged(nameof(Gender));
        }

        partial void OnIsFemaleSelectedChanged(bool oldValue, bool newValue)
        {
            // عندما يتغير اختيار الأنثى، أرسل إشعاراً بتغير خاصية Gender
            OnPropertyChanged(nameof(Gender));
        }

        partial void OnIsUnknownSelectedChanged(bool oldValue, bool newValue)
        {
            // عندما يتغير اختيار غير محدد، أرسل إشعاراً بتغير خاصية Gender
            OnPropertyChanged(nameof(Gender));
        }


        // الخاصية المحسوبة لتمثيل النوع كـ String
        public string Gender
        {
            get
            {
                if (IsMaleSelected) return "Male";
                if (IsFemaleSelected) return "Female";
                return "Unknown"; // قيمة "غير محدد" بالإنجليزية
            }
        }

        [ObservableProperty] private int ageValueInput;
        [ObservableProperty] private string selectedAgeUnit = "Year"; // القيمة الافتراضية بالإنجليزية

        [ObservableProperty] private string? medicalRecordNumber;
        [ObservableProperty] private string? phoneNumber;
        [ObservableProperty] private string? email;
        [ObservableProperty] private string? address; // موجود في النموذج

        [ObservableProperty] private ReferringPhysician? selectedReferringPhysician;
        [ObservableProperty] private string? customReferringPhysicianName;

        [ObservableProperty] private string? displayGeneratedId;

        [ObservableProperty] private Test? selectedAvailableTest;
        [ObservableProperty] private Test? selectedTestForRemoval;

        // قائمة الألقاب (بالعربية)
        public ObservableCollection<string> Titles { get; } = new()
        {
            "السيد", "السيدة", "الطفل", "الطفلة", "الأستاذ", "الأستاذة", "الحاج", "الحاجة",
            "الدكتور", "الدكتورة", "الآنسة", "مدام"
        };

        // قائمة وحدات العمر (بالإنجليزية)
        public ObservableCollection<string> AgeUnits { get; } = new() { "Day", "Month", "Year" };

        // قوائم التحاليل والأطباء
        public ObservableCollection<ReferringPhysician> ReferringPhysicians { get; } = new();
        public ObservableCollection<Test> AvailableTests { get; } = new();
        public ObservableCollection<Test> SelectedTests { get; } = new();

        public PatientsViewModel()
        {
            _contextFactory = new LabDbContextFactory();
            // تهيئة اختيار النوع الافتراضي (واحد فقط True)
            IsMaleSelected = true;
            IsFemaleSelected = false;
            IsUnknownSelected = false;
            // تهيئة وحدة العمر الافتراضية
            SelectedAgeUnit = AgeUnits.FirstOrDefault(u => u == "Year") ?? AgeUnits.FirstOrDefault() ?? "Year";

            // تحميل البيانات الأولية بشكل غير متزامن
            _ = LoadInitialDataAsync();
        }

        private async Task LoadInitialDataAsync()
        {
            try
            {
                using var context = _contextFactory.CreateDbContext(Array.Empty<string>());

                var physicians = await context.ReferringPhysicians.OrderBy(p => p.Name).ToListAsync();
                ReferringPhysicians.Clear();
                foreach (var physician in physicians) ReferringPhysicians.Add(physician);

                var tests = await context.Tests.OrderBy(t => t.Name).ToListAsync();
                AvailableTests.Clear();
                foreach (var test in tests) AvailableTests.Add(test);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء تحميل البيانات: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task<string> GenerateNewPatientIdAsync(LabDbContext context)
        {
            var today = DateTime.Today;
            var datePart = today.ToString("yyyyMMdd");

            // البحث عن آخر GeneratedId يبدأ بتاريخ اليوم
            var latest = await context.Patients
                .Where(p => p.GeneratedId != null && p.GeneratedId.StartsWith(datePart))
                .OrderByDescending(p => p.GeneratedId)
                .FirstOrDefaultAsync();

            int next = 1;
            if (latest?.GeneratedId != null)
            {
                // استخلاص الجزء التسلسلي بعد التاريخ
                string seqPart = latest.GeneratedId[datePart.Length..];
                if (int.TryParse(seqPart, out int lastSeq))
                {
                    next = lastSeq + 1;
                }
            }

            // تنسيق ID الجديد (التاريخ + رقم تسلسلي من خانتين على الأقل)
            return $"{datePart}{next:D2}";
        }


        // أوامر إضافة/حذف التحاليل (لم تتغير)
        [RelayCommand]
        private void AddSelectedTest()
        {
            if (SelectedAvailableTest != null && !SelectedTests.Contains(SelectedAvailableTest))
                SelectedTests.Add(SelectedAvailableTest);
        }

        [RelayCommand]
        private void RemoveSelectedTest()
        {
            if (SelectedTestForRemoval != null)
                SelectedTests.Remove(SelectedTestForRemoval);
        }

        [RelayCommand]
        private void AddAllTests()
        {
            foreach (var test in AvailableTests)
                if (!SelectedTests.Contains(test)) SelectedTests.Add(test);
        }

        // أمر حفظ المريض والتحاليل
        [RelayCommand]
        private async Task SavePatientAsync()
        {
            // التحقق من الحقول المطلوبة (الاسم، قيمة العمر، وحدة العمر)
            if (string.IsNullOrWhiteSpace(PatientName) || AgeValueInput <= 0 || string.IsNullOrWhiteSpace(SelectedAgeUnit))
            {
                MessageBox.Show("يرجى إدخال الاسم والعمر (القيمة والوحدة).", "تحذير", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (SelectedTests.Count == 0)
            {
                MessageBox.Show("يرجى اختيار تحليل واحد على الأقل.", "تحذير", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            using var context = _contextFactory.CreateDbContext(Array.Empty<string>());
            ReferringPhysician? physician = SelectedReferringPhysician;

            // منطق حفظ الطبيب المحول الجديد إذا تم إدخال اسم مخصص
            if (physician == null && !string.IsNullOrWhiteSpace(CustomReferringPhysicianName))
            {
                // البحث عن طبيب بنفس الاسم (غير حساس لحالة الأحرف) في الذاكرة بعد جلب البيانات
                // هذا يحل خطأ الترجمة System.InvalidOperationException
                var trimmedCustomName = CustomReferringPhysicianName.Trim(); // نقوم بعمل Trim على المدخلات مرة واحدة

                // جلب الأطباء إلى الذاكرة ثم البحث في الذاكرة
                // نستخدم AsEnumerable() لجلب كل البيانات إلى الذاكرة أولاً
                physician = context.ReferringPhysicians
                                   .AsEnumerable() // يتم هنا جلب جميع الأطباء إلى الذاكرة
                                   .FirstOrDefault(rp => rp.Name != null && rp.Name.Trim().Equals(trimmedCustomName, StringComparison.OrdinalIgnoreCase)); // البحث في الذاكرة باستخدام LINQ to Objects


                if (physician == null)
                {
                    // لم يتم العثور عليه، قم بإنشاء طبيب جديد
                    physician = new ReferringPhysician { Name = trimmedCustomName }; // استخدم الاسم المقصوص
                    context.ReferringPhysicians.Add(physician);

                    try
                    {
                        await context.SaveChangesAsync();

                        // إضافة الطبيب الجديد إلى قائمة الأطباء في الـ ViewModel وفرز القائمة
                        // (هذه الخطوة مهمة إذا كنت تسمح بإدخال طبيب جديد وتتوقع ظهوره في القائمة المنسدلة فوراً)
                        if (!ReferringPhysicians.Any(rp => rp.Id == physician.Id))
                        {
                            ReferringPhysicians.Add(physician);
                            var sorted = ReferringPhysicians.OrderBy(rp => rp.Name).ToList();
                            ReferringPhysicians.Clear();
                            foreach (var p in sorted) ReferringPhysicians.Add(p);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"خطأ في حفظ الطبيب المحول: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                        // يمكن اختيار التوقف هنا أو الاستمرار بدون ربط المريض بالطبيب إذا كان الحفظ ضرورياً
                        return; // نوقف العملية هنا إذا فشل حفظ الطبيب المحول
                    }
                }
                // إذا تم العثور على طبيب موجود، physician يحتوي الآن على الكائن الموجود
            }

            // توليد ID جديد للمريض
            string newId = await GenerateNewPatientIdAsync(context);

            // إنشاء كائن Patient جديد
            var newPatient = new Patient
            {
                GeneratedId = newId,
                Title = SelectedTitle,
                Name = PatientName.Trim(), // إزالة المسافات الزائدة
                MedicalRecordNumber = MedicalRecordNumber?.Trim(), // إزالة المسافات الزائدة إذا لم يكن null
                Email = Email?.Trim(),
                Address = Address?.Trim(), // Address موجود في النموذج ولكنه غير ظاهر في الواجهة الحالية
                Gender = Gender, // استخدام الخاصية المحسوبة التي تعكس الاختيار بالإنجليزية
                AgeValue = AgeValueInput,
                AgeUnit = SelectedAgeUnit, // استخدام وحدة العمر بالإنجليزية
                PhoneNumber = PhoneNumber?.Trim(),
                RegistrationDateTime = DateTime.Now,
                ReferringPhysicianId = physician?.Id // ربط المريض بالطبيب المحول (إن وجد)
            };

            // إنشاء طلب تحليل (LabOrderA)
            var labOrder = new LabOrderA
            {
                RegistrationDateTime = newPatient.RegistrationDateTime,
                ReferringPhysicianId = physician?.Id // يمكن أيضاً ربط الطلب بالطبيب المحول
            };

            // إضافة التحاليل المختارة إلى طلب التحليل
            foreach (var test in SelectedTests)
            {
                labOrder.Items.Add(new LabOrderItem { TestId = test.Id, IsPrinted = false });
            }

            // إضافة طلب التحليل إلى المريض
            newPatient.LabOrders.Add(labOrder);

            try
            {
                // إضافة المريض الجديد إلى قاعدة البيانات
                context.Patients.Add(newPatient);
                // حفظ التغييرات
                await context.SaveChangesAsync();

                // تحديث قائمة الأطباء في الواجهة بعد الحفظ إذا تم إضافة طبيب جديد
                if (physician != null && !ReferringPhysicians.Any(rp => rp.Id == physician.Id))
                {
                    ReferringPhysicians.Add(physician);
                    var sorted = ReferringPhysicians.OrderBy(rp => rp.Name).ToList();
                    ReferringPhysicians.Clear();
                    foreach (var p in sorted) ReferringPhysicians.Add(p);
                }

                // إعادة تعيين SelectedReferringPhysician إلى الكائن الصحيح من القائمة المحدثة
                var matchingPhysician = ReferringPhysicians.FirstOrDefault(rp => rp.Id == physician?.Id);
                if (matchingPhysician != null)
                    SelectedReferringPhysician = matchingPhysician;


                // إضافة اللقب الجديد إلى قائمة الألقاب في الواجهة إذا تم إدخاله في ComboBox المفتوح
                if (!string.IsNullOrWhiteSpace(SelectedTitle) && !Titles.Contains(SelectedTitle))
                {
                    Titles.Add(SelectedTitle);
                    // يمكن فرز القائمة هنا إذا أردت أن يظهر اللقب الجديد في الترتيب الصحيح
                    var sortedTitles = Titles.OrderBy(t => t).ToList();
                    Titles.Clear();
                    foreach (var title in sortedTitles) Titles.Add(title);
                    SelectedTitle = newPatient.Title; // إعادة تعيين القيمة المختارة
                }


                // عرض الـ Generated ID بعد الحفظ
                DisplayGeneratedId = newId;

                // رسالة نجاح
                MessageBox.Show($"تم حفظ بيانات المريض بنجاح.\nID: {newId}", "تم الحفظ", MessageBoxButton.OK, MessageBoxImage.Information);

                // إعادة تهيئة النموذج لحالة إدخال مريض جديد
                ResetForm();
            }
            catch (DbUpdateException dbEx)
            {
                // معالجة أخطاء قاعدة البيانات
                MessageBox.Show($"خطأ في قاعدة البيانات: {dbEx.InnerException?.Message ?? dbEx.Message}", "DB Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                // معالجة الأخطاء العامة الأخرى
                MessageBox.Show($"خطأ غير متوقع: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // دالة إعادة تهيئة النموذج
        private void ResetForm()
        {
            SelectedTitle = Titles.FirstOrDefault() ?? "السيد";
            PatientName = string.Empty;

            // إعادة تعيين اختيارات النوع (واحد فقط True)
            IsMaleSelected = true;
            IsFemaleSelected = false;
            IsUnknownSelected = false;

            AgeValueInput = 0;
            SelectedAgeUnit = AgeUnits.FirstOrDefault(u => u == "Year") ?? AgeUnits.FirstOrDefault() ?? "Year"; // إعادة تعيين وحدة العمر بالإنجليزية

            MedicalRecordNumber = null;
            PhoneNumber = null;
            Email = null;
            Address = null;
            SelectedReferringPhysician = null;
            CustomReferringPhysicianName = null;
            SelectedAvailableTest = null;
            SelectedTestForRemoval = null;
            SelectedTests.Clear();
            DisplayGeneratedId = null; // إخفاء ID النظام حتى يتم حفظ مريض جديد
        }
    }
}