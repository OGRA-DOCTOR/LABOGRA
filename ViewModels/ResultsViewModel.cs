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
        private Patient? selectedPatient;

        [ObservableProperty]
        private ObservableCollection<LabOrderItemViewModel> labOrderItems = new ObservableCollection<LabOrderItemViewModel>();

        // لا نحتاج لهذه إذا كان الحفظ سيتم من داخل LabOrderItemViewModel
        // [ObservableProperty]
        // private LabOrderItemViewModel? selectedLabOrderItem; 

        [RelayCommand(CanExecute = nameof(CanLoadTests))]
        private async Task LoadPatientTestsAsync()
        {
            if (SelectedPatient == null) return;
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
                    foreach (var item in orderToProcess.Items.OrderBy(i => i.Test.Name))
                    {
                        LabOrderItems.Add(new LabOrderItemViewModel(item, SelectedPatient.Gender, _context));
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء تحميل تحاليل المريض: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
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
                MessageBox.Show($"حدث خطأ أثناء تحميل قائمة المرضى: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // التأكد أن هذه الدالة تستدعي الأمر الصحيح
        async partial void OnSelectedPatientChanged(Patient? value)
        {
            // التأكد من استدعاء الأمر عند تغيير المريض
            await LoadPatientTestsCommand.ExecuteAsync(null);
            // مسح القائمة إذا لم يتم اختيار مريض أصبح ضمنياً في بداية LoadPatientTestsAsync
            // if (value == null) LabOrderItems.Clear(); 
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

        // التأكد أن هذه الخاصية Observable لربطها بـ TextBox
        [ObservableProperty]
        private string? resultValue;

        // التأكد أن هذه الخصائص Observable إذا أردنا تحديثها في الواجهة ديناميكياً
        [ObservableProperty]
        private string? unit;

        [ObservableProperty]
        private string? displayReferenceRange;

        public LabOrderItem OriginalItem => _item;


        public LabOrderItemViewModel(LabOrderItem item, string patientGender, LabDbContext dbContext)
        {
            _item = item;
            _patientGender = patientGender;
            _dbContext = dbContext;
            resultValue = item.Result;
            DetermineReferenceValueAndUnit();
        }

        private void DetermineReferenceValueAndUnit()
        {
            if (_item.Test?.ReferenceValues == null || !_item.Test.ReferenceValues.Any())
            {
                Unit = null;
                DisplayReferenceRange = "N/A";
                return;
            }

            var genderSpecificRef = _item.Test.ReferenceValues
                                         .FirstOrDefault(rv => rv.GenderSpecific != null && rv.GenderSpecific.Equals(_patientGender, StringComparison.OrdinalIgnoreCase));
            var generalRef = _item.Test.ReferenceValues
                                      .FirstOrDefault(rv => rv.GenderSpecific != null && rv.GenderSpecific.Equals("Any", StringComparison.OrdinalIgnoreCase));
            var fallbackRef = _item.Test.ReferenceValues.FirstOrDefault();
            var selectedRef = genderSpecificRef ?? generalRef ?? fallbackRef;

            if (selectedRef != null)
            {
                DisplayReferenceRange = selectedRef.ReferenceText;
                Unit = selectedRef.Unit;
            }
            else
            {
                DisplayReferenceRange = "N/A";
                Unit = null;
            }
        }

        [RelayCommand(CanExecute = nameof(CanSaveResult))]
        private async Task SaveResultAsync()
        {
            try
            {
                var itemToUpdate = await _dbContext.LabOrderItems.FindAsync(this.Id);
                if (itemToUpdate != null)
                {
                    itemToUpdate.Result = this.ResultValue;
                    itemToUpdate.ResultUnit = this.Unit;
                    await _dbContext.SaveChangesAsync();
                    MessageBox.Show($"تم حفظ نتيجة {TestName}: {ResultValue}");
                }
                else
                {
                    MessageBox.Show($"لم يتم العثور على العنصر {TestName} للحفظ.");
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ أثناء حفظ نتيجة {TestName}: {ex.Message}");
            }
        }

        private bool CanSaveResult() => true;
    }
}