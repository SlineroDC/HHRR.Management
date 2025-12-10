using System.Text;
using System.Text.Json;
using HHRR.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace HHRR.Infrastructure.Services;

public class AIService : IAIService
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;

    // ✅ USAMOS EL MODELO QUE APARECE EN TU LISTA: gemini-2.5-flash
    private const string ApiUrl = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent";

    public AIService(IEmployeeRepository employeeRepository, IConfiguration configuration)
    {
        _employeeRepository = employeeRepository;
        _configuration = configuration;
        _httpClient = new HttpClient();
    }

    public async Task<string> GenerateContentAsync(string userQuestion)
    {
        // 1. OBTENER API KEY
        var apiKey = _configuration["Gemini:ApiKey"];
        if (string.IsNullOrEmpty(apiKey)) return "Error: Falta la API Key.";

        // 2. LEER LA BASE DE DATOS (Aquí es donde "lee" antes de responder)
        var employees = await _employeeRepository.GetAllAsync();
        
        // Simplificamos los datos para que sean fáciles de entender para la IA
        var contextData = employees.Select(e => new
        {
            Nombre = e.Name,
            Cargo = e.JobTitle,
            Departamento = e.Department?.Name ?? "General", // Nombre del Dept, no ID
            Salario = e.Salary,
            FechaIngreso = e.HiringDate.ToString("yyyy-MM-dd"),
            Email = e.Email
        });

        // Convertimos la BD a texto JSON
        var jsonContext = JsonSerializer.Serialize(contextData);

        // 3. CREAR EL PROMPT (Instrucciones + Datos + Pregunta)
        var prompt = $@"
        Eres un experto analista de Recursos Humanos para la empresa 'TalentoPlus'.
        
        Tus instrucciones:
        1. Tu única fuente de verdad son los siguientes DATOS JSON.
        2. No inventes información. Si la respuesta no está en los datos, di 'No tengo esa información'.
        3. Si te preguntan por totales o promedios, calcúlalos con los datos provistos.
        
        --- DATOS DE LA BASE DE DATOS (EMPLEADOS) ---
        {jsonContext}
        ---------------------------------------------

        PREGUNTA DEL USUARIO: {userQuestion}
        
        Respuesta (sé conciso y profesional):";

        // 4. PREPARAR EL PAQUETE HTTP
        var requestBody = new
        {
            contents = new[]
            {
                new { parts = new[] { new { text = prompt } } }
            }
        };

        var jsonContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

        try 
        {
            // 5. ENVIAR A GEMINI
            var response = await _httpClient.PostAsync($"{ApiUrl}?key={apiKey}", jsonContent);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                return $"Error Gemini ({response.StatusCode}): {error}";
            }

            // 6. LEER RESPUESTA
            var responseString = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseString);
            
            if (doc.RootElement.TryGetProperty("candidates", out var candidates) && candidates.GetArrayLength() > 0)
            {
                return candidates[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString() ?? "Sin respuesta.";
            }
            
            return "La IA no devolvió texto.";
        }
        catch (Exception ex)
        {
            return $"Excepción: {ex.Message}";
        }
    }
}