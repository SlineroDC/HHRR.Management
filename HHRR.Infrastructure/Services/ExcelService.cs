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

            // Empezamos en la fila 2 (asumiendo que la 1 son encabezados)
            for (int row = 2; row <= rowCount; row++)
            {
                // Mapeo basado en TU estructura de Excel:
                // Col 2: Name, Col 3: LastName -> Unimos para Name
                var firstName = worksheet.Cells[row, 2].Text;
                var lastName = worksheet.Cells[row, 3].Text;
                
                // Col 7: Email
                // Col 8: Position -> JobTitle
                // Col 9: Salary
                // Col 10: DateEntry -> HiringDate
                // Col 14: Department -> DepartmentName

                var employee = new EmployeeDto
                {
                    Name = $"{firstName} {lastName}".Trim(), // Unimos nombre y apellido
                    Email = worksheet.Cells[row, 7].Text,
                    JobTitle = worksheet.Cells[row, 8].Text,
                    
                    // Limpieza de salario (quitar signos $ o puntos si los hay)
                    Salary = decimal.TryParse(worksheet.Cells[row, 9].Text
                        .Replace("$", "").Replace(".", "").Replace(",", "."), out var salary) ? salary : 0,
                    
                    HiringDate = DateTime.TryParse(worksheet.Cells[row, 10].Text, out var date) 
                                ? DateTime.SpecifyKind(date, DateTimeKind.Utc) 
                                : DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc),
                    
                    DepartmentName = worksheet.Cells[row, 14].Text.Trim(), // Importante el Trim()
                    
                    // Col 11: Status (Activo/Inactivo/Vacaciones) -> Mapeo simple
                    Status = ParseStatus(worksheet.Cells[row, 11].Text)
                };

                // Validación simple para no agregar filas vacías
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
        // Mapeo manual para asegurar que coincida con tus Enums
        return statusText.ToLower().Trim() switch
        {
            "activo" => Status.Active,
            "inactivo" => Status.Inactive,
            // Si tienes un enum para Vacaciones úsalo, si no, ponlo como Active o Inactive según lógica
            "vacaciones" => Status.Active, 
            _ => Status.Active
        };
    }
}