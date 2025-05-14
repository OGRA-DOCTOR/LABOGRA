// الإصدار: 14 (لهذا الملف - إعادة تمرير المعاملات الأربعة لـ GeneratePatientReport)
// اسم الملف: ReportPreviewWindow.xaml.cs
using LABOGRA.Models;
using LABOGRA.Services;
using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;

namespace LABOGRA.Views.Print
{
    public partial class ReportPreviewWindow : Window
    {
        private readonly Patient _patient;
        private readonly LabOrderA _labOrder;
        private readonly PatientReportGenerator _reportGenerator;

        public class ReportItemViewModel
        {
            public Test? Test { get; set; }
            public string? Result { get; set; }
            public string? ResultUnit { get; set; }
            public string? ReferenceRange { get; set; }
        }

        public ReportPreviewWindow(Patient patient, LabOrderA labOrder)
        {
            InitializeComponent();
            _patient = patient ?? throw new ArgumentNullException(nameof(patient));
            _labOrder = labOrder ?? throw new ArgumentNullException(nameof(labOrder));
            _reportGenerator = new PatientReportGenerator();
            LoadPatientData();
            LoadResultsDataWithReferenceRanges();
        }

        private void LoadPatientData()
        {
            PatientNameTextBlock.Text = _patient.Name;
            PatientGeneratedIdTextBlock.Text = _patient.GeneratedId;
            MedicalRecordNumberTextBlock.Text = _patient.MedicalRecordNumber ?? "N/A";
            GenderTextBlock.Text = _patient.Gender == "ذكر" ? "Male" : (_patient.Gender == "أنثى" ? "Female" : _patient.Gender);
            string ageUnitEnglish = _patient.AgeUnit switch
            {
                "سنة" => "Years",
                "شهر" => "Months",
                "يوم" => "Days",
                _ => _patient.AgeUnit ?? string.Empty
            };
            AgeTextBlock.Text = $"{_patient.AgeValue} {ageUnitEnglish}";
            ReferringPhysicianTextBlock.Text = !string.IsNullOrEmpty(_patient.ReferringPhysician?.Name)
                                             ? $"أ.د/ {_patient.ReferringPhysician.Name}"
                                             : "N/A";
        }

        private void LoadResultsDataWithReferenceRanges()
        {
            if (_labOrder.Items != null)
            {
                var reportItems = new List<ReportItemViewModel>();
                string patientGender = _patient.Gender?.ToUpperInvariant() ?? "UNKNOWN";
                if (_patient.Gender == "ذكر") patientGender = "MALE";
                else if (_patient.Gender == "أنثى") patientGender = "FEMALE";
                foreach (var item in _labOrder.Items.OrderBy(i => i.Test?.Name))
                {
                    string displayRefRange = "N/A";
                    string unit = item.Test?.ReferenceValues?.FirstOrDefault()?.Unit ?? "N/A";
                    if (item.Test?.ReferenceValues != null && item.Test.ReferenceValues.Any())
                    {
                        var genderSpecificRef = item.Test.ReferenceValues.FirstOrDefault(rv => rv.GenderSpecific != null && rv.GenderSpecific.Equals(patientGender, StringComparison.OrdinalIgnoreCase));
                        var generalRef = item.Test.ReferenceValues.FirstOrDefault(rv => rv.GenderSpecific != null && rv.GenderSpecific.Equals("ANY", StringComparison.OrdinalIgnoreCase));
                        var fallbackRef = item.Test.ReferenceValues.FirstOrDefault();
                        var selectedRef = genderSpecificRef ?? generalRef ?? fallbackRef;
                        if (selectedRef != null) { displayRefRange = selectedRef.ReferenceText ?? "N/A"; unit = selectedRef.Unit ?? unit; }
                    }
                    reportItems.Add(new ReportItemViewModel { Test = item.Test, Result = item.Result, ResultUnit = item.ResultUnit ?? unit, ReferenceRange = displayRefRange });
                }
                ResultsDataGrid.ItemsSource = reportItems;
            }
        }

        private async void SavePdfButton_Click(object sender, RoutedEventArgs e)
        {
            if (_patient == null || _labOrder?.Items == null || !_labOrder.Items.Any()) { MessageBox.Show("No data to generate the report.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
            SaveFileDialog saveFileDialog = new SaveFileDialog { Filter = "PDF Document (*.pdf)|*.pdf", FileName = $"Report_{_patient.Name?.Replace(" ", "_")}_{_labOrder.RegistrationDateTime:yyyyMMdd_HHmmss}.pdf", Title = "Save Patient Report as PDF" };
            if (saveFileDialog.ShowDialog() == true)
            {
                string filePath = saveFileDialog.FileName;
                LoadingOverlay.Visibility = Visibility.Visible;
                try
                {
                    // *** بداية التعديل هنا: التأكد من تمرير المعاملات الأربعة ***
                    byte[] reportBytes = _reportGenerator.GeneratePatientReport(_patient, _labOrder.Items.ToList(), _labOrder.RegistrationDateTime, true);
                    // *** نهاية التعديل هنا ***
                    await File.WriteAllBytesAsync(filePath, reportBytes);
                    MessageBox.Show($"Report saved as PDF successfully at:\n{filePath}", "Saved", MessageBoxButton.OK, MessageBoxImage.Information);
                    try { System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(filePath) { UseShellExecute = true }); }
                    catch (Exception exOpen) { MessageBox.Show($"File saved, but could not be opened automatically: {exOpen.Message}", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning); }
                    this.Close();
                }
                catch (Exception ex) { MessageBox.Show($"Error while saving PDF file: {ex.Message}", "Save Error", MessageBoxButton.OK, MessageBoxImage.Error); }
                finally { LoadingOverlay.Visibility = Visibility.Collapsed; }
            }
        }

        private void PrintButton_Click(object sender, RoutedEventArgs e)
        {
            PrintDialog printDialog = new PrintDialog();
            if (printDialog.ShowDialog() == true)
            {
                try
                {
                    MessageBox.Show("For printing, a PDF file will be generated first. Please select a printer (you can choose Microsoft Print to PDF to save it as a file).", "Print Notice", MessageBoxButton.OK, MessageBoxImage.Information);
                    // *** بداية التعديل هنا: التأكد من تمرير المعاملات الأربعة ***
                    byte[] reportBytesToPrint = _reportGenerator.GeneratePatientReport(_patient, _labOrder.Items.ToList(), _labOrder.RegistrationDateTime, true);
                    // *** نهاية التعديل هنا ***
                    string tempPdfPath = Path.Combine(Path.GetTempPath(), $"print_{Guid.NewGuid()}.pdf");
                    File.WriteAllBytes(tempPdfPath, reportBytesToPrint);
                    System.Diagnostics.Process printProcess = new System.Diagnostics.Process();
                    printProcess.StartInfo.FileName = tempPdfPath;
                    printProcess.StartInfo.Verb = "Print";
                    printProcess.StartInfo.CreateNoWindow = true;
                    printProcess.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                    try { printProcess.Start(); MessageBox.Show("The report has been sent to the printer (or PDF save window).", "Sent to Printer", MessageBoxButton.OK, MessageBoxImage.Information); }
                    catch (Exception exPrint) { MessageBox.Show($"Could not send the file to print automatically. You can open the temporarily saved file and print it manually:\n{tempPdfPath}\nError: {exPrint.Message}", "Print Error", MessageBoxButton.OK, MessageBoxImage.Warning); }
                }
                catch (Exception ex) { MessageBox.Show($"Error during printing attempt: {ex.Message}", "Print Error", MessageBoxButton.OK, MessageBoxImage.Error); }
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e) { this.Close(); }
    }
}