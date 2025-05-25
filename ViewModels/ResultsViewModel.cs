// بداية الكود لملف ViewModels/ResultsViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LABOGRA.Models;
using LABOGRA.Services; // For IDatabaseService
// using LABOGRA.Services.Database.Data; // No longer directly using LabDbContext
using Microsoft.EntityFrameworkCore; // Still needed for specific EF features if used (like ThenInclude, or specific query patterns)
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace LABOGRA.ViewModels
{
    public partial class ResultsViewModel : ObservableObject
    {
        private readonly IDatabaseService _databaseService; // Use IDatabaseService

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

        // Constructor now takes IDatabaseService
        public ResultsViewModel(IDatabaseService databaseService)
        {
            _databaseService = databaseService ?? throw new ArgumentNullException(nameof(databaseService));
            _ = LoadInitialPatientsAsync();
        }

        private async Task LoadInitialPatientsAsync()
        {
            PatientList.Clear();
            IsPatientListLoaded = false;
            OnPropertyChanged(nameof(IsPatientSelectionEnabled)); // Ensure UI updates
            try
            {
                // Use the database service to get patients
                var patientsFromDb = await _databaseService.GetPatientsAsync(); // Assuming this sorts by RegistrationDateTime desc
                if (patientsFromDb != null)
                {
                    foreach (var patient in patientsFromDb.OrderByDescending(p => p.RegistrationDateTime)) // Manual sort if service doesn't
                    {
                        PatientList.Add(patient);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while loading the patient list: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsPatientListLoaded = true;
                OnPropertyChanged(nameof(IsPatientSelectionEnabled)); // Ensure UI updates
            }
        }

        [RelayCommand(CanExecute = nameof(CanLoadTests))]
        private async Task LoadPatientTestsAsync()
        {
            LabOrderItems.Clear();

            if (SelectedPatient == null)
            {
                Debug.WriteLine("LoadPatientTestsAsync: SelectedPatient is null. Clearing items.");
                return;
            }

            Debug.WriteLine($"LoadPatientTestsAsync: Loading tests for Patient ID: {SelectedPatient.Id}, Name: {SelectedPatient.Name}");

            try
            {
                // Get the latest lab order for the selected patient using the service
                // IDatabaseService needs a method like GetLatestLabOrderWithDetailsByPatientIdAsync
                // For now, let's assume GetLabOrdersByPatientIdAsync returns them and we pick the latest.
                var patientOrders = await _databaseService.GetLabOrdersByPatientIdAsync(SelectedPatient.Id);
                var orderToProcess = patientOrders.OrderByDescending(o => o.RegistrationDateTime).FirstOrDefault();

                if (orderToProcess == null)
                {
                    Debug.WriteLine($"LoadPatientTestsAsync: No LabOrder found for Patient ID: {SelectedPatient.Id}");
                    MessageBox.Show($"لم يتم العثور على طلبات تحاليل للمريض المحدد.", "لا توجد طلبات", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                Debug.WriteLine($"LoadPatientTestsAsync: Found LabOrder ID: {orderToProcess.Id} with RegistrationDate: {orderToProcess.RegistrationDateTime}");

                // Ensure Items and Test and ReferenceValues are loaded.
                // The GetLabOrdersByPatientIdAsync in DatabaseService already includes Items.Test.
                // We might need to ensure ReferenceValues are loaded if not already.
                // For simplicity, let's assume the service method handles includes properly.

                if (orderToProcess.Items == null || !orderToProcess.Items.Any())
                {
                    Debug.WriteLine($"LoadPatientTestsAsync: LabOrder ID: {orderToProcess.Id} has no items or Items collection is null.");
                    MessageBox.Show($"الطلب المخبري للمريض لا يحتوي على أي تحاليل.", "لا توجد تحاليل", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                Debug.WriteLine($"LoadPatientTestsAsync: LabOrder ID: {orderToProcess.Id} has {orderToProcess.Items.Count} items. Processing them...");

                foreach (var item in orderToProcess.Items.OrderBy(i => i.Test?.Name)) // Test might be null if data is inconsistent
                {
                    if (item.Test == null)
                    {
                        Debug.WriteLine($"LoadPatientTestsAsync: Item ID: {item.Id} has a null Test object. Skipping.");
                        continue; // Skip items with no associated Test
                    }

                    // Ensure Test.ReferenceValues are loaded if DetermineReferenceValueAndUnit relies on them being populated
                    // This might require another call or adjustment in the service if not already included
                    // For now, assume Test object from 'orderToProcess.Items' has ReferenceValues loaded by the service.

                    Debug.WriteLine($"LoadPatientTestsAsync: Adding item: Test Name='{item.Test.Name}', Item ID={item.Id}");
                    // *** Pass the injected _databaseService ***
                    LabOrderItems.Add(new LabOrderItemViewModel(item, SelectedPatient.Gender, _databaseService));
                }

                if (!LabOrderItems.Any() && orderToProcess.Items.Any(i => i.Test != null)) // Check if there were processable items
                {
                    Debug.WriteLine($"LoadPatientTestsAsync: Finished processing items, but LabOrderItems collection is still empty despite having valid order items.");
                }
                else if (LabOrderItems.Any())
                {
                    Debug.WriteLine($"LoadPatientTestsAsync: Successfully added {LabOrderItems.Count} items to LabOrderItems collection.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"LoadPatientTestsAsync: Exception caught: {ex.ToString()}");
                MessageBox.Show($"An error occurred while loading patient tests: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanLoadTests() => SelectedPatient != null;

        async partial void OnSelectedPatientChanged(Patient? value)
        {
            Debug.WriteLine($"OnSelectedPatientChanged: Patient changed to ID: {value?.Id}, Name: {value?.Name}");
            if (LoadPatientTestsCommand.CanExecute(null))
            {
                Debug.WriteLine($"OnSelectedPatientChanged: Executing LoadPatientTestsCommand.");
                await LoadPatientTestsCommand.ExecuteAsync(null);
            }
            else if (value == null) // Explicitly clear if patient is deselected
            {
                Debug.WriteLine($"OnSelectedPatientChanged: SelectedPatient is null. Clearing LabOrderItems.");
                LabOrderItems.Clear();
            }
            else
            {
                // This case might occur if CanLoadTests becomes false for other reasons while value is not null
                Debug.WriteLine($"OnSelectedPatientChanged: LoadPatientTestsCommand cannot execute. CanLoadTests: {CanLoadTests()}");
            }
        }
    }
}
// نهاية الكود لملف ViewModels/ResultsViewModel.cs