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

    // Using the Gemini 2.5 Flash model
    private const string ApiUrl = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent";

    public AIService(IEmployeeRepository employeeRepository, IConfiguration configuration)
    {
        _employeeRepository = employeeRepository;
        _configuration = configuration;
        _httpClient = new HttpClient();
    }

    public async Task<string> GenerateContentAsync(string userQuestion)
    {
        // 1. Get API Key
        var apiKey = _configuration["Gemini:ApiKey"];
        if (string.IsNullOrEmpty(apiKey)) return "Error: Missing API Key.";

        // 2. Read from database (this is where it "reads" before responding)
        var employees = await _employeeRepository.GetAllAsync();
        
        // Simplify data to make it easy for AI to understand
        var contextData = employees.Select(e => new
        {
            Name = e.Name,
            JobTitle = e.JobTitle,
            Department = e.Department?.Name ?? "General",
            Salary = e.Salary,
            HiringDate = e.HiringDate.ToString("yyyy-MM-dd"),
            Email = e.Email
        });

        // Convert database to JSON text
        var jsonContext = JsonSerializer.Serialize(contextData);

        // 3. Create the prompt (Instructions + Data + Question)
        var prompt = $@"
        You are an expert Human Resources analyst for the company 'TalentoPlus'.
        
        Your instructions:
        1. Your only source of truth is the following JSON DATA.
        2. Do not make up information. If the answer is not in the data, say 'I do not have that information'.
        3. If asked for totals or averages, calculate them with the provided data.
        
        --- DATABASE DATA (EMPLOYEES) ---
        {jsonContext}
        ---------------------------------------------

        USER QUESTION: {userQuestion}
        
        Answer (be concise and professional):";

        // 4. Prepare HTTP request
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
            // 5. Send to Gemini
            var response = await _httpClient.PostAsync($"{ApiUrl}?key={apiKey}", jsonContent);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                return $"Gemini Error ({response.StatusCode}): {error}";
            }

            // 6. Read response
            var responseString = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseString);
            
            if (doc.RootElement.TryGetProperty("candidates", out var candidates) && candidates.GetArrayLength() > 0)
            {
                return candidates[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString() ?? "No response.";
            }
            
            return "AI did not return text.";
        }
        catch (Exception ex)
        {
            return $"Exception: {ex.Message}";
        }
    }
}