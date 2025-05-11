using LABOGRA.Models;
using LABOGRA.Services; // لاستخدام PatientReportGenerator
using Microsoft.Win32; // For SaveFileDialog
using System;
using System.IO; // For File.WriteAllBytesAsync
using System.Linq; // For Linq methods
using System.Windows;
using System.Windows.Controls; // For PrintDialog

namespace LABOGRA.Views
{
    public partial class ReportPreviewWindow : Window
    {
        private readonly Patient _patient;
        private readonly LabOrderA _labOrder; // تم التعديل إلى LabOrderA
        private readonly PatientReportGenerator _reportGenerator;

        public ReportPreviewWindow(Patient patient, LabOrderA labOrder) // تم التعديل إلى LabOrderA
        {
            InitializeComponent();

            _patient = patient ?? throw new ArgumentNullException(nameof(patient));
            _labOrder = labOrder ?? throw new ArgumentNullException(nameof(labOrder));
            _reportGenerator = new PatientReportGenerator(); // ننشئ نسخة جديدة هنا

            LoadPatientData();
            LoadResultsData();
        }

        private void LoadPatientData()
        {
            PatientNameTextBlock.Text = _patient.Name;
            PatientIdTextBlock.Text = _patient.Id.ToString();
            GenderTextBlock.Text = _patient.Gender;
            AgeTextBlock.Text = $"{_patient.AgeValue} {_patient.AgeUnit}";
            ReferringPhysicianTextBlock.Text = _patient.ReferringPhysician?.Name ?? "N/A";
            OrderDateTextBlock.Text = _labOrder.RegistrationDateTime.ToString("yyyy-MM-dd HH:mm");
        }

        private void LoadResultsData()
        {
            // يتم ربط ItemsSource للـ DataGrid في الـ XAML مباشرة
            // ResultsDataGrid.ItemsSource = _labOrder.Items; 
            // لكن لضمان أن البيانات المعروضة هي فقط من الطلب الحالي
            // ولا تتأثر بأي تغييرات محتملة في _labOrder.Items إذا كان كائن مشترك
            // من الأفضل إنشاء نسخة جديدة من القائمة للعرض، أو التأكد أن _labOrder.Items
            // تم جلبه بشكل حصري لهذا التقرير (وهو ما يحدث في PrintViewModel)
            if (_labOrder.Items != null)
            {
                ResultsDataGrid.ItemsSource = _labOrder.Items.ToList(); // .ToList() لإنشاء نسخة إذا لزم الأمر
            }
        }

        private async void SavePdfButton_Click(object sender, RoutedEventArgs e)
        {
            if (_patient == null || _labOrder?.Items == null || !_labOrder.Items.Any())
            {
                MessageBox.Show("لا توجد بيانات لإنشاء التقرير.", "خطأ", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "PDF Document (*.pdf)|*.pdf",
                FileName = $"تقرير_{_patient.Name?.Replace(" ", "_")}_{_labOrder.RegistrationDateTime:yyyyMMdd_HHmmss}.pdf",
                Title = "حفظ تقرير المريض كـ PDF"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                string filePath = saveFileDialog.FileName;
                LoadingOverlay.Visibility = Visibility.Visible; // إظهار مؤشر التحميل

                try
                {
                    // استدعاء مولد التقرير باستخدام بيانات المريض وعناصر الطلب
                    byte[] reportBytes = _reportGenerator.GeneratePatientReport(_patient, _labOrder.Items.ToList());

                    await File.WriteAllBytesAsync(filePath, reportBytes);

                    MessageBox.Show($"تم حفظ التقرير كملف PDF بنجاح في:\n{filePath}", "تم الحفظ", MessageBoxButton.OK, MessageBoxImage.Information);

                    // فتح الملف بعد إنشائه (اختياري)
                    try
                    {
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(filePath) { UseShellExecute = true });
                    }
                    catch (Exception exOpen)
                    {
                        MessageBox.Show($"تم حفظ الملف، ولكن تعذر فتحه تلقائياً: {exOpen.Message}", "تحذير", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }

                    this.Close(); // أغلق نافذة المعاينة بعد الحفظ والفتح
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"حدث خطأ أثناء حفظ ملف PDF: {ex.Message}", "خطأ في الحفظ", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    LoadingOverlay.Visibility = Visibility.Collapsed; // إخفاء مؤشر التحميل
                }
            }
        }

        private void PrintButton_Click(object sender, RoutedEventArgs e)
        {
            PrintDialog printDialog = new PrintDialog();
            if (printDialog.ShowDialog() == true)
            {
                try
                {
                    // يمكنك تخصيص ما يتم طباعته هنا
                    // طباعة النافذة الحالية (قد لا تكون مثالية دائماً)
                    // printDialog.PrintVisual(this, $"تقرير المريض: {_patient.Name}");

                    // الأفضل هو إنشاء FlowDocument للطباعة أو استخدام مكتبة مثل QuestPDF لإنشاء وثيقة للطباعة
                    // حالياً، كحل بسيط، يمكننا إعادة استخدام PatientReportGenerator لتوليد PDF ثم طباعته
                    // ولكن هذا يتطلب حفظ ملف مؤقت أو طباعة الـ byte array مباشرة إذا كان PrintDialog يدعم ذلك.

                    // كحل أبسط مؤقتاً، يمكن طباعة Grid الرئيسي الذي يحتوي على المحتوى
                    // ابحث عن الـ Grid الرئيسي في XAML وأعطه اسمًا، مثلاً "PrintableAreaGrid"
                    // إذا كان Grid الرئيسي هو (Grid)this.Content
                    var mainGrid = this.Content as Grid;
                    if (mainGrid != null)
                    {
                        // قد تحتاج لتعديل حجم الـ Grid قبل الطباعة ليناسب الصفحة
                        // mainGrid.Measure(new Size(printDialog.PrintableAreaWidth, printDialog.PrintableAreaHeight));
                        // mainGrid.Arrange(new Rect(new Point(0, 0), mainGrid.DesiredSize));
                        // printDialog.PrintVisual(mainGrid, $"تقرير المريض: {_patient.Name}");

                        // **** تعديل بسيط لمنطق الطباعة: توليد PDF وطباعته ****
                        // هذا يتطلب من المستخدم اختيار طابعة PDF إذا لم يكن لديه طابعة فعلية
                        // وهو ليس الحل المثالي للطباعة المباشرة، لكنه حل عملي الآن

                        MessageBox.Show("للطباعة، سيتم إنشاء ملف PDF أولاً. الرجاء اختيار طابعة (يمكن اختيار Microsoft Print to PDF لحفظه كملف).", "تنبيه طباعة", MessageBoxButton.OK, MessageBoxImage.Information);

                        byte[] reportBytesToPrint = _reportGenerator.GeneratePatientReport(_patient, _labOrder.Items.ToList());
                        string tempPdfPath = Path.Combine(Path.GetTempPath(), $"print_{Guid.NewGuid()}.pdf");
                        File.WriteAllBytes(tempPdfPath, reportBytesToPrint);

                        // للطباعة، نحتاج لفتح الملف ثم طباعته من خلال التطبيق الافتراضي لـ PDF
                        // أو استخدام واجهات برمجة تطبيقات أكثر تعقيداً للطباعة الصامتة
                        System.Diagnostics.Process printProcess = new System.Diagnostics.Process();
                        printProcess.StartInfo.FileName = tempPdfPath;
                        printProcess.StartInfo.Verb = "Print"; // يحاول إرسال أمر الطباعة
                        printProcess.StartInfo.CreateNoWindow = true;
                        printProcess.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden; // إخفاء أي نافذة قد تظهر

                        try
                        {
                            printProcess.Start();
                            // لا تنتظر هنا طويلاً جداً، فقط اعط فرصة لبدء عملية الطباعة
                            // قد تحتاج لآلية أفضل لتتبع اكتمال الطباعة إذا لزم الأمر
                            // printProcess.WaitForExit(5000); // انتظر 5 ثوان كحد أقصى
                            MessageBox.Show("تم إرسال التقرير إلى الطابعة (أو نافذة حفظ PDF).", "تم الإرسال للطباعة", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        catch (Exception exPrint)
                        {
                            MessageBox.Show($"تعذر إرسال الملف للطباعة تلقائياً. يمكنك فتح الملف المحفوظ مؤقتاً وطباعته يدوياً:\n{tempPdfPath}\nالخطأ: {exPrint.Message}", "خطأ في الطباعة", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                        // لا تحذف الملف المؤقت فوراً، فقد يكون عارض PDF لا يزال يستخدمه
                        // File.Delete(tempPdfPath); // يمكن حذفه لاحقاً أو عند إغلاق البرنامج
                    }
                    else
                    {
                        MessageBox.Show("تعذر تحديد المنطقة القابلة للطباعة.", "خطأ", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"حدث خطأ أثناء محاولة الطباعة: {ex.Message}", "خطأ في الطباعة", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}