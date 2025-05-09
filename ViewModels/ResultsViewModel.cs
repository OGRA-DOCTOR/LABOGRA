// الإصدار: 4 (تعديل طفيف على LabOrderItemViewModel) - لا تغييرات إضافية في هذا التحديث
// اسم الملف: LABOGRA/ViewModels/ResultsViewModel.cs
// تاريخ التحديث: 2023-10-29
// الوصف:
// 1. تعديل CanSaveResult في LabOrderItemViewModel ليكون دائماً true في البداية (لإظهار الزر برتقالياً).
//    التحقق من وجود قيمة سيتم عند ضغط الزر.
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

namespace LABOGRA.ViewModels
{
    public partial class ResultsViewModel : ObservableObject
    {
        private readonly LabDbContext _context;

        [ObservableProperty]
        private ObservableCollection<Patient> patientList = new ObservableCollection<Patient>();

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(LoadPatientTestsCommand))]
        [NotifyPropertyChangedFor(nameof(IsPatientSelectionEnabled))]
        private Patient? selectedPatient;

        [ObservableProperty]
        private ObservableCollection<LabOrderItemViewModel> labOrderItems = new ObservableCollection<LabOrderItemViewModel>();

        [ObservableProperty]
        private bool isPatientListLoaded = false;

        public bool IsPatientSelectionEnabled => IsPatientListLoaded && PatientList.Any();


        [RelayCommand(CanExecute = nameof(CanLoadTests))]
        private async Task LoadPatientTestsAsync()
        {
            if (SelectedPatient == null)
            {
                LabOrderItems.Clear();
                return;
            }
            LabOrderItems.Clear();
            try
            {
                var orderToProcess = await _context.LabOrders
                                        .Include(o => o.Items)
                                            .ThenInclude(i => i.Test)
                                                .ThenInclude(t => t.ReferenceValues)
                                        .Where(o => o.PatientId == SelectedPatient.Id)
                                        .OrderByDescending(o => o.RegistrationDateTime)
                                        .FirstOrDefaultAsync();

                if (orderToProcess != null && orderToProcess.Items != null)
                {
                    foreach (var item in orderToProcess.Items.OrderBy(i => i.Test?.Name))
                    {
                        LabOrderItems.Add(new LabOrderItemViewModel(item, SelectedPatient.Gender, _context));
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while loading patient tests: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanLoadTests() => SelectedPatient != null;

        public ResultsViewModel()
        {
            var contextFactory = new LabDbContextFactory();
            _context = contextFactory.CreateDbContext(Array.Empty<string>());
            _ = LoadInitialPatientsAsync();
        }

        private async Task LoadInitialPatientsAsync()
        {
            PatientList.Clear();
            IsPatientListLoaded = false;
            OnPropertyChanged(nameof(IsPatientSelectionEnabled));

            try
            {
                var patients = await _context.Patients
                                       .OrderByDescending(p => p.RegistrationDateTime)
                                       .ToListAsync();
                foreach (var patient in patients)
                {
                    PatientList.Add(patient);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while loading the patient list: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsPatientListLoaded = true;
                OnPropertyChanged(nameof(IsPatientSelectionEnabled));
            }
        }

        async partial void OnSelectedPatientChanged(Patient? value)
        {
            await LoadPatientTestsCommand.ExecuteAsync(null);
        }
    }

    public partial class LabOrderItemViewModel : ObservableObject
    {
        private readonly LabOrderItem _item;
        private readonly string _patientGender;
        private readonly LabDbContext _dbContext;

        public int Id => _item.Id;
        public string TestName => _item.Test?.Name ?? "N/A";
        public string? TestAbbreviation => _item.Test?.Abbreviation;

        [ObservableProperty]
        private string? resultValue;

        [ObservableProperty]
        private string? unit;

        [ObservableProperty]
        private string? displayReferenceRange;

        [ObservableProperty]
        private bool isSavedSuccessfully = false;

        public LabOrderItem OriginalItem => _item;


        public LabOrderItemViewModel(LabOrderItem item, string patientGender, LabDbContext dbContext)
        {
            _item = item;
            _patientGender = patientGender?.ToUpperInvariant() ?? "UNKNOWN";
            _dbContext = dbContext;
            ResultValue = item.Result;
            IsSavedSuccessfully = !string.IsNullOrWhiteSpace(item.Result);
            DetermineReferenceValueAndUnit();
        }

        private void DetermineReferenceValueAndUnit()
        {
            if (_item.Test?.ReferenceValues == null || !_item.Test.ReferenceValues.Any())
            {
                Unit = "N/A";
                DisplayReferenceRange = "N/A";
                return;
            }

            var genderSpecificRef = _item.Test.ReferenceValues
                                         .FirstOrDefault(rv => rv.GenderSpecific != null && rv.GenderSpecific.Equals(_patientGender, StringComparison.OrdinalIgnoreCase));

            var generalRef = _item.Test.ReferenceValues
                                      .FirstOrDefault(rv => rv.GenderSpecific != null && rv.GenderSpecific.Equals("ANY", StringComparison.OrdinalIgnoreCase));

            var fallbackRef = _item.Test.ReferenceValues.FirstOrDefault();

            var selectedRef = genderSpecificRef ?? generalRef ?? fallbackRef;

            if (selectedRef != null)
            {
                DisplayReferenceRange = selectedRef.ReferenceText ?? "N/A";
                Unit = selectedRef.Unit ?? "N/A";
            }
            else
            {
                DisplayReferenceRange = "N/A";
                Unit = "N/A";
            }
        }

        [RelayCommand(CanExecute = nameof(CanSaveResult))]
        private async Task SaveResultAsync()
        {
            // التحقق من وجود قيمة يتم هنا عند الضغط
            if (string.IsNullOrWhiteSpace(ResultValue))
            {
                // تم إلغاء رسالة التأكيد، لكن يمكن إبقاء هذا التحقق إذا أردت
                // MessageBox.Show($"Please enter a result for {TestName}.", "Input Required", MessageBoxButton.OK, MessageBoxImage.Warning);
                return; // لا تحفظ إذا كانت القيمة فارغة
            }

            try
            {
                var itemToUpdate = await _dbContext.LabOrderItems.FindAsync(this.Id);
                if (itemToUpdate != null)
                {
                    itemToUpdate.Result = this.ResultValue;
                    itemToUpdate.ResultUnit = this.Unit;

                    await _dbContext.SaveChangesAsync();
                    IsSavedSuccessfully = true; // تحديث حالة الحفظ لتغيير لون الزر
                    // تم إلغاء MessageBox.Show من هنا بناءً على طلبك.
                }
                else
                {
                    // يمكنك إعادة تفعيل هذه الرسالة إذا أردت إعلام المستخدم بفشل العثور على العنصر
                    // MessageBox.Show($"Could not find the item {TestName} to save the result.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (DbUpdateException ex)
            {
                MessageBox.Show($"A database error occurred while saving the result for {TestName}: {ex.InnerException?.Message ?? ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An unexpected error occurred while saving the result for {TestName}: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanSaveResult()
        {
            return true;
        }
    }
}