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

            // Start from row 2 (assuming row 1 is header)
            for (int row = 2; row <= rowCount; row++)
            {
                var employee = new EmployeeDto
                {
                    Name = worksheet.Cells[row, 1].Text,
                    Email = worksheet.Cells[row, 2].Text,
                    JobTitle = worksheet.Cells[row, 3].Text,
                    Salary = decimal.TryParse(worksheet.Cells[row, 4].Text, out var salary) ? salary : 0,
                    HiringDate = DateTime.TryParse(worksheet.Cells[row, 5].Text, out var date) 
                                ? DateTime.SpecifyKind(date, DateTimeKind.Utc) 
                                : DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Utc),
                    DepartmentId = int.TryParse(worksheet.Cells[row, 6].Text, out var deptId) ? deptId : 0,
                    Status = Enum.TryParse<Status>(worksheet.Cells[row, 7].Text, true, out var status) ? status : Status.Active
                };

                employees.Add(employee);
            }
        }

        return employees;
    }
}
