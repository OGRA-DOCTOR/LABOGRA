// بداية الكود لملف ViewModels/ResultsViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LABOGRA.Models;
using LABOGRA.Services.Database.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics; // *** إضافة هذا السطر ***
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace LABOGRA.ViewModels
{
    public partial class ResultsViewModel : ObservableObject
    {
        private readonly LabDbContext _dbContext;

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
            LabOrderItems.Clear(); // مسح القائمة دائماً في البداية

            if (SelectedPatient == null)
            {
                Debug.WriteLine("LoadPatientTestsAsync: SelectedPatient is null. Clearing items.");
                return;
            }

            Debug.WriteLine($"LoadPatientTestsAsync: Loading tests for Patient ID: {SelectedPatient.Id}, Name: {SelectedPatient.Name}");

            try
            {
                var orderToProcess = await _dbContext.LabOrders
                                        .Include(o => o.Items) // تضمين عناصر الطلب
                                            .ThenInclude(i => i.Test) // لكل عنصر، ضمّن التحليل المرتبط به
                                                .ThenInclude(t => t.ReferenceValues) // لكل تحليل، ضمّن القيم المرجعية
                                        .Where(o => o.PatientId == SelectedPatient.Id)
                                        .OrderByDescending(o => o.RegistrationDateTime)
                                        .FirstOrDefaultAsync();

                if (orderToProcess == null)
                {
                    Debug.WriteLine($"LoadPatientTestsAsync: No LabOrder found for Patient ID: {SelectedPatient.Id}");
                    MessageBox.Show($"لم يتم العثور على طلبات تحاليل للمريض المحدد.", "لا توجد طلبات", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                Debug.WriteLine($"LoadPatientTestsAsync: Found LabOrder ID: {orderToProcess.Id} with RegistrationDate: {orderToProcess.RegistrationDateTime}");

                if (orderToProcess.Items == null || !orderToProcess.Items.Any())
                {
                    Debug.WriteLine($"LoadPatientTestsAsync: LabOrder ID: {orderToProcess.Id} has no items or Items collection is null.");
                    MessageBox.Show($"الطلب المخبري للمريض لا يحتوي على أي تحاليل.", "لا توجد تحاليل", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                Debug.WriteLine($"LoadPatientTestsAsync: LabOrder ID: {orderToProcess.Id} has {orderToProcess.Items.Count} items. Processing them...");

                foreach (var item in orderToProcess.Items.OrderBy(i => i.Test?.Name))
                {
                    if (item.Test == null)
                    {
                        Debug.WriteLine($"LoadPatientTestsAsync: Item ID: {item.Id} has a null Test object. Skipping.");
                        continue;
                    }
                    Debug.WriteLine($"LoadPatientTestsAsync: Adding item: Test Name='{item.Test.Name}', Item ID={item.Id}");
                    LabOrderItems.Add(new LabOrderItemViewModel(item, SelectedPatient.Gender, _dbContext));
                }

                if (!LabOrderItems.Any())
                {
                    Debug.WriteLine($"LoadPatientTestsAsync: Finished processing items, but LabOrderItems collection is still empty.");
                }
                else
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

        public ResultsViewModel(LabDbContext dbContext)
        {
            _dbContext = dbContext;
            _ = LoadInitialPatientsAsync();
        }

        private async Task LoadInitialPatientsAsync()
        {
            PatientList.Clear();
            IsPatientListLoaded = false;
            OnPropertyChanged(nameof(IsPatientSelectionEnabled));
            try
            {
                var patientsFromDb = await _dbContext.Patients
                                       .OrderByDescending(p => p.RegistrationDateTime)
                                       .ToListAsync();
                if (patientsFromDb != null)
                {
                    foreach (var patient in patientsFromDb)
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
                OnPropertyChanged(nameof(IsPatientSelectionEnabled));
            }
        }

        async partial void OnSelectedPatientChanged(Patient? value)
        {
            Debug.WriteLine($"OnSelectedPatientChanged: Patient changed to ID: {value?.Id}, Name: {value?.Name}");
            if (LoadPatientTestsCommand.CanExecute(null))
            {
                Debug.WriteLine($"OnSelectedPatientChanged: Executing LoadPatientTestsCommand.");
                await LoadPatientTestsCommand.ExecuteAsync(null);
            }
            else if (value == null)
            {
                Debug.WriteLine($"OnSelectedPatientChanged: SelectedPatient is null. Clearing LabOrderItems.");
                LabOrderItems.Clear();
            }
            else
            {
                Debug.WriteLine($"OnSelectedPatientChanged: LoadPatientTestsCommand cannot execute. CanLoadTests: {CanLoadTests()}");
            }
        }
    }
}
// نهاية الكود لملف ViewModels/ResultsViewModel.cs