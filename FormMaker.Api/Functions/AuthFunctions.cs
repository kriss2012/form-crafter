using System.Net;
using FormMaker.Api.Models;
using FormMaker.Api.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace FormMaker.Api.Functions;

/// <summary>
/// Azure Functions for authentication (register, login)
/// </summary>
public class AuthFunctions
{
    private readonly ILogger<AuthFunctions> _logger;
    private readonly AuthService _authService;

    public AuthFunctions(ILogger<AuthFunctions> logger, AuthService authService)
    {
        _logger = logger;
        _authService = authService;
    }

    /// <summary>
    /// OPTIONS handler for CORS preflight
    /// </summary>
    [Function("AuthOptions")]
    public HttpResponseData HandleOptions(
        [HttpTrigger(AuthorizationLevel.Anonymous, "options", Route = "auth/{*path}")] HttpRequestData req)
    {
        _logger.LogInformation("Handling OPTIONS preflight request for auth");
        var response = req.CreateResponse(HttpStatusCode.OK);
        AddCorsHeaders(response, req);
        return response;
    }

    /// <summary>
    /// POST /api/auth/register - Register a new user
    /// </summary>
    [Function("Register")]
    public async Task<HttpResponseData> Register(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "auth/register")] HttpRequestData req)
    {
        _logger.LogInformation("Register endpoint called");
        _logger.LogInformation("Request headers: {Headers}", string.Join(", ", req.Headers.Select(h => $"{h.Key}={string.Join(",", h.Value)}")));
        _logger.LogInformation("Request Content-Type: {ContentType}", req.Headers.TryGetValues("Content-Type", out var ct) ? string.Join(",", ct) : "none");

        try
        {
            // Read the body as string first for debugging
            string bodyContent;
            using (var reader = new StreamReader(req.Body))
            {
                bodyContent = await reader.ReadToEndAsync();
            }
            _logger.LogInformation("Request body content: {Body}", bodyContent);

            // Parse request body from the string (case-insensitive to handle camelCase from client)
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var request = string.IsNullOrWhiteSpace(bodyContent)
                ? null
                : JsonSerializer.Deserialize<RegisterRequest>(bodyContent, options);

            if (request == null)
            {
                return await CreateErrorResponse(req, HttpStatusCode.BadRequest, "Invalid request body");
            }

            // Attempt registration
            var result = await _authService.RegisterAsync(
                request.Email,
                request.Password,
                request.DisplayName
            );

            if (!result.Success)
            {
                return await CreateErrorResponse(req, HttpStatusCode.BadRequest, result.ErrorMessage ?? "Registration failed");
            }

            // Return success response
            var response = req.CreateResponse(HttpStatusCode.Created);

            // Add CORS headers BEFORE writing content
            AddCorsHeaders(response, req);
            response.Headers.Add("Content-Type", "application/json; charset=utf-8");

            // Manually serialize and write
            var authResponse = new AuthResponse
            {
                Token = result.Token!,
                User = new UserDto
                {
                    Id = result.User!.Id,
                    Email = result.User.Email,
                    DisplayName = result.User.DisplayName,
                    EmailVerified = result.User.EmailVerified,
                    CreatedAt = result.User.CreatedAt
                }
            };

            var json = JsonSerializer.Serialize(authResponse);
            await response.WriteStringAsync(json);

            return response;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Invalid JSON in request body");
            return await CreateErrorResponse(req, HttpStatusCode.BadRequest, "Invalid JSON format");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration");
            return await CreateErrorResponse(req, HttpStatusCode.InternalServerError, "An error occurred during registration");
        }
    }

    /// <summary>
    /// POST /api/auth/login - Authenticate a user
    /// </summary>
    [Function("Login")]
    public async Task<HttpResponseData> Login(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "auth/login")] HttpRequestData req)
    {
        _logger.LogInformation("Login endpoint called");
        _logger.LogInformation("Request headers: {Headers}", string.Join(", ", req.Headers.Select(h => $"{h.Key}={string.Join(",", h.Value)}")));
        _logger.LogInformation("Request Content-Type: {ContentType}", req.Headers.TryGetValues("Content-Type", out var ct) ? string.Join(",", ct) : "none");

        try
        {
            // Read the body as string first for debugging
            string bodyContent;
            using (var reader = new StreamReader(req.Body))
            {
                bodyContent = await reader.ReadToEndAsync();
            }
            _logger.LogInformation("Request body content: {Body}", bodyContent);

            // Parse request body from the string (case-insensitive to handle camelCase from client)
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var request = string.IsNullOrWhiteSpace(bodyContent)
                ? null
                : JsonSerializer.Deserialize<LoginRequest>(bodyContent, options);

            if (request == null)
            {
                return await CreateErrorResponse(req, HttpStatusCode.BadRequest, "Invalid request body");
            }

            // Attempt login
            var result = await _authService.LoginAsync(request.Email, request.Password);

            if (!result.Success)
            {
                return await CreateErrorResponse(req, HttpStatusCode.Unauthorized, result.ErrorMessage ?? "Login failed");
            }

            // Return success response
            var response = req.CreateResponse(HttpStatusCode.OK);

            // Add CORS headers BEFORE writing content
            AddCorsHeaders(response, req);
            response.Headers.Add("Content-Type", "application/json; charset=utf-8");

            // Manually serialize and write
            var authResponse = new AuthResponse
            {
                Token = result.Token!,
                User = new UserDto
                {
                    Id = result.User!.Id,
                    Email = result.User.Email,
                    DisplayName = result.User.DisplayName,
                    EmailVerified = result.User.EmailVerified,
                    CreatedAt = result.User.CreatedAt
                }
            };

            var json = JsonSerializer.Serialize(authResponse);
            await response.WriteStringAsync(json);

            return response;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Invalid JSON in request body");
            return await CreateErrorResponse(req, HttpStatusCode.BadRequest, "Invalid JSON format");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login");
            return await CreateErrorResponse(req, HttpStatusCode.InternalServerError, "An error occurred during login");
        }
    }

    /// <summary>
    /// GET /api/auth/users - Get all users (for debugging/admin)
    /// </summary>
    [Function("GetAllUsers")]
    public async Task<HttpResponseData> GetAllUsers(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "auth/users")] HttpRequestData req)
    {
        _logger.LogInformation("GetAllUsers endpoint called");

        try
        {
            // Extract token from Authorization header
            var token = ExtractBearerToken(req);
            if (string.IsNullOrEmpty(token))
            {
                return await CreateErrorResponse(req, HttpStatusCode.Unauthorized, "No authorization token provided");
            }

            // Verify user is authenticated
            var currentUser = await _authService.GetUserFromTokenAsync(token);
            if (currentUser == null)
            {
                return await CreateErrorResponse(req, HttpStatusCode.Unauthorized, "Invalid or expired token");
            }

            // Get all users
            var users = await _authService.GetAllUsersAsync();

            // Return user list
            var response = req.CreateResponse(HttpStatusCode.OK);

            // Add CORS headers BEFORE writing content
            AddCorsHeaders(response, req);
            response.Headers.Add("Content-Type", "application/json; charset=utf-8");

            // Manually serialize and write
            var userDtos = users.Select(u => new UserDto
            {
                Id = u.Id,
                Email = u.Email,
                DisplayName = u.DisplayName,
                EmailVerified = u.EmailVerified,
                CreatedAt = u.CreatedAt
            }).ToList();

            var json = JsonSerializer.Serialize(userDtos);
            await response.WriteStringAsync(json);

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all users");
            return await CreateErrorResponse(req, HttpStatusCode.InternalServerError, "An error occurred");
        }
    }

    /// <summary>
    /// GET /api/auth/me - Get current user info from token
    /// </summary>
    [Function("GetMe")]
    public async Task<HttpResponseData> GetMe(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "auth/me")] HttpRequestData req)
    {
        _logger.LogInformation("GetMe endpoint called");

        try
        {
            // Extract token from Authorization header
            var token = ExtractBearerToken(req);
            if (string.IsNullOrEmpty(token))
            {
                return await CreateErrorResponse(req, HttpStatusCode.Unauthorized, "No authorization token provided");
            }

            // Get user from token
            var user = await _authService.GetUserFromTokenAsync(token);
            if (user == null)
            {
                return await CreateErrorResponse(req, HttpStatusCode.Unauthorized, "Invalid or expired token");
            }

            // Return user info
            var response = req.CreateResponse(HttpStatusCode.OK);

            // Add CORS headers BEFORE writing content
            AddCorsHeaders(response, req);
            response.Headers.Add("Content-Type", "application/json; charset=utf-8");

            // Manually serialize and write
            var userDto = new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                DisplayName = user.DisplayName,
                EmailVerified = user.EmailVerified,
                CreatedAt = user.CreatedAt
            };

            var json = JsonSerializer.Serialize(userDto);
            await response.WriteStringAsync(json);

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user info");
            return await CreateErrorResponse(req, HttpStatusCode.InternalServerError, "An error occurred");
        }
    }

    // Helper methods

    private static async Task<HttpResponseData> CreateErrorResponse(
        HttpRequestData req,
        HttpStatusCode statusCode,
        string errorMessage)
    {
        var response = req.CreateResponse(statusCode);

        // Add CORS headers BEFORE writing content
        AddCorsHeaders(response, req);
        response.Headers.Add("Content-Type", "application/json; charset=utf-8");

        // Manually serialize and write
        var errorResponse = new ErrorResponse { Error = errorMessage };
        var json = JsonSerializer.Serialize(errorResponse);
        await response.WriteStringAsync(json);

        return response;
    }

    private static void AddCorsHeaders(HttpResponseData response, HttpRequestData request)
    {
        // Get origin from request
        var origin = request.Headers.TryGetValues("Origin", out var origins)
            ? origins.FirstOrDefault()
            : null;

        // Set CORS headers - use TryAdd to avoid exceptions if headers exist
        if (!string.IsNullOrEmpty(origin))
        {
            if (!response.Headers.Contains("Access-Control-Allow-Origin"))
                response.Headers.Add("Access-Control-Allow-Origin", origin);
            if (!response.Headers.Contains("Access-Control-Allow-Credentials"))
                response.Headers.Add("Access-Control-Allow-Credentials", "true");
        }
        else
        {
            if (!response.Headers.Contains("Access-Control-Allow-Origin"))
                response.Headers.Add("Access-Control-Allow-Origin", "*");
        }

        if (!response.Headers.Contains("Access-Control-Allow-Methods"))
            response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");
        if (!response.Headers.Contains("Access-Control-Allow-Headers"))
            response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Authorization, Accept");
        if (!response.Headers.Contains("Access-Control-Max-Age"))
            response.Headers.Add("Access-Control-Max-Age", "86400");
    }

    private static string? ExtractBearerToken(HttpRequestData req)
    {
        if (!req.Headers.TryGetValues("Authorization", out var authHeaders))
        {
            return null;
        }

        var authHeader = authHeaders.FirstOrDefault();
        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return authHeader.Substring("Bearer ".Length).Trim();
    }
}
