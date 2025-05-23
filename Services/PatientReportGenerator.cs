// PatientReportGenerator.cs - تصحيح بناء الجملة السلس
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

        public byte[] GeneratePatientReport(Patient patient, List<LabOrderItem> labOrderItems, DateTime orderRegistrationDateTime, bool addDoctorPrefixToName)
        {
            if (patient == null) throw new ArgumentNullException(nameof(patient));
            if (labOrderItems == null) throw new ArgumentNullException(nameof(labOrderItems));

            DateTime printedOnDateTime = DateTime.Now;

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
                    var genderSpecificRef = referenceValues.FirstOrDefault(rv => rv.GenderSpecific != null && rv.GenderSpecific.Equals(patientGenderUpper, StringComparison.OrdinalIgnoreCase));
                    var generalRef = referenceValues.FirstOrDefault(rv => rv.GenderSpecific != null && rv.GenderSpecific.Equals("ANY", StringComparison.OrdinalIgnoreCase));
                    var fallbackRef = referenceValues.FirstOrDefault();
                    var selectedRef = genderSpecificRef ?? generalRef ?? fallbackRef;

                    if (selectedRef != null)
                    {
                        displayRefRange = selectedRef.ReferenceText ?? "N/A";
                        unit = selectedRef.Unit ?? "N/A";
                    }
                }
                reportPdfItems.Add(new ReportItemPdfViewModel
                {
                    TestName = test?.Name ?? "N/A",
                    Result = item.Result ?? "N/A",
                    Unit = item.ResultUnit ?? unit,
                    ReferenceRange = displayRefRange
                });
            }

            string patientTitle = !string.IsNullOrWhiteSpace(patient.Title) ? $"{patient.Title}/" : string.Empty;
            string patientNameForPdf = $"{patientTitle}{patient.Name ?? "N/A"}";

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
            string genderAndAgeDisplay = $"{genderDisplay}, {patient.AgeValue} {ageUnitEnglish}";

            string referringPhysicianDisplay = "N/A";
            if (patient.ReferringPhysician != null && !string.IsNullOrWhiteSpace(patient.ReferringPhysician.Name))
            {
                referringPhysicianDisplay = addDoctorPrefixToName ? $"أ.د/{patient.ReferringPhysician.Name}" : patient.ReferringPhysician.Name;
            }

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1.5f, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontFamily(Fonts.Arial).FontSize(11).LineHeight(1.2f));

                    page.Header().Column(column =>
                    {
                        column.Item().AlignCenter().Text(text =>
                        {
                            text.Span("OGRALAB Medical Laboratory").Bold().FontSize(16).FontColor(Colors.Grey.Darken3);
                        });
                        var line = column.Item().LineHorizontal(0.75f);
                        line.LineColor(Colors.Grey.Lighten1);
                        column.Item().PaddingTop(5).PaddingBottom(15);
                    });

                    page.Content().Column(column =>
                    {
                        column.Item().AlignCenter().PaddingBottom(20).Text(text =>
                        {
                            text.Span("LAB REPORT").Bold().FontSize(22).FontFamily("Arial Black");
                        });

                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(100);
                                columns.RelativeColumn(1.5f);
                                columns.ConstantColumn(30);
                                columns.ConstantColumn(110);
                                columns.RelativeColumn(1.5f);
                            });

                            table.Cell().Element(CellStyleLabel).Text("Name:");
                            table.Cell().Element(CellStyleValue).Text(patientNameForPdf);
                            table.Cell();
                            table.Cell().Element(CellStyleLabel).Text("Gender / Age:");
                            table.Cell().Element(CellStyleValue).Text(genderAndAgeDisplay);

                            table.Cell().Element(CellStyleLabel).Text("Lab ID:");
                            table.Cell().Element(CellStyleValue).Text(patient.GeneratedId ?? "N/A");
                            table.Cell();
                            table.Cell().Element(CellStyleLabel).Text("Referred by:");
                            table.Cell().Element(CellStyleValue).Text(referringPhysicianDisplay);

                            table.Cell().Element(CellStyleLabel).Text("Requested on:");
                            table.Cell().Element(CellStyleValue).Text(orderRegistrationDateTime.ToString("dd/MM/yyyy HH:mm"));
                            table.Cell();
                            table.Cell().Element(CellStyleLabel).Text("Printed on:");
                            table.Cell().Element(CellStyleValue).Text(printedOnDateTime.ToString("dd/MM/yyyy HH:mm"));
                        });

                        column.Item().PaddingTop(20);

                        column.Item().PaddingBottom(8).Text(text =>
                        {
                            text.Span("Test Results:").Bold().FontSize(16);
                        });

                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3.5f);
                                columns.RelativeColumn(1.5f);
                                columns.RelativeColumn(1f);
                                columns.RelativeColumn(2.5f);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Element(HeaderCellStyle).Text("Test");
                                header.Cell().Element(HeaderCellStyle).Text("Result");
                                header.Cell().Element(HeaderCellStyle).Text("Unit");
                                header.Cell().Element(HeaderCellStyle).Text("Normal ranges");
                            });

                            foreach (var item in reportPdfItems)
                            {
                                table.Cell().Element(BodyCellStyle).Text(item.TestName);
                                table.Cell().Element(BodyCellStyle).Text(text =>
                                {
                                    var span = text.Span(item.Result); // تخزين نتيجة Span
                                    span.FontSize(11); // تطبيق FontSize على span
                                    if (IsAbnormal(item.Result, item.ReferenceRange))
                                    {
                                        span.Bold().FontColor(Colors.Red.Medium);
                                    }
                                });
                                table.Cell().Element(BodyCellStyle).Text(item.Unit);
                                table.Cell().Element(BodyCellStyle).Text(item.ReferenceRange);
                            }
                        });
                    });

                    page.Footer()
                        .AlignCenter()
                        .Text(text =>
                        {
                            text.DefaultTextStyle(x => x.FontSize(9).FontColor(Colors.Grey.Darken1));
                            text.Span($"Generated by OGRALAB - {printedOnDateTime:dd MMMM yyyy, HH:mm}");
                            text.EmptyLine();
                            text.Span("Page ");
                            text.CurrentPageNumber();
                            text.Span(" of ");
                            text.TotalPages();
                        });
                });
            }).GeneratePdf();
        }

        static IContainer CellStyleLabel(IContainer container) =>
            container.AlignRight().PaddingRight(5).DefaultTextStyle(style => style.Bold().FontSize(11));

        static IContainer CellStyleValue(IContainer container) =>
            container.AlignLeft().DefaultTextStyle(style => style.FontSize(11));

        static IContainer HeaderCellStyle(IContainer container) =>
            container.BorderBottom(1).BorderColor(Colors.Grey.Lighten1).Background(Colors.Grey.Lighten3)
                     .PaddingVertical(6).PaddingHorizontal(5).DefaultTextStyle(style => style.Bold().FontSize(12));

        static IContainer BodyCellStyle(IContainer container) =>
            container.BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).PaddingVertical(4).PaddingHorizontal(5).DefaultTextStyle(style => style.FontSize(11));


        private bool IsAbnormal(string result, string referenceRange)
        {
            if (string.IsNullOrWhiteSpace(result) || result.Equals("N/A", StringComparison.OrdinalIgnoreCase) ||
                string.IsNullOrWhiteSpace(referenceRange) || referenceRange.Equals("N/A", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            if (result.ToUpperInvariant().Contains("POSITIVE") && referenceRange.ToUpperInvariant().Contains("NEGATIVE")) return true;
            if (result.ToUpperInvariant() == "HIGH" || result.ToUpperInvariant() == "LOW") return true;

            if (double.TryParse(result, out double resValue))
            {
                var parts = referenceRange.Split('-');
                if (parts.Length == 2 && double.TryParse(parts[0].Trim(), out double lower) && double.TryParse(parts[1].Trim(), out double upper))
                {
                    if (resValue < lower || resValue > upper) return true;
                }
                else if (referenceRange.StartsWith("<") && double.TryParse(referenceRange.Substring(1).Trim(), out double upperLimit))
                {
                    if (resValue >= upperLimit) return true;
                }
                else if (referenceRange.StartsWith(">") && double.TryParse(referenceRange.Substring(1).Trim(), out double lowerLimit))
                {
                    if (resValue <= lowerLimit) return true;
                }
            }
            return false;
        }
    }
}