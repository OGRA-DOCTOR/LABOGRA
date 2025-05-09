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
    // تم إزالة تعريف LabOrderItemViewModel من هنا
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
        // هذا السطر يستخدم LabOrderItemViewModel، الآن سيتم العثور عليه في ملفه الخاص
        private ObservableCollection<LabOrderItemViewModel> labOrderItems = new ObservableCollection<LabOrderItemViewModel>();

        [ObservableProperty]
        private bool isPatientListLoaded = false;

        // خاصية محسوبة لتحديد ما إذا كان يجب تفعيل ComboBox اختيار المريض
        public bool IsPatientSelectionEnabled => IsPatientListLoaded && PatientList.Any();


        [RelayCommand(CanExecute = nameof(CanLoadTests))]
        private async Task LoadPatientTestsAsync()
        {
            if (SelectedPatient == null)
            {
                LabOrderItems.Clear(); // مسح قائمة التحاليل إذا لم يتم اختيار مريض
                return;
            }
            LabOrderItems.Clear(); // مسح القائمة القديمة قبل تحميل الجديدة
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
                        // هذا السطر يستخدم LabOrderItemViewModel
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
            // MessageBox.Show("ResultsViewModel: Constructor called. Loading initial patients..."); // يمكن إلغاء التعليق للتحقق
            _ = LoadInitialPatientsAsync();
        }

        private async Task LoadInitialPatientsAsync()
        {
            PatientList.Clear(); // مسح القائمة قبل البدء
            IsPatientListLoaded = false; // تعيين الحالة قبل التحميل
            OnPropertyChanged(nameof(IsPatientSelectionEnabled)); // إعلام الواجهة بتغيير حالة التفعيل

            try
            {
                // MessageBox.Show("ResultsViewModel: Attempting to load patients from DB..."); // يمكن إلغاء التعليق للتحقق
                var patientsFromDb = await _context.Patients
                                       .OrderByDescending(p => p.RegistrationDateTime)
                                       .ToListAsync();

                // MessageBox.Show($"ResultsViewModel: Found {patientsFromDb.Count} patients in DB."); // يمكن إلغاء التعليق للتحقق

                if (patientsFromDb != null) // تحقق إضافي
                {
                    foreach (var patient in patientsFromDb)
                    {
                        PatientList.Add(patient);
                    }
                }

                //  if (PatientList.Any())
                // {
                //     MessageBox.Show($"ResultsViewModel: Successfully added {PatientList.Count} patients to PatientList.");
                // }
                // else if(patientsFromDb.Any())
                // {
                //      MessageBox.Show($"ResultsViewModel: Patients found in DB ({patientsFromDb.Count}) but PatientList is still empty!");
                // }
                // else
                // {
                //      MessageBox.Show($"ResultsViewModel: No patients found in DB, PatientList is empty.");
                // }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while loading the patient list: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsPatientListLoaded = true; // تعيين الحالة بعد اكتمال التحميل
                OnPropertyChanged(nameof(IsPatientSelectionEnabled)); // إعلام الواجهة بتغيير حالة التفعيل
                // MessageBox.Show($"ResultsViewModel: LoadInitialPatientsAsync finished. PatientList final count: {PatientList.Count}. IsPatientSelectionEnabled: {IsPatientSelectionEnabled}"); // يمكن إلغاء التعليق للتحقق
            }
        }

        // تم التأكد من أن هذا الجزء يستدعي الأمر بشكل صحيح
        async partial void OnSelectedPatientChanged(Patient? value)
        {
            // عند تغيير المريض المختار، قم بتحميل تحاليله
            // التأكد من أن الأمر يمكن تنفيذه قبل استدعائه
            if (LoadPatientTestsCommand.CanExecute(null))
            {
                await LoadPatientTestsCommand.ExecuteAsync(null);
            }
            else if (value == null) // إذا تم إلغاء تحديد المريض (مثلاً أصبحت القائمة فارغة)
            {
                LabOrderItems.Clear(); // مسح قائمة التحاليل
            }
        }
    }
}