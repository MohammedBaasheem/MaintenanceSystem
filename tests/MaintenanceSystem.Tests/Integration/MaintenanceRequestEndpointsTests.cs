using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using MaintenanceSystem.API.Contracts.MaintenanceRequests;
using MaintenanceSystem.Domain.Enums;
using Xunit;

namespace MaintenanceSystem.Tests.Integration;

public class MaintenanceRequestEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public MaintenanceRequestEndpointsTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _factory.EnsureDatabaseSeeded();
    }

    [Fact]
    public async Task Create_WithoutToken_ShouldReturnUnauthorized()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new CreateMaintenanceRequestRequest(
            "HVAC Noise",
            "Compressor vibration",
            MaintenancePriority.Medium,
            _factory.SeededMaintenanceTypeId);

        // Act
        var response = await client.PostAsJsonAsync("/api/maintenance-requests", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Create_AsEmployee_ShouldReturnCreated()
    {
        // Arrange
        var client = await _factory.CreateAuthenticatedClientAsync("employee_test", "Employee@123456");
        var request = new CreateMaintenanceRequestRequest(
            "Water Leak in Restroom",
            "Pipe leaking under the sink",
            MaintenancePriority.High,
            _factory.SeededMaintenanceTypeId);

        // Act
        var response = await client.PostAsJsonAsync("/api/maintenance-requests", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<CreateMaintenanceRequestResponse>(content, JsonOptions);

        result.Should().NotBeNull();
        result!.Id.Should().NotBeEmpty();
        result.Title.Should().Be("Water Leak in Restroom");
        result.Status.Should().Be(MaintenanceRequestStatus.New);
    }

    [Fact]
    public async Task AssignTechnician_AsEmployee_ShouldReturnForbidden()
    {
        // Arrange - Create request as employee
        var employeeClient = await _factory.CreateAuthenticatedClientAsync("employee_test", "Employee@123456");
        var createRequest = new CreateMaintenanceRequestRequest(
            "Light Fixture Issue",
            "Flickering lights in hallway",
            MaintenancePriority.Low,
            _factory.SeededMaintenanceTypeId);

        var createResponse = await employeeClient.PostAsJsonAsync("/api/maintenance-requests", createRequest);
        createResponse.EnsureSuccessStatusCode();

        var createContent = await createResponse.Content.ReadAsStringAsync();
        var created = JsonSerializer.Deserialize<CreateMaintenanceRequestResponse>(createContent, JsonOptions)!;

        // Act - Attempt to assign technician using employee client
        var assignRequest = new AssignTechnicianRequest(_factory.TechnicianUserId);
        var assignResponse = await employeeClient.PatchAsJsonAsync($"/api/maintenance-requests/{created.Id}/assign", assignRequest);

        // Assert
        assignResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task AssignTechnician_AsAdmin_ShouldReturnOkAndAssignTechnician()
    {
        // Arrange - Create request
        var employeeClient = await _factory.CreateAuthenticatedClientAsync("employee_test", "Employee@123456");
        var createRequest = new CreateMaintenanceRequestRequest(
            "Main Panel Circuit Failure",
            "Breaker trips continuously",
            MaintenancePriority.Critical,
            _factory.SeededMaintenanceTypeId);

        var createResponse = await employeeClient.PostAsJsonAsync("/api/maintenance-requests", createRequest);
        createResponse.EnsureSuccessStatusCode();

        var createContent = await createResponse.Content.ReadAsStringAsync();
        var created = JsonSerializer.Deserialize<CreateMaintenanceRequestResponse>(createContent, JsonOptions)!;

        // Act - Assign technician using admin client
        var adminClient = await _factory.CreateAuthenticatedClientAsync("admin_test", "Admin@123456");
        var assignRequest = new AssignTechnicianRequest(_factory.TechnicianUserId);
        var assignResponse = await adminClient.PatchAsJsonAsync($"/api/maintenance-requests/{created.Id}/assign", assignRequest);

        // Assert
        assignResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var assignContent = await assignResponse.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<MaintenanceRequestResponse>(assignContent, JsonOptions);

        result.Should().NotBeNull();
        result!.Id.Should().Be(created.Id);
        result.Status.Should().Be(MaintenanceRequestStatus.Assigned);
        result.AssignedTechnicianId.Should().Be(_factory.TechnicianUserId);
    }

    [Fact]
    public async Task UpdateStatus_AsTechnician_FromAssignedToInProgress_ShouldReturnOk()
    {
        // Arrange - Create and assign request
        var employeeClient = await _factory.CreateAuthenticatedClientAsync("employee_test", "Employee@123456");
        var createRequest = new CreateMaintenanceRequestRequest(
            "Generator Check",
            "Routine fuel filter check",
            MaintenancePriority.Medium,
            _factory.SeededMaintenanceTypeId);

        var createResponse = await employeeClient.PostAsJsonAsync("/api/maintenance-requests", createRequest);
        createResponse.EnsureSuccessStatusCode();

        var createContent = await createResponse.Content.ReadAsStringAsync();
        var created = JsonSerializer.Deserialize<CreateMaintenanceRequestResponse>(createContent, JsonOptions)!;

        var adminClient = await _factory.CreateAuthenticatedClientAsync("admin_test", "Admin@123456");
        var assignResponse = await adminClient.PatchAsJsonAsync(
            $"/api/maintenance-requests/{created.Id}/assign",
            new AssignTechnicianRequest(_factory.TechnicianUserId));
        assignResponse.EnsureSuccessStatusCode();

        // Act - Update status to InProgress as technician
        var techClient = await _factory.CreateAuthenticatedClientAsync("tech_test", "Tech@123456");
        var updateStatusRequest = new UpdateStatusRequest(MaintenanceRequestStatus.InProgress);
        var updateResponse = await techClient.PatchAsJsonAsync(
            $"/api/maintenance-requests/{created.Id}/status",
            updateStatusRequest);

        // Assert
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var updateContent = await updateResponse.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<MaintenanceRequestResponse>(updateContent, JsonOptions);

        result.Should().NotBeNull();
        result!.Id.Should().Be(created.Id);
        result.Status.Should().Be(MaintenanceRequestStatus.InProgress);
    }

    [Fact]
    public async Task UpdateStatus_WithInvalidTransition_ShouldReturnBadRequest()
    {
        // Arrange - Create request (starts in New status)
        var employeeClient = await _factory.CreateAuthenticatedClientAsync("employee_test", "Employee@123456");
        var createRequest = new CreateMaintenanceRequestRequest(
            "Elevator Sensor",
            "Sensor reading error",
            MaintenancePriority.High,
            _factory.SeededMaintenanceTypeId);

        var createResponse = await employeeClient.PostAsJsonAsync("/api/maintenance-requests", createRequest);
        createResponse.EnsureSuccessStatusCode();

        var createContent = await createResponse.Content.ReadAsStringAsync();
        var created = JsonSerializer.Deserialize<CreateMaintenanceRequestResponse>(createContent, JsonOptions)!;

        // Act - Attempt illegal transition from New directly to Closed (requires InProgress -> Resolved -> Closed)
        var adminClient = await _factory.CreateAuthenticatedClientAsync("admin_test", "Admin@123456");
        var invalidStatusRequest = new UpdateStatusRequest(MaintenanceRequestStatus.Closed);
        var updateResponse = await adminClient.PatchAsJsonAsync(
            $"/api/maintenance-requests/{created.Id}/status",
            invalidStatusRequest);

        // Assert
        updateResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}

