namespace HHRR.Application.Interfaces;

public interface IAIService
{
    Task<string> GenerateContentAsync(string prompt);
}
