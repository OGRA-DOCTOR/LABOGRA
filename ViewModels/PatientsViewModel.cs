// بداية الكود لملف ViewModels/PatientsViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LABOGRA.Models;
using LABOGRA.Services.Database.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics; // تأكد من وجود هذا السطر
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace LABOGRA.ViewModels
{
    public partial class PatientsViewModel : ObservableObject
    {
        private readonly LabDbContext _dbContext;

        [ObservableProperty] private string selectedTitle = "السيد";
        [ObservableProperty] private string patientName = string.Empty;
        [ObservableProperty] private bool isMaleSelected = true;
        [ObservableProperty] private bool isFemaleSelected = false;
        [ObservableProperty] private bool isUnknownSelected = false;

        partial void OnIsMaleSelectedChanged(bool oldValue, bool newValue) => OnPropertyChanged(nameof(Gender));
        partial void OnIsFemaleSelectedChanged(bool oldValue, bool newValue) => OnPropertyChanged(nameof(Gender));
        partial void OnIsUnknownSelectedChanged(bool oldValue, bool newValue) => OnPropertyChanged(nameof(Gender));

        public string Gender => IsMaleSelected ? "Male" : (IsFemaleSelected ? "Female" : "Unknown");

        [ObservableProperty] private int ageValueInput;
        [ObservableProperty] private string selectedAgeUnit = "Year";
        [ObservableProperty] private string? medicalRecordNumber;
        [ObservableProperty] private string? phoneNumber;
        [ObservableProperty] private string? email;
        [ObservableProperty] private string? address;
        [ObservableProperty] private ReferringPhysician? selectedReferringPhysician;
        [ObservableProperty] private string? customReferringPhysicianName;
        [ObservableProperty] private string? displayGeneratedId;
        [ObservableProperty] private Test? selectedAvailableTest;
        [ObservableProperty] private Test? selectedTestForRemoval;

        public ObservableCollection<string> Titles { get; } = new() { "السيد", "السيدة", "الطفل", "الطفلة", "الأستاذ", "الأستاذة", "الحاج", "الحاجة", "الدكتور", "الدكتورة", "الآنسة", "مدام" };
        public ObservableCollection<string> AgeUnits { get; } = new() { "Day", "Month", "Year" };
        public ObservableCollection<ReferringPhysician> ReferringPhysicians { get; } = new();
        public ObservableCollection<Test> AvailableTests { get; } = new();
        public ObservableCollection<Test> SelectedTests { get; } = new();

        public PatientsViewModel(LabDbContext dbContext)
        {
            _dbContext = dbContext;
            IsMaleSelected = true;
            IsFemaleSelected = false;
            IsUnknownSelected = false;
            SelectedAgeUnit = AgeUnits.FirstOrDefault(u => u == "Year") ?? AgeUnits.First();
            _ = LoadInitialDataAsync();
        }

        private async Task LoadInitialDataAsync()
        {
            try
            {
                var physicians = await _dbContext.ReferringPhysicians.OrderBy(p => p.Name).ToListAsync();
                ReferringPhysicians.Clear();
                foreach (var physician in physicians) ReferringPhysicians.Add(physician);

                var tests = await _dbContext.Tests.OrderBy(t => t.Name).ToListAsync();
                AvailableTests.Clear();
                foreach (var test in tests) AvailableTests.Add(test);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء تحميل البيانات: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task<string> GenerateNewPatientIdAsync()
        {
            var today = DateTime.Today;
            var datePart = today.ToString("yyyyMMdd");
            var latest = await _dbContext.Patients
                .Where(p => p.GeneratedId != null && p.GeneratedId.StartsWith(datePart))
                .OrderByDescending(p => p.GeneratedId)
                .FirstOrDefaultAsync();
            int next = 1;
            if (latest?.GeneratedId != null)
            {
                string seqPart = latest.GeneratedId[datePart.Length..];
                if (int.TryParse(seqPart, out int lastSeq)) next = lastSeq + 1;
            }
            return $"{datePart}{next:D2}";
        }

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

        [RelayCommand]
        private async Task SavePatientAsync()
        {
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

            ReferringPhysician? physicianToSave = SelectedReferringPhysician;

            if (physicianToSave == null && !string.IsNullOrWhiteSpace(CustomReferringPhysicianName))
            {
                var trimmedCustomName = CustomReferringPhysicianName.Trim();
                physicianToSave = await _dbContext.ReferringPhysicians
                                   .FirstOrDefaultAsync(rp => rp.Name.ToLower() == trimmedCustomName.ToLower());
                if (physicianToSave == null)
                {
                    physicianToSave = new ReferringPhysician { Name = trimmedCustomName };
                    _dbContext.ReferringPhysicians.Add(physicianToSave);
                }
            }

            string newGeneratedId = await GenerateNewPatientIdAsync();

            var newPatient = new Patient
            {
                GeneratedId = newGeneratedId,
                Title = SelectedTitle,
                Name = PatientName.Trim(),
                MedicalRecordNumber = MedicalRecordNumber?.Trim(),
                Email = Email?.Trim(),
                Address = Address?.Trim(),
                Gender = Gender,
                AgeValue = AgeValueInput,
                AgeUnit = SelectedAgeUnit,
                PhoneNumber = PhoneNumber?.Trim(),
                RegistrationDateTime = DateTime.Now,
                ReferringPhysician = physicianToSave
            };

            var labOrder = new LabOrderA
            {
                RegistrationDateTime = newPatient.RegistrationDateTime,
                ReferringPhysician = physicianToSave,
                // Patient = newPatient; // لم نعد بحاجة لتعيين هذا هنا، سيتم الربط عبر الإضافة للمجموعة أدناه
            };

            foreach (var test in SelectedTests)
            {
                labOrder.Items.Add(new LabOrderItem { TestId = test.Id, IsPrinted = false });
            }

            // *** التعديل الرئيسي هنا: إضافة الطلب إلى مجموعة طلبات المريض ***
            newPatient.LabOrders.Add(labOrder);

            try
            {
                _dbContext.Patients.Add(newPatient);

                int changes = await _dbContext.SaveChangesAsync();

                if (changes > 0)
                {
                    Debug.WriteLine($"Successfully saved new patient and related data. Rows affected: {changes}");

                    if (physicianToSave != null && ReferringPhysicians.All(rp => rp.Id != physicianToSave.Id))
                    {
                        ReferringPhysicians.Add(physicianToSave);
                        var sortedPhysicians = ReferringPhysicians.OrderBy(p => p.Name).ToList();
                        ReferringPhysicians.Clear();
                        foreach (var p in sortedPhysicians) ReferringPhysicians.Add(p);
                        SelectedReferringPhysician = ReferringPhysicians.FirstOrDefault(rp => rp.Id == physicianToSave.Id);
                    }

                    if (!string.IsNullOrWhiteSpace(SelectedTitle) && !Titles.Contains(SelectedTitle))
                    {
                        Titles.Add(SelectedTitle);
                        var sortedTitles = Titles.OrderBy(t => t).ToList(); Titles.Clear();
                        foreach (var title in sortedTitles) Titles.Add(title);
                        SelectedTitle = newPatient.Title;
                    }

                    DisplayGeneratedId = newGeneratedId;
                    MessageBox.Show($"تم حفظ بيانات المريض بنجاح.\nID: {newGeneratedId}", "تم الحفظ", MessageBoxButton.OK, MessageBoxImage.Information);
                    ResetForm();
                }
                else
                {
                    Debug.WriteLine($"Failed to save new patient data. SaveChangesAsync returned 0 changes.");
                    MessageBox.Show($"لم يتم حفظ بيانات المريض. لم يتم إجراء أي تغييرات في قاعدة البيانات.", "فشل الحفظ", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (DbUpdateException dbEx)
            {
                Debug.WriteLine($"Error saving new patient (DbUpdateException): {dbEx.ToString()}");
                MessageBox.Show($"خطأ في قاعدة البيانات أثناء حفظ المريض: {dbEx.InnerException?.Message ?? dbEx.Message}", "DB Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error saving new patient (Exception): {ex.ToString()}");
                MessageBox.Show($"خطأ غير متوقع أثناء حفظ المريض: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ResetForm()
        {
            SelectedTitle = Titles.FirstOrDefault() ?? "السيد"; PatientName = string.Empty;
            IsMaleSelected = true; IsFemaleSelected = false; IsUnknownSelected = false;
            AgeValueInput = 0; SelectedAgeUnit = AgeUnits.FirstOrDefault(u => u == "Year") ?? AgeUnits.First();
            MedicalRecordNumber = null; PhoneNumber = null; Email = null; Address = null;
            SelectedReferringPhysician = null; CustomReferringPhysicianName = null;
            SelectedAvailableTest = null; SelectedTestForRemoval = null; SelectedTests.Clear(); DisplayGeneratedId = null;
        }
    }
}
// نهاية الكود لملف ViewModels/PatientsViewModel.cs