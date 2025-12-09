using HHRR.Application.DTOs;

namespace HHRR.Application.Interfaces;

public interface IExcelService
{
    List<EmployeeDto> ParseExcel(Stream fileStream);
}
