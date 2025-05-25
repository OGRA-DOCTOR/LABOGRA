// بداية الكود لملف ViewModels/SearchEditViewModel.cs
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
        private readonly LabDbContext _dbContext; // تعديل: لاستقبال DbContext مباشرة

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

        [ObservableProperty] private Patient? editablePatient; // هذا هو الكائن الذي يتم تعديله
        public bool IsPatientSelectedForEdit => EditablePatient != null;
        [ObservableProperty] private bool isEditablePatientMale;
        [ObservableProperty] private bool isEditablePatientFemale;
        [ObservableProperty] private ReferringPhysician? selectedEditableReferringPhysician;

        public ObservableCollection<string> Titles { get; } = new ObservableCollection<string> { "السيد", "السيدة", "الطفل", "الطفلة", "الأستاذ", "الأستاذة", "الحاج", "الحاجة", "الدكتور", "الدكتورة", "الآنسة", "مدام" };
        public ObservableCollection<string> AgeUnits { get; } = new ObservableCollection<string> { "يوم", "شهر", "سنة" }; // يجب أن تكون بالإنجليزية لتتوافق مع PatientsViewModel
        public ObservableCollection<ReferringPhysician> ReferringPhysicians { get; } = new ObservableCollection<ReferringPhysician>();

        // تعديل المنشئ ليقبل LabDbContext
        public SearchEditViewModel(LabDbContext dbContext)
        {
            _dbContext = dbContext; // تخزين DbContext المستلم
            // تعديل وحدات العمر لتكون بالإنجليزية
            AgeUnits.Clear();
            AgeUnits.Add("Day");
            AgeUnits.Add("Month");
            AgeUnits.Add("Year");
            _ = LoadInitialDataAsync();
        }

        private async Task LoadInitialDataAsync()
        {
            try
            {
                // استخدام _dbContext مباشرة
                var physicians = await _dbContext.ReferringPhysicians.OrderBy(p => p.Name).ToListAsync();
                ReferringPhysicians.Clear();
                foreach (var physician in physicians) ReferringPhysicians.Add(physician);
            }
            catch (Exception ex) { MessageBox.Show($"حدث خطأ أثناء تحميل قائمة الأطباء: {ex.Message}", "خطأ تحميل", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        [RelayCommand]
        private async Task SearchAsync()
        {
            SearchResults.Clear(); SelectedPatientFromSearch = null; EditablePatient = null;
            try
            {
                // استخدام _dbContext مباشرة
                IQueryable<Patient> query = _dbContext.Patients.Include(p => p.ReferringPhysician);
                if (!string.IsNullOrWhiteSpace(SearchPatientName)) query = query.Where(p => p.Name.Contains(SearchPatientName));
                if (!string.IsNullOrWhiteSpace(SearchMedicalRecordNumber)) query = query.Where(p => p.MedicalRecordNumber != null && p.MedicalRecordNumber.Contains(SearchMedicalRecordNumber));
                if (!string.IsNullOrWhiteSpace(SearchPhoneNumber)) query = query.Where(p => p.PhoneNumber != null && p.PhoneNumber.Contains(SearchPhoneNumber));
                if (SearchRegistrationDateFrom.HasValue) query = query.Where(p => p.RegistrationDateTime >= SearchRegistrationDateFrom.Value.Date);
                if (SearchRegistrationDateTo.HasValue) query = query.Where(p => p.RegistrationDateTime < SearchRegistrationDateTo.Value.Date.AddDays(1));
                if (!string.IsNullOrWhiteSpace(SearchReferringPhysicianName)) query = query.Where(p => p.ReferringPhysician != null && p.ReferringPhysician.Name.Contains(SearchReferringPhysicianName));

                var results = await query.OrderByDescending(p => p.RegistrationDateTime).Take(100).ToListAsync();
                foreach (var patient in results) SearchResults.Add(patient);
                if (!SearchResults.Any()) MessageBox.Show("لم يتم العثور على نتائج تطابق معايير البحث.", "لا توجد نتائج", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex) { MessageBox.Show($"حدث خطأ أثناء عملية البحث: {ex.Message}", "خطأ بحث", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        [RelayCommand]
        private void ClearSearch()
        {
            SearchPatientName = string.Empty; SearchMedicalRecordNumber = string.Empty; SearchPhoneNumber = string.Empty;
            SearchRegistrationDateFrom = null; SearchRegistrationDateTo = null; SearchReferringPhysicianName = string.Empty;
            SearchResults.Clear(); SelectedPatientFromSearch = null; EditablePatient = null;
        }

        private void LoadSelectedPatientForEditing()
        {
            if (SelectedPatientFromSearch != null)
            {
                // إنشاء نسخة جديدة لتعديلها بدلاً من تعديل كائن البحث مباشرة
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
                    AgeUnit = SelectedPatientFromSearch.AgeUnit, // تأكد أن AgeUnit هنا بالإنجليزية
                    PhoneNumber = SelectedPatientFromSearch.PhoneNumber,
                    RegistrationDateTime = SelectedPatientFromSearch.RegistrationDateTime,
                    ReferringPhysicianId = SelectedPatientFromSearch.ReferringPhysicianId,
                    // لا ننسخ LabOrders أو Items لأننا لا نعدلها هنا
                };
                // تحميل الطبيب المحول بشكل منفصل إذا لزم الأمر
                if (EditablePatient.ReferringPhysicianId.HasValue)
                {
                    EditablePatient.ReferringPhysician = ReferringPhysicians.FirstOrDefault(rp => rp.Id == EditablePatient.ReferringPhysicianId.Value);
                }


                IsEditablePatientMale = EditablePatient.Gender == "Male";
                IsEditablePatientFemale = EditablePatient.Gender == "Female";
                SelectedEditableReferringPhysician = EditablePatient.ReferringPhysician;
            }
            else { EditablePatient = null; }
            OnPropertyChanged(nameof(IsPatientSelectedForEdit));
        }

        [RelayCommand]
        private async Task SaveChangesAsync()
        {
            if (EditablePatient == null) { MessageBox.Show("لا يوجد مريض محدد للتعديل.", "خطأ", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
            if (string.IsNullOrWhiteSpace(EditablePatient.Name)) { MessageBox.Show("اسم المريض لا يمكن أن يكون فارغًا.", "خطأ إدخال", MessageBoxButton.OK, MessageBoxImage.Warning); return; }

            if (IsEditablePatientMale) EditablePatient.Gender = "Male";
            else if (IsEditablePatientFemale) EditablePatient.Gender = "Female";
            // إذا لم يكن أي منهما محددًا، يمكننا تعيينه إلى "Unknown" أو تركه كما هو
            else EditablePatient.Gender = "Unknown";


            EditablePatient.ReferringPhysicianId = SelectedEditableReferringPhysician?.Id;
            // لا حاجة لتعيين EditablePatient.ReferringPhysician هنا لأن EF Core سيتعامل مع العلاقة عبر ReferringPhysicianId

            try
            {
                // استخدام _dbContext مباشرة
                var patientInDb = await _dbContext.Patients.FindAsync(EditablePatient.Id);
                if (patientInDb != null)
                {
                    patientInDb.Title = EditablePatient.Title; patientInDb.Name = EditablePatient.Name;
                    patientInDb.MedicalRecordNumber = EditablePatient.MedicalRecordNumber; patientInDb.Email = EditablePatient.Email;
                    patientInDb.Address = EditablePatient.Address; patientInDb.Gender = EditablePatient.Gender;
                    patientInDb.AgeValue = EditablePatient.AgeValue; patientInDb.AgeUnit = EditablePatient.AgeUnit; // تأكد أن AgeUnit هنا بالإنجليزية
                    patientInDb.PhoneNumber = EditablePatient.PhoneNumber;
                    patientInDb.ReferringPhysicianId = EditablePatient.ReferringPhysicianId;
                    // لا نعدل RegistrationDateTime

                    await _dbContext.SaveChangesAsync();
                    MessageBox.Show("تم حفظ التعديلات بنجاح!", "نجاح", MessageBoxButton.OK, MessageBoxImage.Information);

                    // تحديث العنصر في قائمة نتائج البحث
                    var patientInResults = SearchResults.FirstOrDefault(p => p.Id == patientInDb.Id);
                    if (patientInResults != null)
                    {
                        // تحديث خصائص patientInResults من patientInDb لتعكس أي تغييرات (مثل الطبيب المحول إذا تم جلبه)
                        patientInResults.Title = patientInDb.Title; patientInResults.Name = patientInDb.Name;
                        patientInResults.MedicalRecordNumber = patientInDb.MedicalRecordNumber; patientInResults.Email = patientInDb.Email;
                        patientInResults.Address = patientInDb.Address; patientInResults.Gender = patientInDb.Gender;
                        patientInResults.AgeValue = patientInDb.AgeValue; patientInResults.AgeUnit = patientInDb.AgeUnit;
                        patientInResults.PhoneNumber = patientInDb.PhoneNumber;
                        patientInResults.ReferringPhysicianId = patientInDb.ReferringPhysicianId;
                        // إعادة تحميل الطبيب المحول لقائمة العرض
                        if (patientInDb.ReferringPhysicianId.HasValue)
                        {
                            patientInResults.ReferringPhysician = await _dbContext.ReferringPhysicians.FindAsync(patientInDb.ReferringPhysicianId.Value);
                        }
                        else
                        {
                            patientInResults.ReferringPhysician = null;
                        }
                        // لإجبار واجهة المستخدم على التحديث
                        var index = SearchResults.IndexOf(patientInResults);
                        SearchResults.RemoveAt(index);
                        SearchResults.Insert(index, patientInResults);
                        SelectedPatientFromSearch = patientInResults; // إعادة تحديد العنصر المحدث
                    }
                    EditablePatient = null; // مسح نموذج التعديل
                    OnPropertyChanged(nameof(IsPatientSelectedForEdit));
                }
                else { MessageBox.Show("لم يتم العثور على المريض في قاعدة البيانات لتحديثه.", "خطأ تحديث", MessageBoxButton.OK, MessageBoxImage.Error); }
            }
            catch (Exception ex) { MessageBox.Show($"حدث خطأ أثناء حفظ التعديلات: {ex.Message}", "خطأ حفظ", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        [RelayCommand]
        private void CancelEdit() { EditablePatient = null; OnPropertyChanged(nameof(IsPatientSelectedForEdit)); }

        partial void OnIsEditablePatientMaleChanged(bool value) { if (value && EditablePatient != null) { EditablePatient.Gender = "Male"; IsEditablePatientFemale = false; } }
        partial void OnIsEditablePatientFemaleChanged(bool value) { if (value && EditablePatient != null) { EditablePatient.Gender = "Female"; IsEditablePatientMale = false; } }
    }
}
// نهاية الكود لملف ViewModels/SearchEditViewModel.cs