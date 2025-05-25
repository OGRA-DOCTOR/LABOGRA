using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LABOGRA.Models;
using LABOGRA.Services; // For IDatabaseService
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace LABOGRA.ViewModels
{
    public partial class LabOrderItemViewModel : ObservableObject
    {
        private readonly LabOrderItem _item;
        private readonly string _patientGender;
        private readonly IDatabaseService _databaseService;

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

        public LabOrderItemViewModel(LabOrderItem item, string patientGender, IDatabaseService databaseService)
        {
            _item = item ?? throw new ArgumentNullException(nameof(item));
            _patientGender = patientGender?.ToUpperInvariant() ?? "UNKNOWN";
            _databaseService = databaseService ?? throw new ArgumentNullException(nameof(databaseService));

            ResultValue = item.Result;
            IsSavedSuccessfully = !string.IsNullOrWhiteSpace(item.Result);
            DetermineReferenceValueAndUnit();
        }

        private void DetermineReferenceValueAndUnit()
        {
            Unit = "N/A"; // Default to N/A
            DisplayReferenceRange = "N/A"; // Default to N/A

            if (_item.Test?.ReferenceValues != null && _item.Test.ReferenceValues.Any())
            {
                var genderSpecificRef = _item.Test.ReferenceValues
                                             .FirstOrDefault(rv => rv.GenderSpecific != null && rv.GenderSpecific.Equals(_patientGender, StringComparison.OrdinalIgnoreCase));
                var generalRef = _item.Test.ReferenceValues
                                          .FirstOrDefault(rv => rv.GenderSpecific != null && rv.GenderSpecific.Equals("ANY", StringComparison.OrdinalIgnoreCase));
                var fallbackRef = _item.Test.ReferenceValues.FirstOrDefault();

                var selectedRef = genderSpecificRef ?? generalRef ?? fallbackRef;

                if (selectedRef != null)
                {
                    DisplayReferenceRange = selectedRef.ReferenceText ?? "N/A";
                    Unit = selectedRef.Unit ?? "N/A"; // Use unit from reference value if available
                }
            }
            // If no reference values or no suitable unit found, Unit remains "N/A"
        }

        [RelayCommand(CanExecute = nameof(CanSaveResult))]
        private async Task SaveResultAsync()
        {
            if (!CanSaveResult()) return;

            try
            {
                bool success = await _databaseService.UpdateLabOrderItemResultAsync(this.Id, this.ResultValue, this.Unit);

                if (success)
                {
                    IsSavedSuccessfully = true;
                }
                else
                {
                    IsSavedSuccessfully = false;
                    // Consider showing a more specific message if save operation explicitly returns false
                    // MessageBox.Show($"فشل حفظ النتيجة للعنصر {TestName}.", "فشل الحفظ", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                IsSavedSuccessfully = false;
                MessageBox.Show($"An unexpected error occurred while saving the result for {TestName}: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanSaveResult()
        {
            return !string.IsNullOrWhiteSpace(ResultValue) && !IsSavedSuccessfully;
        }

        partial void OnResultValueChanged(string? value)
        {
            IsSavedSuccessfully = false;
        }
    }
}