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
using System.IO; // لحفظ الملف

namespace LABOGRA.ViewModels
{
    public partial class PrintViewModel : ObservableObject
    {
        private readonly LabDbContextFactory _contextFactory;
        private readonly PatientReportGenerator _reportGenerator;

        // قائمة جميع المرضى التي ستعرض في الواجهة
        [ObservableProperty]
        private ObservableCollection<Patient> patientList = new ObservableCollection<Patient>();

        // المريض المختار من قائمة المرضى في الواجهة
        [ObservableProperty]
        // إعلام الواجهة والأوامر المرتبطة عند تغيير حالة تحديد المريض
        [NotifyPropertyChangedFor(nameof(IsPatientSelected))]
        [NotifyCanExecuteChangedFor(nameof(LoadPatientOrdersCommand))]
        private Patient? selectedPatient;

        // خاصية مساعدة لمعرفة ما إذا تم اختيار مريض (لتفعيل عناصر الواجهة)
        public bool IsPatientSelected => SelectedPatient != null;

        // قائمة طلبات التحاليل (LabOrders) للمريض المختار
        [ObservableProperty]
        private ObservableCollection<LabOrder> patientLabOrders = new ObservableCollection<LabOrder>();

        // طلب التحاليل (LabOrder) المختار من قائمة الطلبات للمريض المحدد
        [ObservableProperty]
        // إعلام الواجهة والأوامر المرتبطة عند تغيير حالة تحديد طلب التحليل
        [NotifyPropertyChangedFor(nameof(IsLabOrderSelected))]
        [NotifyCanExecuteChangedFor(nameof(PrintReportCommand))]
        private LabOrder? selectedLabOrder;

        // خاصية مساعدة لمعرفة ما إذا تم اختيار طلب تحليل (لتفعيل زر الطباعة)
        public bool IsLabOrderSelected => SelectedLabOrder != null;

        // خاصية لعرض مؤشر تحميل في الواجهة (اختياري)
        [ObservableProperty]
        private bool isLoading = false;

        // أمر لتحميل طلبات التحاليل للمريض المختار
        [RelayCommand(CanExecute = nameof(CanLoadPatientOrders))]
        private async Task LoadPatientOrdersAsync()
        {
            // لا تقم بالتحميل إذا لم يتم اختيار مريض
            if (SelectedPatient == null)
            {
                PatientLabOrders.Clear();
                SelectedLabOrder = null; // مسح تحديد الطلب السابق
                return;
            }

            IsLoading = true; // تفعيل مؤشر التحميل
            PatientLabOrders.Clear(); // مسح قائمة الطلبات الحالية قبل التحميل

            try
            {
                // استخدام Factory لإنشاء Context جديد لكل عملية قاعدة بيانات
                using (var context = _contextFactory.CreateDbContext(Array.Empty<string>()))
                {
                    // جلب طلبات التحاليل الخاصة بالمريض المختار
                    // تضمين (Include) عناصر الطلب (Items) ونوع التحليل (Test) والقيم المرجعية (ReferenceValues)
                    // هذا يضمن تحميل كافة البيانات اللازمة للتقرير دفعة واحدة
                    var orders = await context.LabOrders
                        .Include(o => o.Items)
                            .ThenInclude(item => item.Test)
                                .ThenInclude(test => test.ReferenceValues) // لضمان وجود ReferenceValues في Test
                        .Where(o => o.PatientId == SelectedPatient.Id) // تصفية حسب المريض المختار
                        .OrderByDescending(o => o.RegistrationDateTime) // ترتيب حسب تاريخ التسجيل (الأحدث أولاً)
                        .ToListAsync(); // تنفيذ الاستعلام وجلب البيانات

                    // تحديث ObservableCollection على واجهة المستخدم UI Thread
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        foreach (var order in orders)
                        {
                            PatientLabOrders.Add(order);
                        }
                        // تحديد أول طلب في القائمة بشكل افتراضي إذا كانت غير فارغة
                        SelectedLabOrder = PatientLabOrders.FirstOrDefault();
                    });
                }
            }
            catch (Exception ex)
            {
                // عرض رسالة خطأ إذا حدثت مشكلة أثناء التحميل
                MessageBox.Show($"حدث خطأ أثناء تحميل طلبات التحاليل للمريض: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false; // تعطيل مؤشر التحميل
            }
        }

        // شرط لتنفيذ أمر LoadPatientOrdersCommand: يمكن التنفيذ فقط إذا تم اختيار مريض وليس هناك عملية تحميل جارية
        private bool CanLoadPatientOrders() => SelectedPatient != null && !IsLoading;


        // أمر لطباعة التقرير (إنشاء ملف PDF)
        [RelayCommand(CanExecute = nameof(CanPrintReport))]
        private async Task PrintReportAsync()
        {
            // التحقق من أن هناك مريضاً وطلب تحليل مختارين وأن الطلب يحتوي على عناصر
            if (SelectedPatient == null || SelectedLabOrder == null || !SelectedLabOrder.Items.Any())
            {
                MessageBox.Show("الرجاء اختيار مريض وطلب تحليل يحتوي على نتائج لطباعتها.", "خطأ في الطباعة", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            IsLoading = true; // تفعيل مؤشر التحميل

            try
            {
                // إعادة جلب الطلب من قاعدة البيانات مع تضمين البيانات المرتبطة (التحاليل والقيم المرجعية)
                // للتأكد من أن البيانات كاملة وحديثة قبل إنشاء التقرير
                using (var context = _contextFactory.CreateDbContext(Array.Empty<string>()))
                {
                    var labOrderToPrint = await context.LabOrders
                       .Include(o => o.Items)
                           .ThenInclude(item => item.Test)
                               .ThenInclude(test => test.ReferenceValues) // لضمان تحميل ReferenceValues للاستخدام المحتمل في التقرير
                       .Where(o => o.Id == SelectedLabOrder.Id)
                       .FirstOrDefaultAsync();

                    // نحتاج أيضاً إلى كائن المريض الكامل للتقرير (إذا لم يكن محمل بالكامل بالفعل مع الطلب أو نحتاج بيانات إضافية)
                    var patientToPrint = await context.Patients
                                           .Where(p => p.Id == SelectedPatient.Id)
                                           // يمكنك هنا Include Physician إذا كنت تحتاجه في التقرير وتأكدت أنه غير محمل
                                           // .Include(p => p.ReferringPhysician)
                                           .FirstOrDefaultAsync();


                    if (labOrderToPrint != null && patientToPrint != null)
                    {
                        // استدعاء مولد التقرير باستخدام بيانات المريض وعناصر الطلب
                        var reportBytes = _reportGenerator.GeneratePatientReport(patientToPrint, labOrderToPrint.Items.ToList());

                        // تحديد مسار حفظ ملف PDF (على سبيل المثال، مجلد سطح المكتب)
                        var defaultSavePath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                        // إنشاء اسم ملف فريد بناءً على اسم المريض وتاريخ ووقت الطلب
                        var fileName = $"تقرير_{patientToPrint.Name?.Replace(" ", "_")}_{labOrderToPrint.RegistrationDateTime:yyyyMMdd_HHmmss}.pdf";
                        var filePath = Path.Combine(defaultSavePath, fileName);

                        // حفظ بايتات ملف PDF إلى المسار المحدد بشكل غير متزامن
                        await File.WriteAllBytesAsync(filePath, reportBytes);

                        // عرض رسالة نجاح مع مسار الملف المحفوظ
                        MessageBox.Show($"تم إنشاء التقرير بنجاح وحفظه في: {filePath}", "نجاح", MessageBoxButton.OK, MessageBoxImage.Information);

                        // ملاحظة: لفتح الملف تلقائياً أو طباعته مباشرة، ستحتاج لاستخدام System.Diagnostics.Process
                        // مثال:
                        // System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(filePath) { UseShellExecute = true });
                    }
                    else
                    {
                        // رسالة تحذير إذا لم يتم العثور على البيانات المطلوبة للتقرير
                        MessageBox.Show("لم يتم العثور على بيانات الطلب أو المريض المطلوبة لإنشاء التقرير.", "خطأ في البيانات", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
            }
            catch (Exception ex)
            {
                // عرض رسالة خطأ إذا حدثت مشكلة أثناء إنشاء التقرير
                MessageBox.Show($"حدث خطأ أثناء إنشاء التقرير: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false; // تعطيل مؤشر التحميل
            }
        }

        // شرط لتنفيذ أمر PrintReportCommand: يمكن التنفيذ فقط إذا تم اختيار طلب تحليل (وبالتالي مريض ضمنياً) وليس هناك عملية تحميل جارية
        private bool CanPrintReport() => SelectedLabOrder != null && !IsLoading;


        // مُنشئ الـ ViewModel
        public PrintViewModel()
        {
            _contextFactory = new LabDbContextFactory();
            _reportGenerator = new PatientReportGenerator();

            // تحميل قائمة المرضى عند تهيئة الـ ViewModel
            // استخدام _ = لبدء المهمة دون انتظارها (fire and forget) لتجنب حظر المُنشئ
            _ = LoadPatientsAsync();
        }

        // دالة لتحميل قائمة المرضى من قاعدة البيانات
        private async Task LoadPatientsAsync()
        {
            IsLoading = true; // تفعيل مؤشر التحميل
            PatientList.Clear(); // مسح القائمة الحالية
            SelectedPatient = null; // مسح التحديد الحالي للمريض (مهم لتحديث الواجهة)
            PatientLabOrders.Clear(); // مسح طلبات التحاليل والقوائم المرتبطة بها
            SelectedLabOrder = null; // مسح تحديد طلب التحليل

            try
            {
                // استخدام Factory لإنشاء Context جديد
                using (var context = _contextFactory.CreateDbContext(Array.Empty<string>()))
                {
                    // جلب جميع المرضى وترتيبهم أبجدياً حسب الاسم
                    var patientsFromDb = await context.Patients
                                       .OrderBy(p => p.Name) // ترتيب أبجدي حسب الاسم
                                       .ToListAsync(); // تنفيذ الاستعلام وجلب البيانات

                    // تحديث ObservableCollection على واجهة المستخدم UI Thread
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        foreach (var patient in patientsFromDb)
                        {
                            PatientList.Add(patient);
                        }
                        // لا نحدد أول مريض افتراضياً هنا لنجعل المستخدم يختار
                    });
                }
            }
            catch (Exception ex)
            {
                // عرض رسالة خطأ إذا حدثت مشكلة أثناء تحميل المرضى
                MessageBox.Show($"حدث خطأ أثناء تحميل قائمة المرضى: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false; // تعطيل مؤشر التحميل
            }
        }

        // معالج يتم استدعاؤه تلقائياً بواسطة CommunityToolkit.Mvvm عند تغيير SelectedPatient
        // يقوم هذا المعالج بتشغيل أمر تحميل طلبات التحاليل للمريض الجديد المختار
        partial void OnSelectedPatientChanged(Patient? value)
        {
            // التحقق من أن أمر تحميل الطلبات يمكن تنفيذه قبل استدعائه
            if (LoadPatientOrdersCommand.CanExecute(null))
            {
                // استخدام _ = لبدء مهمة تحميل الطلبات دون انتظارها
                _ = LoadPatientOrdersCommand.ExecuteAsync(null);
            }
            else if (value == null) // إذا تم إلغاء تحديد المريض
            {
                // مسح قائمة الطلبات وتحديد الطلب إذا لم يتم اختيار مريض
                PatientLabOrders.Clear();
                SelectedLabOrder = null;
            }
        }

        // يمكن إضافة معالج لـ OnSelectedLabOrderChanged هنا إذا أردت تنفيذ شيء عند اختيار طلب تحليل
        // (مثلاً، عرض تفاصيل عناصر الطلب في جزء آخر من الواجهة)
        // partial void OnSelectedLabOrderChanged(LabOrder? value)
        // {
        //     // منطق عند تغيير طلب التحليل المختار
        // }
    }
}