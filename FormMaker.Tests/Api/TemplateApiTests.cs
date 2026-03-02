using System.Net;
using System.Net.Http.Json;
using FormMaker.Shared.Models;
using Xunit;

namespace FormMaker.Tests.Api;

/// <summary>
/// Integration tests for Template API endpoints
/// Tests the full API flow with real HTTP calls
/// </summary>
public class TemplateApiTests
{
    private readonly HttpClient _httpClient;
    private const string BaseUrl = "https://form-crafter.onrender.com/api/";

    // Test user credentials - you can change these
    private const string TestEmail = "test@example.com";
    private const string TestPassword = "Test123!";
    private string? _authToken;

    public TemplateApiTests()
    {
        _httpClient = new HttpClient();
        _httpClient.BaseAddress = new Uri(BaseUrl);
    }

    /// <summary>
    /// Test data: Sample form template JSON
    /// </summary>
    private string GetSampleFormJson()
    {
        return @"{
            ""id"": ""00000000-0000-0000-0000-000000000000"",
            ""name"": ""Test Form"",
            ""description"": ""A test form"",
            ""elements"": [
                {
                    ""id"": ""elem-1"",
                    ""type"": ""TextInput"",
                    ""x"": 100,
                    ""y"": 100,
                    ""width"": 200,
                    ""height"": 40,
                    ""label"": ""Name"",
                    ""placeholder"": ""Enter your name"",
                    ""isRequired"": true
                }
            ],
            ""pageSize"": ""A4"",
            ""widthInPixels"": 794,
            ""heightInPixels"": 1123
        }";
    }

    /// <summary>
    /// Helper: Register or login to get auth token
    /// </summary>
    private async Task<string> GetAuthTokenAsync()
    {
        if (_authToken != null)
            return _authToken;

        // Try to register first
        try
        {
            var registerRequest = new
            {
                email = TestEmail,
                password = TestPassword,
                displayName = "Test User"
            };

            var registerResponse = await _httpClient.PostAsJsonAsync("auth/register", registerRequest);

            if (registerResponse.IsSuccessStatusCode)
            {
                var registerResult = await registerResponse.Content.ReadFromJsonAsync<AuthResponse>();
                _authToken = registerResult?.Token ?? throw new Exception("No token received from register");
                return _authToken;
            }
        }
        catch
        {
            // Registration failed, try login
        }

        // Try login
        var loginRequest = new
        {
            email = TestEmail,
            password = TestPassword
        };

        var loginResponse = await _httpClient.PostAsJsonAsync("auth/login", loginRequest);
        loginResponse.EnsureSuccessStatusCode();

        var loginResult = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();
        _authToken = loginResult?.Token ?? throw new Exception("No token received from login");
        return _authToken;
    }

    /// <summary>
    /// Helper: Add auth header to request
    /// </summary>
    private async Task<HttpRequestMessage> CreateAuthenticatedRequest(HttpMethod method, string uri)
    {
        var token = await GetAuthTokenAsync();
        var request = new HttpRequestMessage(method, uri);
        request.Headers.Add("Authorization", $"Bearer {token}");
        return request;
    }

    [Fact]
    public async Task Test01_RegisterAndLogin_ShouldReturnToken()
    {
        // Act
        var token = await GetAuthTokenAsync();

        // Assert
        Assert.NotNull(token);
        Assert.NotEmpty(token);
    }

    [Fact]
    public async Task Test02_CreateTemplate_ShouldReturnCreatedTemplate()
    {
        // Arrange
        await GetAuthTokenAsync();
        var createRequest = new CreateTemplateRequest
        {
            Name = "API Test Form",
            Description = "Created via unit test",
            JsonData = GetSampleFormJson(),
            Category = "Test",
            Tags = "test,api",
            IsPublic = false
        };

        var request = await CreateAuthenticatedRequest(HttpMethod.Post, "templates");
        request.Content = JsonContent.Create(createRequest);

        // Act
        var response = await _httpClient.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<TemplateResponse>();
        Assert.NotNull(result);
        Assert.Equal("API Test Form", result.Name);
        Assert.Equal("Created via unit test", result.Description);
        Assert.NotEqual(Guid.Empty, result.Id);
    }

    [Fact]
    public async Task Test03_GetAllTemplates_ShouldReturnList()
    {
        // Arrange
        var request = await CreateAuthenticatedRequest(HttpMethod.Get, "templates");

        // Act
        var response = await _httpClient.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<TemplateListResponse>();
        Assert.NotNull(result);
        Assert.NotNull(result.Templates);
        Assert.True(result.TotalCount >= 0);
    }

    [Fact]
    public async Task Test04_CreateAndGetTemplate_ShouldReturnSameData()
    {
        // Arrange - Create a template
        await GetAuthTokenAsync();
        var createRequest = new CreateTemplateRequest
        {
            Name = "Get Test Form",
            Description = "For GET test",
            JsonData = GetSampleFormJson(),
            Category = "Test"
        };

        var createReq = await CreateAuthenticatedRequest(HttpMethod.Post, "templates");
        createReq.Content = JsonContent.Create(createRequest);
        var createResponse = await _httpClient.SendAsync(createReq);
        var created = await createResponse.Content.ReadFromJsonAsync<TemplateResponse>();

        // Act - Get the template
        var getRequest = await CreateAuthenticatedRequest(HttpMethod.Get, $"templates/{created!.Id}");
        var getResponse = await _httpClient.SendAsync(getRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var result = await getResponse.Content.ReadFromJsonAsync<TemplateResponse>();
        Assert.NotNull(result);
        Assert.Equal(created.Id, result.Id);
        Assert.Equal("Get Test Form", result.Name);
    }

    [Fact]
    public async Task Test05_CreateAndUpdateTemplate_ShouldUpdateSuccessfully()
    {
        // Arrange - Create a template
        await GetAuthTokenAsync();
        var createRequest = new CreateTemplateRequest
        {
            Name = "Update Test Form",
            Description = "Original description",
            JsonData = GetSampleFormJson(),
            Category = "Test"
        };

        var createReq = await CreateAuthenticatedRequest(HttpMethod.Post, "templates");
        createReq.Content = JsonContent.Create(createRequest);
        var createResponse = await _httpClient.SendAsync(createReq);
        var created = await createResponse.Content.ReadFromJsonAsync<TemplateResponse>();

        // Act - Update the template
        var updateRequest = new UpdateTemplateRequest
        {
            Name = "Updated Test Form",
            Description = "Updated description",
            Category = "Updated"
        };

        var updateReq = await CreateAuthenticatedRequest(HttpMethod.Put, $"templates/{created!.Id}");
        updateReq.Content = JsonContent.Create(updateRequest);
        var updateResponse = await _httpClient.SendAsync(updateReq);

        // Assert
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

        var result = await updateResponse.Content.ReadFromJsonAsync<TemplateResponse>();
        Assert.NotNull(result);
        Assert.Equal("Updated Test Form", result.Name);
        Assert.Equal("Updated description", result.Description);
        Assert.Equal("Updated", result.Category);
    }

    [Fact]
    public async Task Test06_CreateAndDeleteTemplate_ShouldDeleteSuccessfully()
    {
        // Arrange - Create a template
        await GetAuthTokenAsync();
        var createRequest = new CreateTemplateRequest
        {
            Name = "Delete Test Form",
            Description = "To be deleted",
            JsonData = GetSampleFormJson(),
            Category = "Test"
        };

        var createReq = await CreateAuthenticatedRequest(HttpMethod.Post, "templates");
        createReq.Content = JsonContent.Create(createRequest);
        var createResponse = await _httpClient.SendAsync(createReq);
        var created = await createResponse.Content.ReadFromJsonAsync<TemplateResponse>();

        // Act - Delete the template
        var deleteReq = await CreateAuthenticatedRequest(HttpMethod.Delete, $"templates/{created!.Id}");
        var deleteResponse = await _httpClient.SendAsync(deleteReq);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        // Verify it's deleted (GET should fail)
        var getReq = await CreateAuthenticatedRequest(HttpMethod.Get, $"templates/{created.Id}");
        var getResponse = await _httpClient.SendAsync(getReq);
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task Test07_GetTemplates_WithoutAuth_ShouldReturn401()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, "templates");
        // No auth header

        // Act
        var response = await _httpClient.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Test08_CreateTemplate_WithInvalidData_ShouldReturn400()
    {
        // Arrange
        await GetAuthTokenAsync();
        var invalidRequest = new
        {
            // Missing required fields
            description = "Invalid request"
        };

        var request = await CreateAuthenticatedRequest(HttpMethod.Post, "templates");
        request.Content = JsonContent.Create(invalidRequest);

        // Act
        var response = await _httpClient.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}

/// <summary>
/// Simplified auth response model for tests
/// </summary>
public class AuthResponse
{
    public string Token { get; set; } = string.Empty;
    public UserDto? User { get; set; }
}

/// <summary>
/// Simplified user DTO for tests
/// </summary>
public class UserDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public bool EmailVerified { get; set; }
    public DateTime CreatedAt { get; set; }
}
