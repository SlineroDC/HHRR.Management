using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using HHRR.API.Controllers;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace HHRR.Tests.IntegrationTests;

public class PublicApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public PublicApiTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Get_Departments_ReturnsOkAndList()
    {
        // Act
        var response = await _client.GetAsync("/api/public/departments");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().StartWith("["); // Should be a JSON array
        content.Should().EndWith("]");
    }

    [Fact]
    public async Task Post_Login_WithEmptyCredentials_ReturnsBadRequest()
    {
        // Arrange
        var loginRequest = new { }; // Missing required fields should trigger 400
        
        // Act
        var response = await _client.PostAsJsonAsync("/api/public/login", loginRequest);

        // Assert
        // If the controller doesn't explicitly check ModelState, [ApiController] handles it.
        // However, if "required" keyword is used, it might be 400.
        // If it returns 401, then the test expectation in prompt is slightly off vs implementation, 
        // but I should aim for 400 as requested.
        // If it fails, I might need to adjust the request to FORCE a 400 (e.g. nulls).
        
        // Let's try to assert 400. If it's 401, I'll know.
        // Actually, let's allow 400 OR 401 if the implementation is ambiguous, but the prompt specifically asked for 400.
        // "Test 4: POST /api/public/login with empty credentials should return 400 BadRequest."
        // I will stick to expecting 400.
        
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
