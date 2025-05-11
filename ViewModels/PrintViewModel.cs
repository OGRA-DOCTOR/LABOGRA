using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LABOGRA.Models;
using LABOGRA.Services; // لاستخدام PatientReportGenerator
using LABOGRA.Services.Database.Data; // لاستخدام LabDbContextFactory و LabDbContext
using Microsoft.EntityFrameworkCore; // لاستخدام Include و ToListAsync و OrderByDescending و FirstOrDefaultAsync
using System;
using System.Collections.ObjectModel; // لاستخدام ObservableCollection
using System.Linq; // لاستخدام Linq methods مثل Any و FirstOrDefault
using System.Threading.Tasks; // لاستخدام Task و async/await
using System.Windows; // لاستخدام MessageBox و Application.Current.Dispatcher
// using System.IO; // لم نعد نحتاج هذا هنا مباشرة، سينتقل لنافذة المعاينة

// تأكد من أن هذا هو الـ namespace الصحيح لنافذة المعاينة عندما تنشئها
// إذا وضعتها في مجلد Views/Print مثل PrintView، قد يكون هذا صحيحاً.
// أو إذا وضعتها في مجلد Views مباشرة، سيكون using LABOGRA.Views;
// سنقوم بتعديل هذا السطر إذا لزم الأمر بعد إنشاء نافذة المعاينة.
// حالياً، إذا لم تكن النافذة موجودة، قد يظهر تحذير أو خطأ تحته، تجاهله مؤقتاً.
using LABOGRA.Views; // افترض أن ReportPreviewWindow ستكون في مجلد Views مباشرة

namespace LABOGRA.ViewModels
{
    public partial class PrintViewModel : ObservableObject
    {
        private readonly LabDbContextFactory _contextFactory;
        // لم نعد نحتاج _reportGenerator هنا مباشرة، سينتقل لنافذة المعاينة
        // private readonly PatientReportGenerator _reportGenerator;

        [ObservableProperty]
        private ObservableCollection<Patient> patientList = new ObservableCollection<Patient>();

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsPatientSelected))]
        [NotifyCanExecuteChangedFor(nameof(LoadPatientOrdersCommand))]
        private Patient? selectedPatient;

        public bool IsPatientSelected => SelectedPatient != null;

        [ObservableProperty]
        private ObservableCollection<LabOrderA> patientLabOrders = new ObservableCollection<LabOrderA>(); // تم التعديل إلى LabOrderA

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsLabOrderSelected))]
        [NotifyCanExecuteChangedFor(nameof(PrintReportCommand))]
        private LabOrderA? selectedLabOrder; // تم التعديل إلى LabOrderA

        public bool IsLabOrderSelected => SelectedLabOrder != null &&
            SelectedLabOrder.Items != null &&
            SelectedLabOrder.Items.Any();

        [ObservableProperty]
        private bool isLoading = false;

        [RelayCommand(CanExecute = nameof(CanLoadPatientOrders))]
        private async Task LoadPatientOrdersAsync()
        {
            if (SelectedPatient == null)
            {
                PatientLabOrders.Clear();
                SelectedLabOrder = null;
                return;
            }

            IsLoading = true;
            PatientLabOrders.Clear();

            try
            {
                using (var context = _contextFactory.CreateDbContext(Array.Empty<string>()))
                {
                    var orders = await context.LabOrders // تأكد من أن اسم الجدول هنا هو LabOrders أو LabOrderAs إذا كان اسم الكلاس LabOrderA
                        .Include(o => o.Items)
                            .ThenInclude(item => item.Test)
                                .ThenInclude(test => test.ReferenceValues)
                        .Where(o => o.PatientId == SelectedPatient.Id)
                        .OrderByDescending(o => o.RegistrationDateTime)
                        .ToListAsync();

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        foreach (var order in orders)
                        {
                            // يجب أن يكون نوع order هنا مطابقاً لنوع العناصر في PatientLabOrders (LabOrderA)
                            if (order is LabOrderA labOrderAInstance) // تحقق من النوع قبل الإضافة
                            {
                                PatientLabOrders.Add(labOrderAInstance);
                            }
                            // إذا كان order من نوع LabOrder وتحتاج لتحويله أو لديك LabOrderAs في قاعدة البيانات:
                            // PatientLabOrders.Add(order); // هذا السطر سيعمل إذا كان نوع order هو LabOrderA
                        }
                        SelectedLabOrder = PatientLabOrders.FirstOrDefault();
                        PrintReportCommand.NotifyCanExecuteChanged();
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء تحميل طلبات التحاليل للمريض: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
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
                    // تم التعديل لجلب LabOrderA
                    var labOrderToPrint = await context.LabOrders // أو LabOrderAs إذا كان هذا اسم DbSet
                       .Include(o => o.Items)
                           .ThenInclude(item => item.Test)
                               .ThenInclude(test => test.ReferenceValues)
                       .Where(o => o.Id == SelectedLabOrder.Id)
                       .FirstOrDefaultAsync();

                    var patientToPrint = await context.Patients
                                           .Where(p => p.Id == SelectedPatient.Id)
                                           .FirstOrDefaultAsync();

                    if (labOrderToPrint != null && patientToPrint != null)
                    {
                        // *** بداية التحقق من إدخال جميع النتائج ***
                        bool allResultsEntered = true;
                        if (labOrderToPrint.Items == null || !labOrderToPrint.Items.Any()) // تحقق إضافي للأمان
                        {
                            allResultsEntered = false; // لا يوجد تحاليل لطباعتها
                        }
                        else
                        {
                            foreach (var item in labOrderToPrint.Items)
                            {
                                if (string.IsNullOrWhiteSpace(item.Result))
                                {
                                    allResultsEntered = false;
                                    break; // وجدنا نتيجة واحدة على الأقل غير مدخلة، لا داعي لإكمال الحلقة
                                }
                            }
                        }

                        if (!allResultsEntered)
                        {
                            MessageBox.Show("لم يتم إدخال جميع نتائج التحاليل لهذا الطلب. الرجاء إكمال إدخال النتائج أولاً.", "نتائج غير مكتملة", MessageBoxButton.OK, MessageBoxImage.Warning);
                            IsLoading = false; // تأكد من إيقاف التحميل
                            return; // لا تكمل العملية
                        }
                        // *** نهاية التحقق من إدخال جميع النتائج ***

                        // إذا كانت جميع النتائج مدخلة، افتح نافذة المعاينة
                        // تأكد من أن ReportPreviewWindow موجودة في الـ namespace الصحيح (LABOGRA.Views)
                        // وأنها تستقبل (Patient, LabOrderA) في الـ constructor
                        ReportPreviewWindow previewWindow = new ReportPreviewWindow(patientToPrint, labOrderToPrint);
                        previewWindow.ShowDialog(); // استخدام ShowDialog لجعلها نافذة مشروطة (modal)

                        // لم نعد ننشئ أو نحفظ الـ PDF هنا
                        // تم نقل منطق توليد وحفظ الـ PDF وفتح الملف إلى نافذة المعاينة
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

        private bool CanPrintReport() =>
            SelectedLabOrder != null &&
            SelectedLabOrder.Items != null &&
            SelectedLabOrder.Items.Any() &&
            !IsLoading;

        public PrintViewModel()
        {
            _contextFactory = new LabDbContextFactory();
            // لم نعد ننشئ _reportGenerator هنا
            // _reportGenerator = new PatientReportGenerator(); 
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
                using (var context = _contextFactory.CreateDbContext(Array.Empty<string>()))
                {
                    var patientsFromDb = await context.Patients
                                       .OrderBy(p => p.Name)
                                       .ToListAsync();

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        foreach (var patient in patientsFromDb)
                        {
                            PatientList.Add(patient);
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء تحميل قائمة المرضى: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

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
            PrintReportCommand.NotifyCanExecuteChanged();
        }

        partial void OnSelectedLabOrderChanged(LabOrderA? value) // تم التعديل إلى LabOrderA
        {
            PrintReportCommand.NotifyCanExecuteChanged();
        }
    }
}