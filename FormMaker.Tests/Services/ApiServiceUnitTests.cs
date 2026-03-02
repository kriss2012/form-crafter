using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FormMaker.Client.Services;
using Microsoft.JSInterop;
using Moq;
using Moq.Protected;
using Xunit;

namespace FormMaker.Tests.Services;

/// <summary>
/// Fake LocalStorageService for testing
/// </summary>
public class FakeLocalStorageService : LocalStorageService
{
    private readonly Dictionary<string, object> _storage = new();

    public FakeLocalStorageService() : base(null!)
    {
    }

    public override async Task<T?> GetItemAsync<T>(string key) where T : default
    {
        await Task.CompletedTask;
        if (_storage.TryGetValue(key, out var value))
        {
            if (value is T typedValue)
                return typedValue;
            if (value is string str && typeof(T) == typeof(string))
                return (T)(object)str;
        }
        return default;
    }

    public override async Task SetItemAsync<T>(string key, T value)
    {
        await Task.CompletedTask;
        _storage[key] = value!;
    }

    public override async Task RemoveItemAsync(string key)
    {
        await Task.CompletedTask;
        _storage.Remove(key);
    }
}

/// <summary>
/// Unit tests for ApiService with mocked HTTP responses
/// Tests API integration without hitting real endpoints
/// </summary>
public class ApiServiceUnitTests
{
    private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
    private readonly HttpClient _httpClient;
    private readonly FakeLocalStorageService _fakeLocalStorage;
    private readonly ApiService _apiService;

    public ApiServiceUnitTests()
    {
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("https://test.example.com/api/")
        };
        _fakeLocalStorage = new FakeLocalStorageService();
        _apiService = new ApiService(_httpClient, _fakeLocalStorage);
    }

    private void SetupMockHttpResponse(HttpStatusCode statusCode, object? content = null)
    {
        var responseMessage = new HttpResponseMessage
        {
            StatusCode = statusCode,
            Content = content != null ? JsonContent.Create(content) : null
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(responseMessage);
    }

    #region Template CRUD Tests

    [Fact]
    public async Task PostAsync_CreateTemplate_ReturnsTemplateResponse()
    {
        // Arrange
        var mockResponse = new TemplateResponse
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Name = "Test Form",
            Description = "Test Description",
            JsonData = "{\"name\":\"Test\"}",
            Category = "Test",
            Tags = "test",
            IsPublic = false,
            IsFeatured = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Version = 1,
            UsageCount = 0
        };

        SetupMockHttpResponse(HttpStatusCode.Created, mockResponse);

        var request = new CreateTemplateRequest
        {
            Name = "Test Form",
            Description = "Test Description",
            JsonData = "{\"name\":\"Test\"}",
            Category = "Test",
            Tags = "test",
            IsPublic = false
        };

        await _fakeLocalStorage.SetItemAsync("authToken", "fake-token");

        // Act
        var result = await _apiService.PostAsync<CreateTemplateRequest, TemplateResponse>("templates", request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(mockResponse.Id, result.Id);
        Assert.Equal("Test Form", result.Name);
        Assert.Equal("Test Description", result.Description);
    }

    [Fact]
    public async Task GetAsync_GetTemplate_ReturnsTemplateResponse()
    {
        // Arrange
        var templateId = Guid.NewGuid();
        var mockResponse = new TemplateResponse
        {
            Id = templateId,
            UserId = Guid.NewGuid(),
            Name = "Retrieved Form",
            Description = "Retrieved Description",
            JsonData = "{\"name\":\"Retrieved\"}",
            Category = "Test",
            Tags = "test",
            IsPublic = false,
            IsFeatured = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Version = 1,
            UsageCount = 5
        };

        SetupMockHttpResponse(HttpStatusCode.OK, mockResponse);

        await _fakeLocalStorage.SetItemAsync("authToken", "fake-token");

        // Act
        var result = await _apiService.GetAsync<TemplateResponse>($"templates/{templateId}");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(templateId, result.Id);
        Assert.Equal("Retrieved Form", result.Name);
        Assert.Equal(5, result.UsageCount);
    }

    [Fact]
    public async Task GetAsync_GetAllTemplates_ReturnsTemplateList()
    {
        // Arrange
        var mockResponse = new TemplateListResponse
        {
            Templates = new List<TemplateResponse>
            {
                new TemplateResponse
                {
                    Id = Guid.NewGuid(),
                    UserId = Guid.NewGuid(),
                    Name = "Form 1",
                    Description = "Description 1",
                    JsonData = "{}",
                    Category = "Test",
                    Tags = "",
                    IsPublic = false,
                    IsFeatured = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    Version = 1,
                    UsageCount = 0
                },
                new TemplateResponse
                {
                    Id = Guid.NewGuid(),
                    UserId = Guid.NewGuid(),
                    Name = "Form 2",
                    Description = "Description 2",
                    JsonData = "{}",
                    Category = "Test",
                    Tags = "",
                    IsPublic = false,
                    IsFeatured = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    Version = 1,
                    UsageCount = 0
                }
            },
            TotalCount = 2
        };

        SetupMockHttpResponse(HttpStatusCode.OK, mockResponse);

        await _fakeLocalStorage.SetItemAsync("authToken", "fake-token");

        // Act
        var result = await _apiService.GetAsync<TemplateListResponse>("templates");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.TotalCount);
        Assert.Equal(2, result.Templates.Count);
        Assert.Equal("Form 1", result.Templates[0].Name);
        Assert.Equal("Form 2", result.Templates[1].Name);
    }

    [Fact]
    public async Task PutAsync_UpdateTemplate_ReturnsUpdatedTemplate()
    {
        // Arrange
        var templateId = Guid.NewGuid();
        var mockResponse = new TemplateResponse
        {
            Id = templateId,
            UserId = Guid.NewGuid(),
            Name = "Updated Form",
            Description = "Updated Description",
            JsonData = "{\"name\":\"Updated\"}",
            Category = "Updated",
            Tags = "updated",
            IsPublic = true,
            IsFeatured = false,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            UpdatedAt = DateTime.UtcNow,
            Version = 2,
            UsageCount = 10
        };

        SetupMockHttpResponse(HttpStatusCode.OK, mockResponse);

        var request = new UpdateTemplateRequest
        {
            Name = "Updated Form",
            Description = "Updated Description",
            JsonData = "{\"name\":\"Updated\"}",
            Category = "Updated",
            Tags = "updated",
            IsPublic = true
        };

        await _fakeLocalStorage.SetItemAsync("authToken", "fake-token");

        // Act
        var result = await _apiService.PutAsync<UpdateTemplateRequest, TemplateResponse>($"templates/{templateId}", request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(templateId, result.Id);
        Assert.Equal("Updated Form", result.Name);
        Assert.Equal("Updated Description", result.Description);
        Assert.Equal(2, result.Version);
        Assert.True(result.IsPublic);
    }

    [Fact]
    public async Task DeleteAsync_DeleteTemplate_ReturnsTrue()
    {
        // Arrange
        var templateId = Guid.NewGuid();
        SetupMockHttpResponse(HttpStatusCode.NoContent);

        await _fakeLocalStorage.SetItemAsync("authToken", "fake-token");

        // Act
        var result = await _apiService.DeleteAsync($"templates/{templateId}");

        // Assert
        Assert.True(result);
    }

    #endregion

    #region Authentication Tests

    [Fact]
    public async Task PostUnauthenticatedAsync_Register_ReturnsAuthResponse()
    {
        // Arrange
        var mockResponse = new AuthResponse
        {
            Token = "fake-jwt-token-12345",
            User = new UserDto
            {
                Id = Guid.NewGuid(),
                Email = "test@example.com",
                DisplayName = "Test User",
                EmailVerified = false,
                CreatedAt = DateTime.UtcNow
            }
        };

        SetupMockHttpResponse(HttpStatusCode.OK, mockResponse);

        var request = new RegisterRequest
        {
            Email = "test@example.com",
            Password = "Test123!",
            DisplayName = "Test User"
        };

        // Act
        var result = await _apiService.PostUnauthenticatedAsync<RegisterRequest, AuthResponse>("auth/register", request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("fake-jwt-token-12345", result.Token);
        Assert.NotNull(result.User);
        Assert.Equal("test@example.com", result.User.Email);
        Assert.Equal("Test User", result.User.DisplayName);
    }

    [Fact]
    public async Task PostUnauthenticatedAsync_Login_ReturnsAuthResponse()
    {
        // Arrange
        var mockResponse = new AuthResponse
        {
            Token = "fake-jwt-token-67890",
            User = new UserDto
            {
                Id = Guid.NewGuid(),
                Email = "existing@example.com",
                DisplayName = "Existing User",
                EmailVerified = true,
                CreatedAt = DateTime.UtcNow.AddDays(-30)
            }
        };

        SetupMockHttpResponse(HttpStatusCode.OK, mockResponse);

        var request = new LoginRequest
        {
            Email = "existing@example.com",
            Password = "Test123!"
        };

        // Act
        var result = await _apiService.PostUnauthenticatedAsync<LoginRequest, AuthResponse>("auth/login", request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("fake-jwt-token-67890", result.Token);
        Assert.NotNull(result.User);
        Assert.Equal("existing@example.com", result.User.Email);
        Assert.True(result.User.EmailVerified);
    }

    #endregion

    #region Token Management Tests

    [Fact]
    public async Task GetTokenAsync_ReturnsStoredToken()
    {
        // Arrange
        await _fakeLocalStorage.SetItemAsync("authToken", "stored-token-123");

        // Act
        var result = await _apiService.GetTokenAsync();

        // Assert
        Assert.Equal("stored-token-123", result);
    }

    [Fact]
    public async Task SetTokenAsync_StoresToken()
    {
        // Arrange
        var token = "new-token-456";

        // Act
        await _apiService.SetTokenAsync(token);

        // Assert
        var storedToken = await _fakeLocalStorage.GetItemAsync<string>("authToken");
        Assert.Equal(token, storedToken);
    }

    [Fact]
    public async Task RemoveTokenAsync_RemovesToken()
    {
        // Arrange
        await _fakeLocalStorage.SetItemAsync("authToken", "some-token");

        // Act
        await _apiService.RemoveTokenAsync();

        // Assert
        var storedToken = await _fakeLocalStorage.GetItemAsync<string>("authToken");
        Assert.Null(storedToken);
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task GetAsync_WhenUnauthorized_ThrowsHttpRequestException()
    {
        // Arrange
        SetupMockHttpResponse(HttpStatusCode.Unauthorized);
        // Don't set a token - simulate unauthorized request

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(async () =>
        {
            await _apiService.GetAsync<TemplateResponse>("templates/123");
        });
    }

    [Fact]
    public async Task PostAsync_WhenBadRequest_ThrowsHttpRequestException()
    {
        // Arrange
        SetupMockHttpResponse(HttpStatusCode.BadRequest);

        var request = new CreateTemplateRequest
        {
            Name = "",
            Description = "",
            JsonData = "",
            Category = ""
        };

        await _fakeLocalStorage.SetItemAsync("authToken", "fake-token");

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(async () =>
        {
            await _apiService.PostAsync<CreateTemplateRequest, TemplateResponse>("templates", request);
        });
    }

    [Fact]
    public async Task DeleteAsync_WhenNotFound_ReturnsFalse()
    {
        // Arrange
        SetupMockHttpResponse(HttpStatusCode.NotFound);

        await _fakeLocalStorage.SetItemAsync("authToken", "fake-token");

        // Act
        var result = await _apiService.DeleteAsync("templates/nonexistent-id");

        // Assert
        Assert.False(result);
    }

    #endregion

    #region Form Sharing Tests

    [Fact]
    public async Task CreateFormAsync_ReturnsFormResponse()
    {
        // Arrange
        var templateId = Guid.NewGuid();
        var mockResponse = new FormResponse
        {
            Id = Guid.NewGuid(),
            TemplateId = templateId,
            ShareLink = "abc123xyz",
            Title = "Shared Form",
            Description = "A shared form",
            IsPublic = true,
            IsActive = true,
            MaxSubmissions = 100,
            SubmissionCount = 0,
            RequireAuth = false,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(30),
            ViewCount = 0,
            LastAccessedAt = null,
            NotificationEmail = "test@example.com",
            EnableNotifications = true
        };

        SetupMockHttpResponse(HttpStatusCode.Created, mockResponse);

        var request = new CreateFormRequest
        {
            TemplateId = templateId,
            Title = "Shared Form",
            Description = "A shared form",
            RequireAuth = false,
            MaxSubmissions = 100,
            ExpiresAt = DateTime.UtcNow.AddDays(30),
            NotificationEmail = "test@example.com",
            EnableNotifications = true
        };

        await _fakeLocalStorage.SetItemAsync("authToken", "fake-token");

        // Act
        var result = await _apiService.CreateFormAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("abc123xyz", result.ShareLink);
        Assert.Equal("Shared Form", result.Title);
        Assert.True(result.IsActive);
        Assert.Equal(100, result.MaxSubmissions);
    }

    [Fact]
    public async Task GetFormByShareLinkAsync_ReturnsPublicFormResponse()
    {
        // Arrange
        var mockResponse = new PublicFormResponse
        {
            Id = Guid.NewGuid(),
            Title = "Public Form",
            Description = "A public form to fill",
            TemplateJsonData = "{\"name\":\"Public Form\",\"elements\":[]}",
            RequireAuth = false
        };

        SetupMockHttpResponse(HttpStatusCode.OK, mockResponse);

        // Act
        var result = await _apiService.GetFormByShareLinkAsync("abc123xyz");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Public Form", result.Title);
        Assert.Contains("Public Form", result.TemplateJsonData);
        Assert.False(result.RequireAuth);
    }

    #endregion
}
