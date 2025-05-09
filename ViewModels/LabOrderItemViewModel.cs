using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LABOGRA.Models;
using LABOGRA.Services.Database.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace LABOGRA.ViewModels
{
    // هذا هو تعريف كلاس LabOrderItemViewModel المنقول إلى ملفه الخاص
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
        [NotifyCanExecuteChangedFor(nameof(SaveResultCommand))]
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
            if (!CanSaveResult()) return;

            try
            {
                var itemToUpdate = await _dbContext.LabOrderItems.FindAsync(this.Id);
                if (itemToUpdate != null)
                {
                    itemToUpdate.Result = this.ResultValue;
                    itemToUpdate.ResultUnit = this.Unit;
                    await _dbContext.SaveChangesAsync();
                    IsSavedSuccessfully = true;
                    // MessageBox.Show($"تم حفظ نتيجة {TestName}: {ResultValue}"); // تم التعليق على الرسالة
                }
                else
                {
                    // MessageBox.Show($"لم يتم العثور على العنصر {TestName} للحفظ.");
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
            return !string.IsNullOrWhiteSpace(ResultValue);
        }

        partial void OnResultValueChanged(string? value)
        {
            IsSavedSuccessfully = false;
        }
    }
}