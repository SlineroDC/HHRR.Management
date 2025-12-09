using HHRR.Application.Interfaces;
using HHRR.Core.Entities;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
namespace HHRR.Infrastructure.Services;

public class PdfService : IPdfService
{
    public PdfService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public async Task<byte[]> GeneratePdfAsync(object data)
    {
        if (data is not Employee employee)
        {
            throw new ArgumentException("Data must be of type Employee", nameof(data));
        }

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(12));

                page.Header()
                    .Text("Curriculum Vitae - TalentoPlus")
                    .SemiBold().FontSize(20).FontColor(Colors.Blue.Medium);

                page.Content()
                    .PaddingVertical(1, Unit.Centimetre)
                    .Column(x =>
                    {
                        x.Spacing(20);

                        x.Item().Text($"Name: {employee.Name}").Bold().FontSize(16);
                        x.Item().Text($"Email: {employee.Email}");
                        x.Item().Text("Address: Data not available"); // Placeholder

                        x.Item().LineHorizontal(1).LineColor(Colors.Grey.Medium);

                        x.Item().Text("Job Information").Bold().FontSize(14);
                        x.Item().Text($"Job Title: {employee.JobTitle}");
                        x.Item().Text($"Department: {employee.Department?.Name ?? "N/A"}");
                        x.Item().Text($"Salary: {employee.Salary:C}");
                        x.Item().Text($"Hiring Date: {employee.HiringDate:d}");
                        x.Item().Text($"Status: {employee.Status}");

                        x.Item().LineHorizontal(1).LineColor(Colors.Grey.Medium);

                        x.Item().Text("Education").Bold().FontSize(14);
                        x.Item().Text("Data not available");

                        x.Item().LineHorizontal(1).LineColor(Colors.Grey.Medium);

                        x.Item().Text("Profile").Bold().FontSize(14);
                        x.Item().Text("Data not available");
                    });

                page.Footer()
                    .AlignCenter()
                    .Text(x =>
                    {
                        x.Span("Page ");
                        x.CurrentPageNumber();
                    });
            });
        });

        return document.GeneratePdf();
    }
}
