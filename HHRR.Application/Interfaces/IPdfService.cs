namespace HHRR.Application.Interfaces;

public interface IPdfService
{
    Task<byte[]> GeneratePdfAsync(object data);
}
