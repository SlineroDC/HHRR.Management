using HHRR.Application.DTOs;
using HHRR.Application.Interfaces;
using HHRR.Core.Enums;
using OfficeOpenXml;

namespace HHRR.Infrastructure.Services;

public class ExcelService : IExcelService
{
    public ExcelService()
    {
        ExcelPackage.License.SetNonCommercialPersonal("Student");
    }

    public List<EmployeeDto> ParseExcel(Stream fileStream)
    {
        var employees = new List<EmployeeDto>();

        using (var package = new ExcelPackage(fileStream))
        {
            var worksheet = package.Workbook.Worksheets[0];
            var rowCount = worksheet.Dimension.Rows;

            // Start at row 2 (assuming row 1 contains headers)
            for (int row = 2; row <= rowCount; row++)
            {
                // Mapping based on Excel structure:
                // Col 2: Name, Col 3: LastName -> Combine into Name
                var firstName = worksheet.Cells[row, 2].Text;
                var lastName = worksheet.Cells[row, 3].Text;
                
                // Col 7: Email
                // Col 8: Position -> JobTitle
                // Col 9: Salary
                // Col 10: DateEntry -> HiringDate
                // Col 14: Department -> DepartmentName

                var employee = new EmployeeDto
                {
                    Name = $"{firstName} {lastName}".Trim(),
                    Email = worksheet.Cells[row, 7].Text,
                    JobTitle = worksheet.Cells[row, 8].Text,
                    
                    // Clean salary (remove $ or dots if present)
                    Salary = decimal.TryParse(worksheet.Cells[row, 9].Text
                        .Replace("$", "").Replace(".", "").Replace(",", "."), out var salary) ? salary : 0,
                    
                    HiringDate = DateTime.TryParse(worksheet.Cells[row, 10].Text, out var date) 
                                ? DateTime.SpecifyKind(date, DateTimeKind.Utc) 
                                : DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc),
                    
                    DepartmentName = worksheet.Cells[row, 14].Text.Trim(),
                    
                    // Col 11: Status (Active/Inactive/Vacation) -> Simple mapping
                    Status = ParseStatus(worksheet.Cells[row, 11].Text)
                };

                // Simple validation to avoid adding empty rows
                if (!string.IsNullOrWhiteSpace(employee.Email))
                {
                    employees.Add(employee);
                }
            }
        }

        return employees;
    }

    private Status ParseStatus(string statusText)
    {
        // Manual mapping to match the Status enum
        return statusText.ToLower().Trim() switch
        {
            "activo" => Status.Active,
            "inactivo" => Status.Inactive,
            "vacaciones" => Status.Active,
            _ => Status.Active
        };
    }
}