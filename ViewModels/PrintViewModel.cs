// الإصدار: 3 (لهذا الملف - تصحيح مسار ReportPreviewWindow)
// اسم الملف: PrintViewModel.cs
// الوصف: ViewModel لطباعة التقارير مع تصحيح مسار ReportPreviewWindow.
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LABOGRA.Models;
using LABOGRA.Services;
using LABOGRA.Services.Database.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
// *** السطر التالي هو المهم للتأكد من الوصول الصحيح ***
using LABOGRA.Views.Print; // هذا يجب أن يكون صحيحًا بما أن ReportPreviewWindow في مجلد Print

namespace LABOGRA.ViewModels
{
    public partial class PrintViewModel : ObservableObject
    {
        private readonly LabDbContextFactory _contextFactory;

        // (باقي الخصائص كما كانت)
        [ObservableProperty] private ObservableCollection<Patient> patientList = new ObservableCollection<Patient>();
        [ObservableProperty][NotifyPropertyChangedFor(nameof(IsPatientSelected))][NotifyCanExecuteChangedFor(nameof(LoadPatientOrdersCommand))] private Patient? selectedPatient;
        public bool IsPatientSelected => SelectedPatient != null;
        [ObservableProperty] private ObservableCollection<LabOrderA> patientLabOrders = new ObservableCollection<LabOrderA>();
        [ObservableProperty][NotifyPropertyChangedFor(nameof(IsLabOrderSelected))][NotifyCanExecuteChangedFor(nameof(PrintReportCommand))] private LabOrderA? selectedLabOrder;
        public bool IsLabOrderSelected => SelectedLabOrder != null && SelectedLabOrder.Items != null && SelectedLabOrder.Items.Any();
        [ObservableProperty] private bool isLoading = false;


        [RelayCommand(CanExecute = nameof(CanLoadPatientOrders))]
        private async Task LoadPatientOrdersAsync()
        {
            // (كود هذه الدالة كما كان في الإصدار السابق - لم يتغير)
            if (SelectedPatient == null) { PatientLabOrders.Clear(); SelectedLabOrder = null; return; }
            IsLoading = true; PatientLabOrders.Clear();
            try { using (var context = _contextFactory.CreateDbContext(Array.Empty<string>())) { var orders = await context.LabOrders.Include(o => o.Items).ThenInclude(item => item.Test).ThenInclude(test => test.ReferenceValues).Where(o => o.PatientId == SelectedPatient.Id).OrderByDescending(o => o.RegistrationDateTime).ToListAsync(); Application.Current.Dispatcher.Invoke(() => { foreach (var order in orders) { if (order is LabOrderA labOrderAInstance) { PatientLabOrders.Add(labOrderAInstance); } } SelectedLabOrder = PatientLabOrders.FirstOrDefault(); PrintReportCommand.NotifyCanExecuteChanged(); }); } }
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
                using (var context = _contextFactory.CreateDbContext(Array.Empty<string>()))
                {
                    var patientToPrint = await context.Patients
                                           .Include(p => p.ReferringPhysician)
                                           .Where(p => p.Id == SelectedPatient.Id)
                                           .FirstOrDefaultAsync();

                    var labOrderToPrint = await context.LabOrders
                       .Include(o => o.Items)
                           .ThenInclude(item => item.Test)
                               .ThenInclude(test => test.ReferenceValues)
                       .Where(o => o.Id == SelectedLabOrder.Id)
                       .FirstOrDefaultAsync();

                    if (labOrderToPrint != null && patientToPrint != null)
                    {
                        bool allResultsEntered = labOrderToPrint.Items?.All(item => !string.IsNullOrWhiteSpace(item.Result)) ?? false;

                        if (!allResultsEntered)
                        {
                            MessageBox.Show("لم يتم إدخال جميع نتائج التحاليل لهذا الطلب. الرجاء إكمال إدخال النتائج أولاً.", "نتائج غير مكتملة", MessageBoxButton.OK, MessageBoxImage.Warning);
                            IsLoading = false;
                            return;
                        }

                        // *** التأكد من استخدام المسار (namespace) الصحيح عند إنشاء النافذة ***
                        // بما أن using LABOGRA.Views.Print; موجود في الأعلى، يجب أن يعمل هذا مباشرة
                        ReportPreviewWindow previewWindow = new ReportPreviewWindow(patientToPrint, labOrderToPrint);
                        previewWindow.ShowDialog();
                    }
                    else
                    {
                        MessageBox.Show("لم يتم العثور على بيانات الطلب أو المريض المطلوبة لإنشاء التقرير.", "خطأ في البيانات", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء عملية التقرير: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
                PrintReportCommand.NotifyCanExecuteChanged();
            }
        }

        private bool CanPrintReport() => SelectedLabOrder != null && SelectedLabOrder.Items != null && SelectedLabOrder.Items.Any() && !IsLoading;

        public PrintViewModel()
        {
            _contextFactory = new LabDbContextFactory();
            _ = LoadPatientsAsync();
        }

        private async Task LoadPatientsAsync()
        {
            // (كود هذه الدالة كما كان في الإصدار السابق - لم يتغير)
            IsLoading = true; PatientList.Clear(); SelectedPatient = null; PatientLabOrders.Clear(); SelectedLabOrder = null;
            try { using (var context = _contextFactory.CreateDbContext(Array.Empty<string>())) { var patientsFromDb = await context.Patients.OrderBy(p => p.Name).ToListAsync(); Application.Current.Dispatcher.Invoke(() => { foreach (var patient in patientsFromDb) { PatientList.Add(patient); } }); } }
            catch (Exception ex) { MessageBox.Show($"حدث خطأ أثناء تحميل قائمة المرضى: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error); }
            finally { IsLoading = false; }
        }

        // (باقي الدوال كما كانت)
        partial void OnSelectedPatientChanged(Patient? value) { if (LoadPatientOrdersCommand.CanExecute(null)) { _ = LoadPatientOrdersCommand.ExecuteAsync(null); } else if (value == null) { PatientLabOrders.Clear(); SelectedLabOrder = null; } PrintReportCommand.NotifyCanExecuteChanged(); }
        partial void OnSelectedLabOrderChanged(LabOrderA? value) { PrintReportCommand.NotifyCanExecuteChanged(); }
    }
}