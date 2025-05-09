using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using LABOGRA.Models;
using QuestPDF.Helpers;

namespace LABOGRA.Services
{
    public class PatientReportGenerator
    {
        public byte[] GeneratePatientReport(Patient patient, List<LabOrderItem> labOrderItems)
        {
            if (patient == null || labOrderItems == null)
                throw new ArgumentNullException("Patient or LabOrderItems cannot be null.");

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(12));

                    // Header
                    page.Header().Text($"Patient Report: {patient.Name}").FontSize(18).Bold().AlignCenter();

                    // Content
                    page.Content().Column(column =>
                    {
                        // Patient Details
                        column.Item().Text($"Patient ID: {patient.Id}");
                        column.Item().Text($"Gender: {patient.Gender}");
                        column.Item().Text($"Age: {patient.AgeValue} {patient.AgeUnit}");
                        column.Item().Text($"Referring Physician: {patient.ReferringPhysician?.Name ?? "N/A"}");
                        column.Item().Text($"Registration Date: {patient.RegistrationDateTime:yyyy-MM-dd}");
                        column.Item().Text(" ");

                        // Lab Results
                        column.Item().Text("Lab Results:").FontSize(14).Bold();
                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3); // Test Name
                                columns.RelativeColumn(2); // Result
                                columns.RelativeColumn(1); // Unit
                                columns.RelativeColumn(3); // Reference Range
                            });

                            table.Header(header =>
                            {
                                header.Cell().Text("Test Name").Bold();
                                header.Cell().Text("Result").Bold();
                                header.Cell().Text("Unit").Bold();
                                header.Cell().Text("Reference Range").Bold();
                            });

                            foreach (var item in labOrderItems)
                            {
                                table.Cell().Text(item.Test.Name); // Test Name
                                table.Cell().Text(item.Result ?? "N/A"); // Result
                                table.Cell().Text(item.ResultUnit ?? "N/A"); // Unit
                                table.Cell().Text(item.ReferenceRange ?? "N/A"); // Reference Range
                            }
                        });
                    });

                    // Footer
                    page.Footer().AlignCenter().Text($"Generated on {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                });
            });

            using var memoryStream = new System.IO.MemoryStream();
            document.GeneratePdf(memoryStream);
            return memoryStream.ToArray();
        }
    }
}
