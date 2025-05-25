// بداية الكود لملف ViewModels/PrintViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LABOGRA.Models;
// using LABOGRA.Services; // هذا لم يعد مستخدماً مباشرة هنا، قد يكون داخل ReportPreviewWindow
using LABOGRA.Services.Database.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using LABOGRA.Views.Print;

namespace LABOGRA.ViewModels
{
    public partial class PrintViewModel : ObservableObject
    {
        private readonly LabDbContext _dbContext; // تعديل: لاستقبال DbContext مباشرة

        [ObservableProperty] private ObservableCollection<Patient> patientList = new ObservableCollection<Patient>();
        [ObservableProperty][NotifyPropertyChangedFor(nameof(IsPatientSelected))][NotifyCanExecuteChangedFor(nameof(LoadPatientOrdersCommand))] private Patient? selectedPatient;
        public bool IsPatientSelected => SelectedPatient != null;
        [ObservableProperty] private ObservableCollection<LabOrderA> patientLabOrders = new ObservableCollection<LabOrderA>();
        [ObservableProperty][NotifyPropertyChangedFor(nameof(IsLabOrderSelected))][NotifyCanExecuteChangedFor(nameof(PrintReportCommand))] private LabOrderA? selectedLabOrder;
        public bool IsLabOrderSelected => SelectedLabOrder?.Items != null && SelectedLabOrder.Items.Any();
        [ObservableProperty] private bool isLoading = false;

        [RelayCommand(CanExecute = nameof(CanLoadPatientOrders))]
        private async Task LoadPatientOrdersAsync()
        {
            if (SelectedPatient == null) { PatientLabOrders.Clear(); SelectedLabOrder = null; return; }
            IsLoading = true; PatientLabOrders.Clear();
            try
            {
                // استخدام _dbContext مباشرة
                var orders = await _dbContext.LabOrders
                                .Include(o => o.Items).ThenInclude(item => item.Test).ThenInclude(test => test.ReferenceValues)
                                .Where(o => o.PatientId == SelectedPatient.Id)
                                .OrderByDescending(o => o.RegistrationDateTime)
                                .ToListAsync();

                // لا حاجة لـ Application.Current.Dispatcher.Invoke إذا كانت هذه الدالة تُستدعى من thread الواجهة
                // وإذا كانت تُستدعى من thread آخر، فيجب أن يكون هناك سبب وجيه لذلك
                foreach (var order in orders)
                {
                    if (order is LabOrderA labOrderAInstance) // التأكد من النوع
                    {
                        PatientLabOrders.Add(labOrderAInstance);
                    }
                }
                SelectedLabOrder = PatientLabOrders.FirstOrDefault();
                PrintReportCommand.NotifyCanExecuteChanged();
            }
            catch (Exception ex) { MessageBox.Show($"حدث خطأ أثناء تحميل طلبات التحاليل للمريض: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error); }
            finally { IsLoading = false; }
        }

        private bool CanLoadPatientOrders() => SelectedPatient != null && !IsLoading;

        [RelayCommand(CanExecute = nameof(CanPrintReport))]
        private async Task PrintReportAsync()
        {
            if (SelectedPatient == null || SelectedLabOrder?.Items == null || !SelectedLabOrder.Items.Any())
            {
                MessageBox.Show("الرجاء اختيار مريض وطلب تحليل يحتوي على عناصر لطباعتها.", "خطأ في الطباعة", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            IsLoading = true;
            try
            {
                // استخدام _dbContext مباشرة
                var patientToPrint = await _dbContext.Patients
                                       .Include(p => p.ReferringPhysician)
                                       .FirstOrDefaultAsync(p => p.Id == SelectedPatient.Id);

                var labOrderToPrint = await _dbContext.LabOrders
                                       .Include(o => o.Items)
                                           .ThenInclude(item => item.Test)
                                               .ThenInclude(test => test.ReferenceValues)
                                       .FirstOrDefaultAsync(o => o.Id == SelectedLabOrder.Id);

                if (labOrderToPrint != null && patientToPrint != null)
                {
                    bool allResultsEntered = labOrderToPrint.Items.All(item => !string.IsNullOrWhiteSpace(item.Result));
                    if (!allResultsEntered)
                    {
                        MessageBox.Show("لم يتم إدخال جميع نتائج التحاليل لهذا الطلب. الرجاء إكمال إدخال النتائج أولاً.", "نتائج غير مكتملة", MessageBoxButton.OK, MessageBoxImage.Warning);
                        IsLoading = false;
                        return;
                    }
                    ReportPreviewWindow previewWindow = new ReportPreviewWindow(patientToPrint, labOrderToPrint);
                    previewWindow.ShowDialog();
                }
                else
                {
                    MessageBox.Show("لم يتم العثور على بيانات الطلب أو المريض المطلوبة لإنشاء التقرير.", "خطأ في البيانات", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex) { MessageBox.Show($"حدث خطأ أثناء عملية التقرير: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error); }
            finally { IsLoading = false; PrintReportCommand.NotifyCanExecuteChanged(); }
        }

        private bool CanPrintReport() => SelectedLabOrder?.Items != null && SelectedLabOrder.Items.Any() && !IsLoading;

        // تعديل المنشئ ليقبل LabDbContext
        public PrintViewModel(LabDbContext dbContext)
        {
            _dbContext = dbContext; // تخزين DbContext المستلم
            _ = LoadPatientsAsync();
        }

        private async Task LoadPatientsAsync()
        {
            IsLoading = true; PatientList.Clear(); SelectedPatient = null; PatientLabOrders.Clear(); SelectedLabOrder = null;
            try
            {
                // استخدام _dbContext مباشرة
                var patientsFromDb = await _dbContext.Patients.OrderBy(p => p.Name).ToListAsync();
                foreach (var patient in patientsFromDb)
                {
                    PatientList.Add(patient);
                }
            }
            catch (Exception ex) { MessageBox.Show($"حدث خطأ أثناء تحميل قائمة المرضى: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error); }
            finally { IsLoading = false; }
        }

        partial void OnSelectedPatientChanged(Patient? value) { if (LoadPatientOrdersCommand.CanExecute(null)) { _ = LoadPatientOrdersCommand.ExecuteAsync(null); } else if (value == null) { PatientLabOrders.Clear(); SelectedLabOrder = null; } PrintReportCommand.NotifyCanExecuteChanged(); }
        partial void OnSelectedLabOrderChanged(LabOrderA? value) { PrintReportCommand.NotifyCanExecuteChanged(); }
    }
}
// نهاية الكود لملف ViewModels/PrintViewModel.cs