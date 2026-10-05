using System.Net;
using System.Net.Http.Json;
using System.Text;
using full.API.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace full.API.Tests;

public class EmployeesApiTests(ApiFactory factory) : IClassFixture<ApiFactory>, IAsyncLifetime
{
    private readonly HttpClient _client = factory.CreateClient();

    public Task InitializeAsync() => factory.ResetDatabaseAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    private static EmployeeRequest NewEmployee(string name = "Jane Doe", string email = "jane@example.com", string department = "Engineering") => new()
    {
        Name = name,
        Email = email,
        Phone = "+44 7700 900123",
        Salary = 55000.50m,
        Department = department
    };

    private async Task<EmployeeResponse> CreateAsync(EmployeeRequest request)
    {
        var response = await _client.PostAsJsonAsync("/api/employees", request);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<EmployeeResponse>())!;
    }

    [Fact]
    public async Task List_WhenEmpty_ReturnsEmptyArray()
    {
        var response = await _client.GetAsync("/api/employees");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Empty((await response.Content.ReadFromJsonAsync<List<EmployeeResponse>>())!);
        Assert.Equal("0", response.Headers.GetValues("X-Total-Count").Single());
    }

    [Fact]
    public async Task Create_ValidRequest_Returns201WithLocationAndNormalizedData()
    {
        var response = await _client.PostAsJsonAsync("/api/employees", NewEmployee(email: "  Jane@Example.COM "));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = (await response.Content.ReadFromJsonAsync<EmployeeResponse>())!;
        Assert.NotEqual(Guid.Empty, created.Id);
        Assert.Equal("jane@example.com", created.Email);
        Assert.Equal(55000.50m, created.Salary);
        Assert.EndsWith($"/api/employees/{created.Id}", response.Headers.Location!.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Create_AcceptsNumericPhone_ForBackwardCompatibility()
    {
        const string legacyJson = """
            {"name":"Old Client","email":"old@example.com","phone":7700900123,"salary":1000,"department":"Sales"}
            """;

        var response = await _client.PostAsync("/api/employees", new StringContent(legacyJson, Encoding.UTF8, "application/json"));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = (await response.Content.ReadFromJsonAsync<EmployeeResponse>())!;
        Assert.Equal("7700900123", created.Phone);
    }

    [Fact]
    public async Task Create_InvalidRequest_Returns400WithFieldErrors()
    {
        var invalid = NewEmployee() with { Name = "", Email = "not-an-email", Phone = "abc", Salary = -1 };

        var response = await _client.PostAsJsonAsync("/api/employees", invalid);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = (await response.Content.ReadFromJsonAsync<ValidationProblemDetails>())!;
        Assert.Contains("Name", problem.Errors.Keys);
        Assert.Contains("Email", problem.Errors.Keys);
        Assert.Contains("Phone", problem.Errors.Keys);
        Assert.Contains("Salary", problem.Errors.Keys);
    }

    [Fact]
    public async Task Create_DuplicateEmail_Returns409()
    {
        await CreateAsync(NewEmployee());

        var response = await _client.PostAsJsonAsync("/api/employees", NewEmployee(name: "Someone Else", email: "JANE@example.com"));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var problem = (await response.Content.ReadFromJsonAsync<ProblemDetails>())!;
        Assert.Equal(409, problem.Status);
    }

    [Fact]
    public async Task Get_UnknownId_Returns404ProblemDetails()
    {
        var response = await _client.GetAsync($"/api/employees/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task Get_ExistingId_ReturnsEmployee()
    {
        var created = await CreateAsync(NewEmployee());

        var fetched = await _client.GetFromJsonAsync<EmployeeResponse>($"/api/employees/{created.Id}");

        Assert.Equal(created, fetched);
    }

    [Fact]
    public async Task Update_ExistingEmployee_ChangesFields()
    {
        var created = await CreateAsync(NewEmployee());
        var update = NewEmployee(name: "Jane Smith", department: "Platform") with { Salary = 60000 };

        var response = await _client.PutAsJsonAsync($"/api/employees/{created.Id}", update);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var fetched = await _client.GetFromJsonAsync<EmployeeResponse>($"/api/employees/{created.Id}");
        Assert.Equal("Jane Smith", fetched!.Name);
        Assert.Equal("Platform", fetched.Department);
        Assert.Equal(60000m, fetched.Salary);
    }

    [Fact]
    public async Task Update_KeepingOwnEmail_IsNotAConflict()
    {
        var created = await CreateAsync(NewEmployee());

        var response = await _client.PutAsJsonAsync($"/api/employees/{created.Id}", NewEmployee(name: "Renamed"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Update_ToAnotherEmployeesEmail_Returns409()
    {
        await CreateAsync(NewEmployee());
        var other = await CreateAsync(NewEmployee(name: "John Roe", email: "john@example.com"));

        var response = await _client.PutAsJsonAsync($"/api/employees/{other.Id}", NewEmployee(name: "John Roe", email: "jane@example.com"));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Update_UnknownId_Returns404()
    {
        var response = await _client.PutAsJsonAsync($"/api/employees/{Guid.NewGuid()}", NewEmployee());

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_ExistingEmployee_Returns204ThenGone()
    {
        var created = await CreateAsync(NewEmployee());

        var delete = await _client.DeleteAsync($"/api/employees/{created.Id}");
        var get = await _client.GetAsync($"/api/employees/{created.Id}");

        Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, get.StatusCode);
    }

    [Fact]
    public async Task Delete_UnknownId_Returns404()
    {
        var response = await _client.DeleteAsync($"/api/employees/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task List_FiltersBySearchAndDepartment()
    {
        await CreateAsync(NewEmployee("Alice Smith", "alice@example.com", "Engineering"));
        await CreateAsync(NewEmployee("Bob Jones", "bob@example.com", "Sales"));
        await CreateAsync(NewEmployee("Carol Smith", "carol@example.com", "Sales"));

        var bySearch = await _client.GetFromJsonAsync<List<EmployeeResponse>>("/api/employees?search=smith");
        var byDepartment = await _client.GetFromJsonAsync<List<EmployeeResponse>>("/api/employees?department=Sales");

        Assert.Equal(["Alice Smith", "Carol Smith"], bySearch!.Select(e => e.Name));
        Assert.Equal(["Bob Jones", "Carol Smith"], byDepartment!.Select(e => e.Name));
    }

    [Fact]
    public async Task List_PaginatesAndReportsTotal()
    {
        foreach (var name in new[] { "Amy Adams", "Ben Brown", "Cat Cole" })
        {
            await CreateAsync(NewEmployee(name, $"{name.Split(' ')[0].ToLowerInvariant()}@example.com"));
        }

        var response = await _client.GetAsync("/api/employees?page=2&pageSize=2");
        var page = await response.Content.ReadFromJsonAsync<List<EmployeeResponse>>();

        Assert.Equal("3", response.Headers.GetValues("X-Total-Count").Single());
        Assert.Equal(["Cat Cole"], page!.Select(e => e.Name));
    }

    [Theory]
    [InlineData("page=0")]
    [InlineData("pageSize=0")]
    [InlineData("pageSize=101")]
    public async Task List_InvalidPaging_Returns400(string query)
    {
        var response = await _client.GetAsync($"/api/employees?{query}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Health_ReportsHealthy()
    {
        var response = await _client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Healthy", await response.Content.ReadAsStringAsync());
    }
}
