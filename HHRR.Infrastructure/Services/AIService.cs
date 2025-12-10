using System.Text;
using System.Text.Json;
using HHRR.Application.Interfaces;
using HHRR.Core.Entities;

namespace HHRR.Infrastructure.Services;

public class AIService : IAIService
{
    private readonly IEmployeeRepository _employeeRepository;
    private const string ApiUrl = "https://generativelanguage.googleapis.com/v1beta/models/gemini-pro:generateContent";

    public AIService(IEmployeeRepository employeeRepository)
    {
        _employeeRepository = employeeRepository;
    }

    // RENOMBRADO A: GenerateContentAsync (Para coincidir con la Interfaz y el Controlador)
    public async Task<string> GenerateContentAsync(string userQuestion)
    {
        // Leemos la Key del .env (cargado en Program.cs)
        var apiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY");
        
        if (string.IsNullOrEmpty(apiKey))
        {
            return "Error de Configuración: Falta la API Key de Gemini en el archivo .env";
        }

        // 1. Traer datos reales
        var employees = await _employeeRepository.GetAllAsync();
        
        // 2. Simplificar datos para el Prompt (ahorra tokens)
        var simplifiedData = employees.Select(e => new
        {
            Nombre = e.Name,
            Departamento = e.Department?.Name ?? "Sin Asignar",
            Estado = e.Status.ToString(), // "Active", "Inactive", etc.
            Salario = e.Salary,
            Cargo = e.JobTitle
        });

        var jsonData = JsonSerializer.Serialize(simplifiedData);
        
        // 3. Crear el Prompt en ESPAÑOL
        var prompt = $@"
        Actúa como un Asistente de Recursos Humanos experto. 
        Analiza estos datos JSON reales de la empresa: {jsonData}. 
        
        Pregunta del usuario: {userQuestion}. 
        
        Responde brevemente basándote SOLO en los datos proporcionados. Si no sabes, dilo.";

        using var client = new HttpClient();
        
        // 4. Estructura del Body para Gemini
        var requestBody = new
        {
            contents = new[]
            {
                new
                {
                    parts = new[]
                    {
                        new { text = prompt }
                    }
                }
            }
        };

        var jsonContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
        
        // 5. Llamada a la API
        var response = await client.PostAsync($"{ApiUrl}?key={apiKey}", jsonContent);
        
        if (!response.IsSuccessStatusCode)
        {
            return $"Error comunicándose con Gemini (Status: {response.StatusCode})";
        }

        var responseString = await response.Content.ReadAsStringAsync();
        
        // 6. Parsear respuesta
        try 
        {
            using var doc = JsonDocument.Parse(responseString);
            var root = doc.RootElement;

            if (root.TryGetProperty("candidates", out var candidates) && candidates.GetArrayLength() > 0)
            {
                var candidate = candidates[0];
                if (candidate.TryGetProperty("content", out var content) && 
                    content.TryGetProperty("parts", out var parts) && 
                    parts.GetArrayLength() > 0)
                {
                    var text = parts[0].GetProperty("text").GetString();
                    return text ?? "La IA generó una respuesta vacía.";
                }
            }
                
            return "La IA no devolvió candidatos válidos.";
        }
        catch (Exception ex)
        {
            return $"Error interpretando la respuesta de la IA: {ex.Message}";
        }
    }
}