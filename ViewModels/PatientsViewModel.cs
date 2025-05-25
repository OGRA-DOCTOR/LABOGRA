// بداية الكود لملف ViewModels/PatientsViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LABOGRA.Models;
using LABOGRA.Services; // For IDatabaseService
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace LABOGRA.ViewModels
{
    public partial class PatientsViewModel : ObservableObject
    {
        private readonly IDatabaseService _databaseService;

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

        public PatientsViewModel(IDatabaseService databaseService)
        {
            _databaseService = databaseService ?? throw new ArgumentNullException(nameof(databaseService));
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
                var physicians = await _databaseService.GetPhysiciansAsync(); // Uses updated service
                ReferringPhysicians.Clear();
                foreach (var physician in physicians.OrderBy(p => p.Name)) ReferringPhysicians.Add(physician);

                var tests = await _databaseService.GetTestsAsync(); // Uses updated service
                AvailableTests.Clear();
                foreach (var test in tests.OrderBy(t => t.Name)) AvailableTests.Add(test);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء تحميل البيانات الأولية: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task<string> GenerateNewPatientIdAsync()
        {
            var today = DateTime.Today;
            var datePart = today.ToString("yyyyMMdd");
            var latestPatient = await _databaseService.GetLatestPatientByGeneratedIdPrefixAsync(datePart);

            int next = 1;
            if (latestPatient?.GeneratedId != null)
            {
                string seqPart = latestPatient.GeneratedId.Substring(datePart.Length); // Corrected Substring usage
                if (int.TryParse(seqPart, out int lastSeq)) next = lastSeq + 1;
            }
            return $"{datePart}{next:D2}";
        }

        [RelayCommand]
        private void AddSelectedTest() { if (SelectedAvailableTest != null && !SelectedTests.Contains(SelectedAvailableTest)) SelectedTests.Add(SelectedAvailableTest); }
        [RelayCommand]
        private void RemoveSelectedTest() { if (SelectedTestForRemoval != null) SelectedTests.Remove(SelectedTestForRemoval); }
        [RelayCommand]
        private void AddAllTests() { foreach (var test in AvailableTests) if (!SelectedTests.Contains(test)) SelectedTests.Add(test); }

        [RelayCommand]
        private async Task SavePatientAsync()
        {
            if (string.IsNullOrWhiteSpace(PatientName) || AgeValueInput <= 0 || string.IsNullOrWhiteSpace(SelectedAgeUnit)) { MessageBox.Show("يرجى إدخال الاسم والعمر (القيمة والوحدة).", "تحذير", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
            if (SelectedTests.Count == 0) { MessageBox.Show("يرجى اختيار تحليل واحد على الأقل.", "تحذير", MessageBoxButton.OK, MessageBoxImage.Warning); return; }

            ReferringPhysician? physicianToSave = SelectedReferringPhysician;
            bool newPhysicianAddedDuringThisOperation = false;

            if (physicianToSave == null && !string.IsNullOrWhiteSpace(CustomReferringPhysicianName))
            {
                var trimmedCustomName = CustomReferringPhysicianName.Trim();
                physicianToSave = await _databaseService.FindOrCreateReferringPhysicianAsync(trimmedCustomName);
                // If FindOrCreateReferringPhysicianAsync saves the physician immediately, then physicianToSave.Id will be set.
                // If it doesn't save, then it's tracked by context and will be saved with AddPatientWithLabOrderAsync.
                // We need to know if it's truly new to add it to the ObservableCollection later.
                if (physicianToSave.Id == 0) // Indicates it was new and not yet saved by FindOrCreate (if it doesn't save)
                {
                    newPhysicianAddedDuringThisOperation = true;
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
                // ReferringPhysician property will be set by EF Core if ID is set, or if object is tracked
                ReferringPhysicianId = physicianToSave?.Id
            };
            // If physicianToSave is a new entity not yet saved, and its ID is 0,
            // EF Core will handle the relationship when newPatient (and thus physicianToSave via navigation) is added.
            if (physicianToSave != null && physicianToSave.Id != 0) newPatient.ReferringPhysician = physicianToSave;


            var labOrder = new LabOrderA // Corrected to LabOrderA
            {
                RegistrationDateTime = newPatient.RegistrationDateTime,
                ReferringPhysicianId = physicianToSave?.Id,
                // PatientId will be set by EF Core relationship
            };
            if (physicianToSave != null && physicianToSave.Id != 0) labOrder.ReferringPhysician = physicianToSave;


            foreach (var test in SelectedTests) { labOrder.Items.Add(new LabOrderItem { TestId = test.Id, IsPrinted = false }); }
            newPatient.LabOrders.Add(labOrder);

            try
            {
                bool success = await _databaseService.AddPatientWithLabOrderAsync(newPatient);

                if (success)
                {
                    Debug.WriteLine($"Successfully saved new patient. Generated ID: {newGeneratedId}");
                    if (physicianToSave != null && (newPhysicianAddedDuringThisOperation || ReferringPhysicians.All(rp => rp.Id != physicianToSave.Id)))
                    {
                        // If FindOrCreate... saved it, physicianToSave.Id will be > 0.
                        // We need to ensure the local collection is updated.
                        // It's safer to reload physicians or ensure the service returns the saved entity.
                        // For now, if it was truly new OR not in collection, add it.
                        if (ReferringPhysicians.All(rp => rp.Id != physicianToSave.Id) && physicianToSave.Id != 0) // Ensure it has an ID
                        {
                            ReferringPhysicians.Add(physicianToSave); // Add the potentially newly saved physician
                                                                      // Re-sort or re-fetch list might be better for UI consistency
                        }
                    }
                    DisplayGeneratedId = newGeneratedId;
                    MessageBox.Show($"تم حفظ بيانات المريض بنجاح.\nID: {newGeneratedId}", "تم الحفظ", MessageBoxButton.OK, MessageBoxImage.Information);
                    ResetForm();
                }
                else { Debug.WriteLine($"Failed to save new patient data. Service returned false."); MessageBox.Show($"لم يتم حفظ بيانات المريض.", "فشل الحفظ", MessageBoxButton.OK, MessageBoxImage.Warning); }
            }
            catch (DbUpdateException dbEx) { Debug.WriteLine($"Error saving new patient (DbUpdateException): {dbEx.ToString()}"); MessageBox.Show($"خطأ في قاعدة البيانات: {dbEx.InnerException?.Message ?? dbEx.Message}", "DB Error", MessageBoxButton.OK, MessageBoxImage.Error); }
            catch (Exception ex) { Debug.WriteLine($"Error saving new patient (Exception): {ex.ToString()}"); MessageBox.Show($"خطأ غير متوقع: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
        }
        private void ResetForm() { SelectedTitle = Titles.FirstOrDefault() ?? "السيد"; PatientName = string.Empty; IsMaleSelected = true; IsFemaleSelected = false; IsUnknownSelected = false; AgeValueInput = 0; SelectedAgeUnit = AgeUnits.FirstOrDefault(u => u == "Year") ?? AgeUnits.First(); MedicalRecordNumber = null; PhoneNumber = null; Email = null; Address = null; SelectedReferringPhysician = null; CustomReferringPhysicianName = null; SelectedAvailableTest = null; SelectedTestForRemoval = null; SelectedTests.Clear(); DisplayGeneratedId = null; }
    }
}
// نهاية الكود لملف ViewModels/PatientsViewModel.cs