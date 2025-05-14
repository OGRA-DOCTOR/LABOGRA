// الإصدار: 5 (إزالة تحذيرات Nullable من EditablePatient) // اسم الملف: SearchEditViewModel.cs // الوصف: ViewModel لواجهة البحث عن المرضى وتعديل بياناتهم، مع إزالة تحذيرات nullable من EditablePatient.

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LABOGRA.Models;
using LABOGRA.Services.Database.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace LABOGRA.ViewModels
{
    public partial class SearchEditViewModel : ObservableObject
    {
        private readonly LabDbContextFactory _contextFactory;

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
            set
            {
                if (SetProperty(ref _selectedPatientFromSearch, value))
                {
                    LoadSelectedPatientForEditing();
                    OnPropertyChanged(nameof(IsPatientSelectedForEdit));
                }
            }
        }

        [ObservableProperty] private Patient? editablePatient;
        public bool IsPatientSelectedForEdit => EditablePatient != null;

        [ObservableProperty] private bool isEditablePatientMale;
        [ObservableProperty] private bool isEditablePatientFemale;

        [ObservableProperty] private ReferringPhysician? selectedEditableReferringPhysician;

        public ObservableCollection<string> Titles { get; } = new ObservableCollection<string>
    {
        "السيد", "السيدة", "الطفل", "الطفلة", "الأستاذ", "الأستاذة",
        "الحاج", "الحاجة", "الدكتور", "الدكتورة", "الآنسة", "مدام"
    };

        public ObservableCollection<string> AgeUnits { get; } = new ObservableCollection<string>
    {
        "يوم", "شهر", "سنة"
    };

        public ObservableCollection<ReferringPhysician> ReferringPhysicians { get; } = new ObservableCollection<ReferringPhysician>();

        public SearchEditViewModel()
        {
            _contextFactory = new LabDbContextFactory();
            _ = LoadInitialDataAsync();
        }

        private async Task LoadInitialDataAsync()
        {
            try
            {
                using var context = _contextFactory.CreateDbContext(Array.Empty<string>());
                var physicians = await context.ReferringPhysicians.OrderBy(p => p.Name).ToListAsync();
                ReferringPhysicians.Clear();
                foreach (var physician in physicians)
                    ReferringPhysicians.Add(physician);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء تحميل قائمة الأطباء: {ex.Message}", "خطأ تحميل", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task SearchAsync()
        {
            SearchResults.Clear();
            SelectedPatientFromSearch = null;
            EditablePatient = null;

            try
            {
                using var context = _contextFactory.CreateDbContext(Array.Empty<string>());
                IQueryable<Patient> query = context.Patients.Include(p => p.ReferringPhysician);

                if (!string.IsNullOrWhiteSpace(SearchPatientName))
                    query = query.Where(p => p.Name.Contains(SearchPatientName));

                if (!string.IsNullOrWhiteSpace(SearchMedicalRecordNumber))
                    query = query.Where(p => p.MedicalRecordNumber != null && p.MedicalRecordNumber.Contains(SearchMedicalRecordNumber));

                if (!string.IsNullOrWhiteSpace(SearchPhoneNumber))
                    query = query.Where(p => p.PhoneNumber != null && p.PhoneNumber.Contains(SearchPhoneNumber));

                if (SearchRegistrationDateFrom.HasValue)
                    query = query.Where(p => p.RegistrationDateTime >= SearchRegistrationDateFrom.Value.Date);

                if (SearchRegistrationDateTo.HasValue)
                    query = query.Where(p => p.RegistrationDateTime < SearchRegistrationDateTo.Value.Date.AddDays(1));

                if (!string.IsNullOrWhiteSpace(SearchReferringPhysicianName))
                    query = query.Where(p => p.ReferringPhysician != null && p.ReferringPhysician.Name.Contains(SearchReferringPhysicianName));

                var results = await query.OrderByDescending(p => p.RegistrationDateTime).Take(100).ToListAsync();

                foreach (var patient in results)
                    SearchResults.Add(patient);

                if (!SearchResults.Any())
                    MessageBox.Show("لم يتم العثور على نتائج تطابق معايير البحث.", "لا توجد نتائج", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء عملية البحث: {ex.Message}", "خطأ بحث", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void ClearSearch()
        {
            SearchPatientName = string.Empty;
            SearchMedicalRecordNumber = string.Empty;
            SearchPhoneNumber = string.Empty;
            SearchRegistrationDateFrom = null;
            SearchRegistrationDateTo = null;
            SearchReferringPhysicianName = string.Empty;
            SearchResults.Clear();
            SelectedPatientFromSearch = null;
            EditablePatient = null;
        }

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
                    ReferringPhysician = SelectedPatientFromSearch.ReferringPhysician
                };

                IsEditablePatientMale = EditablePatient.Gender == "Male";
                IsEditablePatientFemale = EditablePatient.Gender == "Female";

                SelectedEditableReferringPhysician = EditablePatient.ReferringPhysicianId.HasValue
                    ? ReferringPhysicians.FirstOrDefault(rp => rp.Id == EditablePatient.ReferringPhysicianId.Value)
                    : null;
            }
            else
            {
                EditablePatient = null;
            }

            OnPropertyChanged(nameof(IsPatientSelectedForEdit));
        }

        [RelayCommand]
        private async Task SaveChangesAsync()
        {
            if (EditablePatient is not Patient safeEditablePatient)
            {
                MessageBox.Show("لا يوجد مريض محدد للتعديل.", "خطأ", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(safeEditablePatient.Name))
            {
                MessageBox.Show("اسم المريض لا يمكن أن يكون فارغًا.", "خطأ إدخال", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (IsEditablePatientMale) safeEditablePatient.Gender = "Male";
            else if (IsEditablePatientFemale) safeEditablePatient.Gender = "Female";

            if (SelectedEditableReferringPhysician != null)
            {
                safeEditablePatient.ReferringPhysicianId = SelectedEditableReferringPhysician.Id;
                safeEditablePatient.ReferringPhysician = SelectedEditableReferringPhysician;
            }
            else if (!string.IsNullOrWhiteSpace(safeEditablePatient.ReferringPhysician?.Name))
            {
                var existingPhysician = ReferringPhysicians.FirstOrDefault(rp => rp.Name.Equals(safeEditablePatient.ReferringPhysician.Name, StringComparison.OrdinalIgnoreCase));
                if (existingPhysician != null)
                {
                    safeEditablePatient.ReferringPhysicianId = existingPhysician.Id;
                    safeEditablePatient.ReferringPhysician = existingPhysician;
                }
            }
            else
            {
                safeEditablePatient.ReferringPhysicianId = null;
                safeEditablePatient.ReferringPhysician = null;
            }

            var dataForDisplayUpdate = new Patient
            {
                Id = safeEditablePatient.Id,
                Title = safeEditablePatient.Title,
                Name = safeEditablePatient.Name,
                MedicalRecordNumber = safeEditablePatient.MedicalRecordNumber,
                Email = safeEditablePatient.Email,
                Address = safeEditablePatient.Address,
                Gender = safeEditablePatient.Gender,
                AgeValue = safeEditablePatient.AgeValue,
                AgeUnit = safeEditablePatient.AgeUnit,
                PhoneNumber = safeEditablePatient.PhoneNumber,
                RegistrationDateTime = safeEditablePatient.RegistrationDateTime,
                ReferringPhysicianId = safeEditablePatient.ReferringPhysicianId,
                ReferringPhysician = safeEditablePatient.ReferringPhysician
            };

            try
            {
                using var context = _contextFactory.CreateDbContext(Array.Empty<string>());
                var patientInDb = await context.Patients.FirstOrDefaultAsync(p => p.Id == safeEditablePatient.Id);

                if (patientInDb != null)
                {
                    patientInDb.Title = safeEditablePatient.Title;
                    patientInDb.Name = safeEditablePatient.Name;
                    patientInDb.MedicalRecordNumber = safeEditablePatient.MedicalRecordNumber;
                    patientInDb.Email = safeEditablePatient.Email;
                    patientInDb.Address = safeEditablePatient.Address;
                    patientInDb.Gender = safeEditablePatient.Gender;
                    patientInDb.AgeValue = safeEditablePatient.AgeValue;
                    patientInDb.AgeUnit = safeEditablePatient.AgeUnit;
                    patientInDb.PhoneNumber = safeEditablePatient.PhoneNumber;
                    patientInDb.ReferringPhysicianId = safeEditablePatient.ReferringPhysicianId;

                    await context.SaveChangesAsync();

                    MessageBox.Show("تم حفظ التعديلات بنجاح!", "نجاح", MessageBoxButton.OK, MessageBoxImage.Information);

                    var patientInResults = SearchResults.FirstOrDefault(p => p.Id == dataForDisplayUpdate.Id);
                    if (patientInResults != null)
                    {
                        var index = SearchResults.IndexOf(patientInResults);
                        dataForDisplayUpdate.RegistrationDateTime = patientInDb.RegistrationDateTime;
                        SearchResults[index] = dataForDisplayUpdate;
                        SelectedPatientFromSearch = dataForDisplayUpdate;
                    }

                    EditablePatient = null;
                    OnPropertyChanged(nameof(IsPatientSelectedForEdit));
                }
                else
                {
                    MessageBox.Show("لم يتم العثور على المريض في قاعدة البيانات لتحديثه.", "خطأ تحديث", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء حفظ التعديلات: {ex.Message}", "خطأ حفظ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void CancelEdit()
        {
            EditablePatient = null;
            OnPropertyChanged(nameof(IsPatientSelectedForEdit));
        }

        partial void OnIsEditablePatientMaleChanged(bool value)
        {
            if (value && EditablePatient != null)
            {
                EditablePatient.Gender = "Male";
                IsEditablePatientFemale = false;
            }
        }

        partial void OnIsEditablePatientFemaleChanged(bool value)
        {
            if (value && EditablePatient != null)
            {
                EditablePatient.Gender = "Female";
                IsEditablePatientMale = false;
            }
        }
    }

}

