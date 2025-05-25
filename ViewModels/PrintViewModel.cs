// بداية الكود لملف ViewModels/PrintViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LABOGRA.Models;
using LABOGRA.Services; // For IDatabaseService
// using LABOGRA.Services.Database.Data; // No longer directly using LabDbContext
using Microsoft.EntityFrameworkCore; // May still be needed for some specific EF types or extension methods
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using LABOGRA.Views.Print; // For ReportPreviewWindow

namespace LABOGRA.ViewModels
{
    public partial class PrintViewModel : ObservableObject
    {
        private readonly IDatabaseService _databaseService; // Use IDatabaseService

        [ObservableProperty] private ObservableCollection<Patient> patientList = new ObservableCollection<Patient>();
        [ObservableProperty][NotifyPropertyChangedFor(nameof(IsPatientSelected))][NotifyCanExecuteChangedFor(nameof(LoadPatientOrdersCommand))] private Patient? selectedPatient;
        public bool IsPatientSelected => SelectedPatient != null;

        [ObservableProperty] private ObservableCollection<LabOrderA> patientLabOrders = new ObservableCollection<LabOrderA>();
        [ObservableProperty][NotifyPropertyChangedFor(nameof(IsLabOrderSelected))][NotifyCanExecuteChangedFor(nameof(PrintReportCommand))] private LabOrderA? selectedLabOrder;
        public bool IsLabOrderSelected => SelectedLabOrder?.Items != null && SelectedLabOrder.Items.Any();

        [ObservableProperty] private bool isLoading = false;

        // Constructor now takes IDatabaseService
        public PrintViewModel(IDatabaseService databaseService)
        {
            _databaseService = databaseService ?? throw new ArgumentNullException(nameof(databaseService));
            _ = LoadPatientsAsync();
        }

        private async Task LoadPatientsAsync()
        {
            IsLoading = true;
            PatientList.Clear();
            SelectedPatient = null;
            PatientLabOrders.Clear();
            SelectedLabOrder = null;
            try
            {
                // Use the database service
                var patientsFromDb = await _databaseService.GetPatientsAsync(); // Assumes this gets all needed data or sorts as needed
                foreach (var patient in patientsFromDb.OrderBy(p => p.Name)) // Manual sort if service doesn't
                {
                    PatientList.Add(patient);
                }
            }
            catch (Exception ex) { MessageBox.Show($"حدث خطأ أثناء تحميل قائمة المرضى: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error); }
            finally { IsLoading = false; }
        }

        [RelayCommand(CanExecute = nameof(CanLoadPatientOrders))]
        private async Task LoadPatientOrdersAsync()
        {
            if (SelectedPatient == null) { PatientLabOrders.Clear(); SelectedLabOrder = null; return; }
            IsLoading = true;
            PatientLabOrders.Clear();
            SelectedLabOrder = null; // Clear selected order when loading new ones
            try
            {
                // Use the database service
                var orders = await _databaseService.GetLabOrdersByPatientIdAsync(SelectedPatient.Id);

                foreach (var order in orders.OrderByDescending(o => o.RegistrationDateTime)) // Service method already includes items and tests
                {
                    // The service method GetLabOrdersByPatientIdAsync already includes Items.Test.
                    // We might need to ensure Test.ReferenceValues is included by the service method if ReportPreviewWindow needs it directly.
                    // For now, assuming ReportPreviewWindow can handle if Test.ReferenceValues is null or fetches it.
                    PatientLabOrders.Add(order);
                }
                SelectedLabOrder = PatientLabOrders.FirstOrDefault();
                // PrintReportCommand.NotifyCanExecuteChanged(); // Already handled by OnSelectedLabOrderChanged
            }
            catch (Exception ex) { MessageBox.Show($"حدث خطأ أثناء تحميل طلبات التحاليل للمريض: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error); }
            finally { IsLoading = false; }
        }

        private bool CanLoadPatientOrders() => SelectedPatient != null && !IsLoading;

        [RelayCommand(CanExecute = nameof(CanPrintReport))]
        private async Task PrintReportAsync()
        {
            if (SelectedPatient == null || SelectedLabOrder == null || SelectedLabOrder.Items == null || !SelectedLabOrder.Items.Any())
            {
                MessageBox.Show("الرجاء اختيار مريض وطلب تحليل يحتوي على عناصر لطباعتها.", "خطأ في الطباعة", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            IsLoading = true;
            try
            {
                // We need the full patient and lab order details.
                // The SelectedPatient and SelectedLabOrder might be summary objects.
                // It's safer to re-fetch them with all includes needed for the report.
                var patientToPrint = await _databaseService.GetPatientByIdAsync(SelectedPatient.Id); // Ensures all includes for patient
                var labOrderToPrint = await _databaseService.GetLabOrderByIdAsync(SelectedLabOrder.Id); // Ensures all includes for order

                if (labOrderToPrint != null && patientToPrint != null && labOrderToPrint.Items != null)
                {
                    bool allResultsEntered = labOrderToPrint.Items.All(item => !string.IsNullOrWhiteSpace(item.Result));
                    if (!allResultsEntered)
                    {
                        MessageBox.Show("لم يتم إدخال جميع نتائج التحاليل لهذا الطلب. الرجاء إكمال إدخال النتائج أولاً.", "نتائج غير مكتملة", MessageBoxButton.OK, MessageBoxImage.Warning);
                        IsLoading = false;
                        return;
                    }
                    // Pass the fully loaded entities to the preview window
                    ReportPreviewWindow previewWindow = new ReportPreviewWindow(patientToPrint, labOrderToPrint);
                    previewWindow.ShowDialog();
                }
                else
                {
                    MessageBox.Show("لم يتم العثور على بيانات الطلب أو المريض المطلوبة لإنشاء التقرير.", "خطأ في البيانات", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex) { MessageBox.Show($"حدث خطأ أثناء عملية التقرير: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error); }
            finally { IsLoading = false; /* PrintReportCommand.NotifyCanExecuteChanged(); // Handled by IsLoading change */ }
        }

        private bool CanPrintReport() => IsLabOrderSelected && !IsLoading; // IsLabOrderSelected already checks Items

        // These partial methods are automatically called by CommunityToolkit.Mvvm when the corresponding property changes.
        partial void OnSelectedPatientChanged(Patient? value)
        {
            if (LoadPatientOrdersCommand.CanExecute(null))
            {
                _ = LoadPatientOrdersCommand.ExecuteAsync(null);
            }
            else if (value == null)
            {
                PatientLabOrders.Clear();
                SelectedLabOrder = null;
            }
            // PrintReportCommand.NotifyCanExecuteChanged(); // Handled by OnSelectedLabOrderChanged or IsLabOrderSelected
        }

        partial void OnSelectedLabOrderChanged(LabOrderA? value)
        {
            // PrintReportCommand.NotifyCanExecuteChanged(); // This is automatically handled by [NotifyCanExecuteChangedFor] on SelectedLabOrder
        }

        // Partial method for IsLoading to update command states
        partial void OnIsLoadingChanged(bool value)
        {
            LoadPatientOrdersCommand.NotifyCanExecuteChanged();
            PrintReportCommand.NotifyCanExecuteChanged();
        }
    }
}
// نهاية الكود لملف ViewModels/PrintViewModel.cs