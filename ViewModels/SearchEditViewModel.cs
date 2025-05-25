// بداية الكود لملف ViewModels/SearchEditViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LABOGRA.Models;
using LABOGRA.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace LABOGRA.ViewModels
{
    public partial class SearchEditViewModel : ObservableObject
    {
        private readonly IDatabaseService _databaseService;

        [ObservableProperty] private string? searchPatientName;
        [ObservableProperty] private string? searchMedicalRecordNumber;
        [ObservableProperty] private string? searchPhoneNumber;
        [ObservableProperty] private DateTime? searchRegistrationDateFrom;
        [ObservableProperty] private DateTime? searchRegistrationDateTo;
        [ObservableProperty] private string? searchReferringPhysicianName;
        [ObservableProperty] private ObservableCollection<Patient> searchResults = new ObservableCollection<Patient>();

        private Patient? _selectedPatientFromSearch;
        public Patient? SelectedPatientFromSearch
        {
            get => _selectedPatientFromSearch;
            set { if (SetProperty(ref _selectedPatientFromSearch, value)) { LoadSelectedPatientForEditing(); OnPropertyChanged(nameof(IsPatientSelectedForEdit)); } }
        }

        [ObservableProperty] private Patient? editablePatient;
        public bool IsPatientSelectedForEdit => EditablePatient != null;
        [ObservableProperty] private bool isEditablePatientMale;
        [ObservableProperty] private bool isEditablePatientFemale;
        [ObservableProperty] private ReferringPhysician? selectedEditableReferringPhysician;

        public ObservableCollection<string> Titles { get; } = new ObservableCollection<string> { "السيد", "السيدة", "الطفل", "الطفلة", "الأستاذ", "الأستاذة", "الحاج", "الحاجة", "الدكتور", "الدكتورة", "الآنسة", "مدام" };
        public ObservableCollection<string> AgeUnits { get; } = new ObservableCollection<string> { "Day", "Month", "Year" };
        public ObservableCollection<ReferringPhysician> ReferringPhysicians { get; } = new ObservableCollection<ReferringPhysician>();

        public SearchEditViewModel(IDatabaseService databaseService)
        {
            _databaseService = databaseService ?? throw new ArgumentNullException(nameof(databaseService));
            _ = LoadInitialDataAsync();
        }

        private async Task LoadInitialDataAsync()
        {
            try
            {
                var physicians = await _databaseService.GetPhysiciansAsync();
                ReferringPhysicians.Clear();
                foreach (var physician in physicians.OrderBy(p => p.Name)) ReferringPhysicians.Add(physician);
            }
            catch (Exception ex) { MessageBox.Show($"حدث خطأ أثناء تحميل قائمة الأطباء: {ex.Message}", "خطأ تحميل", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        [RelayCommand]
        private async Task SearchAsync()
        {
            SearchResults.Clear();
            SelectedPatientFromSearch = null;
            try
            {
                string searchTermForService = SearchPatientName ?? SearchMedicalRecordNumber ?? SearchPhoneNumber ?? string.Empty;
                var results = await _databaseService.SearchPatientsAsync(searchTermForService);

                // Manual filtering for other criteria (Not efficient, done in memory)
                // This section highlights the need for a more advanced search in IDatabaseService
                IEnumerable<Patient> filteredResults = results; // Start with results from service
                if (!string.IsNullOrWhiteSpace(SearchPatientName) && string.IsNullOrEmpty(searchTermForService)) // If service search was empty but name is not
                {
                    filteredResults = filteredResults.Where(p => p.Name != null && p.Name.Contains(SearchPatientName, StringComparison.OrdinalIgnoreCase));
                }
                if (!string.IsNullOrWhiteSpace(SearchMedicalRecordNumber) && string.IsNullOrEmpty(searchTermForService))
                {
                    filteredResults = filteredResults.Where(p => p.MedicalRecordNumber != null && p.MedicalRecordNumber.Contains(SearchMedicalRecordNumber));
                }
                if (!string.IsNullOrWhiteSpace(SearchPhoneNumber) && string.IsNullOrEmpty(searchTermForService))
                {
                    filteredResults = filteredResults.Where(p => p.PhoneNumber != null && p.PhoneNumber.Contains(SearchPhoneNumber));
                }

                if (SearchRegistrationDateFrom.HasValue)
                    filteredResults = filteredResults.Where(p => p.RegistrationDateTime >= SearchRegistrationDateFrom.Value.Date);
                if (SearchRegistrationDateTo.HasValue)
                    filteredResults = filteredResults.Where(p => p.RegistrationDateTime < SearchRegistrationDateTo.Value.Date.AddDays(1));
                if (!string.IsNullOrWhiteSpace(SearchReferringPhysicianName))
                    filteredResults = filteredResults.Where(p => p.ReferringPhysician != null && p.ReferringPhysician.Name.Contains(SearchReferringPhysicianName, StringComparison.OrdinalIgnoreCase));

                foreach (var patient in filteredResults.OrderByDescending(p => p.RegistrationDateTime).Take(100))
                    SearchResults.Add(patient);

                if (!SearchResults.Any())
                    MessageBox.Show("لم يتم العثور على نتائج تطابق معايير البحث.", "لا توجد نتائج", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex) { MessageBox.Show($"حدث خطأ أثناء عملية البحث: {ex.Message}", "خطأ بحث", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        [RelayCommand]
        private void ClearSearch()
        {
            SearchPatientName = string.Empty; SearchMedicalRecordNumber = string.Empty; SearchPhoneNumber = string.Empty;
            SearchRegistrationDateFrom = null; SearchRegistrationDateTo = null; SearchReferringPhysicianName = string.Empty;
            SearchResults.Clear();
            SelectedPatientFromSearch = null; // This will trigger LoadSelectedPatientForEditing and clear EditablePatient
        }

        // *** Removed 'async' from this method signature ***
        private void LoadSelectedPatientForEditing()
        {
            if (SelectedPatientFromSearch != null)
            {
                EditablePatient = new Patient
                {
                    Id = SelectedPatientFromSearch.Id,
                    Title = SelectedPatientFromSearch.Title,
                    Name = SelectedPatientFromSearch.Name,
                    MedicalRecordNumber = SelectedPatientFromSearch.MedicalRecordNumber,
                    Email = SelectedPatientFromSearch.Email,
                    Address = SelectedPatientFromSearch.Address,
                    Gender = SelectedPatientFromSearch.Gender,
                    AgeValue = SelectedPatientFromSearch.AgeValue,
                    AgeUnit = SelectedPatientFromSearch.AgeUnit,
                    PhoneNumber = SelectedPatientFromSearch.PhoneNumber,
                    RegistrationDateTime = SelectedPatientFromSearch.RegistrationDateTime,
                    ReferringPhysicianId = SelectedPatientFromSearch.ReferringPhysicianId,
                    ReferringPhysician = SelectedPatientFromSearch.ReferringPhysician // Copy the object too
                };

                if (EditablePatient.ReferringPhysicianId.HasValue)
                {
                    SelectedEditableReferringPhysician = ReferringPhysicians.FirstOrDefault(rp => rp.Id == EditablePatient.ReferringPhysicianId.Value);
                    // EditablePatient.ReferringPhysician is already set from SelectedPatientFromSearch if it was loaded.
                }
                else
                {
                    SelectedEditableReferringPhysician = null;
                }

                IsEditablePatientMale = EditablePatient.Gender == "Male";
                IsEditablePatientFemale = EditablePatient.Gender == "Female";
            }
            else
            {
                EditablePatient = null;
                SelectedEditableReferringPhysician = null;
                IsEditablePatientMale = false; // Reset these too
                IsEditablePatientFemale = false;
            }
        }

        [RelayCommand]
        private async Task SaveChangesAsync()
        {
            if (EditablePatient == null) { MessageBox.Show("لا يوجد مريض محدد للتعديل.", "خطأ", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
            if (string.IsNullOrWhiteSpace(EditablePatient.Name)) { MessageBox.Show("اسم المريض لا يمكن أن يكون فارغًا.", "خطأ إدخال", MessageBoxButton.OK, MessageBoxImage.Warning); return; }

            if (IsEditablePatientMale) EditablePatient.Gender = "Male";
            else if (IsEditablePatientFemale) EditablePatient.Gender = "Female";
            else EditablePatient.Gender = "Unknown";

            EditablePatient.ReferringPhysicianId = SelectedEditableReferringPhysician?.Id;
            EditablePatient.ReferringPhysician = SelectedEditableReferringPhysician; // Ensure the object is also set

            try
            {
                Patient updatedPatient = await _databaseService.SavePatientAsync(EditablePatient);

                MessageBox.Show("تم حفظ التعديلات بنجاح!", "نجاح", MessageBoxButton.OK, MessageBoxImage.Information);

                var patientInResults = SearchResults.FirstOrDefault(p => p.Id == updatedPatient.Id);
                if (patientInResults != null)
                {
                    var index = SearchResults.IndexOf(patientInResults);
                    SearchResults.RemoveAt(index);

                    // Ensure the updatedPatient has its ReferringPhysician object loaded if ID exists
                    if (updatedPatient.ReferringPhysicianId.HasValue && updatedPatient.ReferringPhysician == null)
                    {
                        updatedPatient.ReferringPhysician = ReferringPhysicians.FirstOrDefault(r => r.Id == updatedPatient.ReferringPhysicianId.Value);
                    }
                    SearchResults.Insert(index, updatedPatient);
                    SelectedPatientFromSearch = updatedPatient;
                }
                EditablePatient = null;
            }
            catch (Exception ex) { MessageBox.Show($"حدث خطأ أثناء حفظ التعديلات: {ex.Message}", "خطأ حفظ", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        [RelayCommand]
        private void CancelEdit() { EditablePatient = null; }

        partial void OnIsEditablePatientMaleChanged(bool value) { if (value && EditablePatient != null) { /*EditablePatient.Gender = "Male";*/ IsEditablePatientFemale = false; } } // Gender is set in SaveChanges
        partial void OnIsEditablePatientFemaleChanged(bool value) { if (value && EditablePatient != null) { /*EditablePatient.Gender = "Female";*/ IsEditablePatientMale = false; } } // Gender is set in SaveChanges
    }
}
// نهاية الكود لملف ViewModels/SearchEditViewModel.cs