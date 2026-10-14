using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using MaintenanceSystem.API.Contracts.Auth;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace MaintenanceSystem.Tests.Integration;

public class AuthEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public AuthEndpointsTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _factory.EnsureDatabaseSeeded();
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturnOkAndJwtToken()
    {
        // Arrange
        var request = new LoginRequest("admin_test", "Admin@123456");

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<LoginResponse>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        result.Should().NotBeNull();
        result!.Token.Should().NotBeNullOrWhiteSpace();
        result.ExpiresAtUtc.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public async Task Login_WithIncorrectPassword_ShouldReturnUnauthorized()
    {
        // Arrange
        var request = new LoginRequest("admin_test", "IncorrectPassword123!");

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_WithNonExistentUser_ShouldReturnUnauthorized()
    {
        // Arrange
        var request = new LoginRequest("non_existent_user", "SomePassword123!");

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Theory]
    [InlineData("", "ValidPassword123!")]
    [InlineData("admin_test", "")]
    public async Task Login_WithEmptyFields_ShouldReturnBadRequestWithProblemDetails(string userName, string password)
    {
        // Arrange
        var request = new LoginRequest(userName, password);

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        var problem = JsonSerializer.Deserialize<ProblemDetails>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        problem.Should().NotBeNull();
        problem!.Status.Should().Be((int)HttpStatusCode.BadRequest);
    }
}

