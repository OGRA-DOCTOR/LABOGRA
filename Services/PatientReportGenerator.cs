// اسم الملف: PatientReportGenerator.cs
using LABOGRA.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LABOGRA.Services
{
    public class PatientReportGenerator
    {
        private class ReportItemPdfViewModel
        {
            public string TestName { get; set; } = string.Empty;
            public string Result { get; set; } = string.Empty;
            public string Unit { get; set; } = string.Empty;
            public string ReferenceRange { get; set; } = string.Empty;
        }

        public byte[] GeneratePatientReport(Patient patient, List<LabOrderItem> labOrderItems, DateTime orderRegistrationDateTime, bool addDoctorPrefix)
        {
            if (patient == null) throw new ArgumentNullException(nameof(patient));
            if (labOrderItems == null) throw new ArgumentNullException(nameof(labOrderItems));

            var reportPdfItems = new List<ReportItemPdfViewModel>();
            string patientGenderUpper = patient.Gender == "ذكر" ? "MALE" :
                                        patient.Gender == "أنثى" ? "FEMALE" :
                                        patient.Gender?.ToUpperInvariant() ?? "UNKNOWN";

            foreach (var item in labOrderItems.OrderBy(i => i.Test?.Name))
            {
                string displayRefRange = "N/A";
                string unit = "N/A";

                var test = item.Test;
                if (test?.ReferenceValues != null && test.ReferenceValues.Any())
                {
                    var referenceValues = test.ReferenceValues;

                    var genderSpecificRef = referenceValues.FirstOrDefault(rv => rv.GenderSpecific != null &&
                                                                                 rv.GenderSpecific.Equals(patientGenderUpper, StringComparison.OrdinalIgnoreCase));
                    var generalRef = referenceValues.FirstOrDefault(rv => rv.GenderSpecific != null &&
                                                                          rv.GenderSpecific.Equals("ANY", StringComparison.OrdinalIgnoreCase));
                    var fallbackRef = referenceValues.FirstOrDefault();

                    var selectedRef = genderSpecificRef ?? generalRef ?? fallbackRef;

                    if (selectedRef != null)
                    {
                        displayRefRange = selectedRef.ReferenceText ?? "N/A";
                        unit = selectedRef.Unit ?? "N/A";
                    }
                }

                var testName = test?.Name ?? "N/A";
                var result = item.Result ?? "N/A";
                var resultUnit = item.ResultUnit ?? unit;

                reportPdfItems.Add(new ReportItemPdfViewModel
                {
                    TestName = testName,
                    Result = result,
                    Unit = resultUnit,
                    ReferenceRange = displayRefRange
                });
            }

            string genderDisplay = patient.Gender == "ذكر" ? "Male" :
                                   patient.Gender == "أنثى" ? "Female" :
                                   patient.Gender ?? "N/A";

            string ageUnitEnglish = patient.AgeUnit switch
            {
                "سنة" => "Years",
                "شهر" => "Months",
                "يوم" => "Days",
                _ => patient.AgeUnit ?? string.Empty
            };

            string ageDisplay = $"{patient.AgeValue} {ageUnitEnglish}";
            string referringPhysicianDisplay = patient.ReferringPhysician?.Name ?? "N/A";

            if (addDoctorPrefix && !string.IsNullOrWhiteSpace(patient.ReferringPhysician?.Name) && patient.ReferringPhysician?.Name != "N/A")
            {
                referringPhysicianDisplay = $"أ.د/ {patient.ReferringPhysician.Name}";
            }

            return QuestPDF.Fluent.Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1.5f, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontFamily(Fonts.Arial).FontSize(10));

                    page.Header()
                        .AlignCenter()
                        .PaddingBottom(10)
                        .Text("Patient Lab Results Report")
                        .SemiBold()
                        .FontSize(16)
                        .FontColor(Colors.Blue.Medium);

                    page.Content().Column(column =>
                    {
                        column.Item().Table(patientDetailsTable =>
                        {
                            patientDetailsTable.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(100);
                                columns.RelativeColumn();
                                columns.ConstantColumn(20);
                                columns.ConstantColumn(100);
                                columns.RelativeColumn();
                            });

                            void AddPatientDetailRow(string labelAr, string valueEn, string labelAr2, string valueEn2)
                            {
                                patientDetailsTable.Cell().ColumnSpan(1).AlignRight().PaddingRight(5).Text(labelAr).Bold();
                                patientDetailsTable.Cell().ColumnSpan(1).AlignLeft().Text(valueEn).DirectionFromLeftToRight();
                                patientDetailsTable.Cell().ColumnSpan(1);
                                patientDetailsTable.Cell().ColumnSpan(1).AlignRight().PaddingRight(5).Text(labelAr2).Bold();
                                patientDetailsTable.Cell().ColumnSpan(1).AlignLeft().Text(valueEn2).DirectionFromLeftToRight();
                            }

                            AddPatientDetailRow("اسم المريض:", patient.Name ?? "N/A", "ID:", patient.GeneratedId ?? "N/A");
                            AddPatientDetailRow("النوع:", genderDisplay, "العمر:", ageDisplay);
                            AddPatientDetailRow("الطبيب المعالج:", referringPhysicianDisplay, "الرقم الطبي:", patient.MedicalRecordNumber ?? "N/A");
                        });

                        column.Item().PaddingVertical(10);

                        column.Item().Text("Test Results:")
                            .SemiBold()
                            .FontSize(12)
                            .DirectionFromLeftToRight();

                        column.Item().Table(resultsTable =>
                        {
                            resultsTable.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3.5f);
                                columns.RelativeColumn(1.5f);
                                columns.RelativeColumn(1.5f);
                                columns.RelativeColumn(3.5f);
                            });

                            resultsTable.Header(header =>
                            {
                                header.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5)
                                    .Text(text => text.Span("Test Name").SemiBold().DirectionFromLeftToRight());

                                header.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5)
                                    .Text(text => text.Span("Result").SemiBold().DirectionFromLeftToRight());

                                header.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5)
                                    .Text(text => text.Span("Unit").SemiBold().DirectionFromLeftToRight());

                                header.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5)
                                    .Text(text => text.Span("Reference Range").SemiBold().DirectionFromLeftToRight());
                            });

                            foreach (var item in reportPdfItems)
                            {
                                resultsTable.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5)
                                    .Text(item.TestName).DirectionFromLeftToRight();

                                resultsTable.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5)
                                    .Text(item.Result).DirectionFromLeftToRight();

                                resultsTable.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5)
                                    .Text(item.Unit).DirectionFromLeftToRight();

                                resultsTable.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5)
                                    .Text(item.ReferenceRange).DirectionFromLeftToRight();
                            }
                        });

                        column.Item().PaddingTop(30).AlignCenter().Text(text =>
                        {
                            text.DefaultTextStyle(x => x.FontSize(8).FontColor(Colors.Grey.Medium));
                            text.Span($"Order Date: {orderRegistrationDateTime:yyyy-MM-dd HH:mm} - Generated by OGRALAB");
                            text.EmptyLine();
                            text.Span("Page ");
                            text.CurrentPageNumber();
                            text.Span(" of ");
                            text.TotalPages();
                        });
                    });
                });
            }).GeneratePdf();
        }
    }
}