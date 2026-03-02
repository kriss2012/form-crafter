using System.Net;
using System.Text.Json;
using FormMaker.Api.Data.Entities;
using FormMaker.Api.Models;
using FormMaker.Api.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace FormMaker.Api.Functions;

/// <summary>
/// Azure Functions for template CRUD operations
/// </summary>
public class TemplateFunctions
{
    private readonly ILogger<TemplateFunctions> _logger;
    private readonly TemplateService _templateService;
    private readonly AuthService _authService;

    public TemplateFunctions(
        ILogger<TemplateFunctions> logger,
        TemplateService templateService,
        AuthService authService)
    {
        _logger = logger;
        _templateService = templateService;
        _authService = authService;
    }

    /// <summary>
    /// OPTIONS handler for CORS preflight
    /// </summary>
    [Function("TemplateOptions")]
    public HttpResponseData HandleOptions(
        [HttpTrigger(AuthorizationLevel.Anonymous, "options", Route = "templates/{*path}")] HttpRequestData req)
    {
        _logger.LogInformation("Handling OPTIONS preflight request for templates");
        var response = req.CreateResponse(HttpStatusCode.OK);
        AddCorsHeaders(response, req);
        return response;
    }

    /// <summary>
    /// POST /api/templates - Create a new template
    /// </summary>
    [Function("CreateTemplate")]
    public async Task<HttpResponseData> CreateTemplate(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "templates")] HttpRequestData req)
    {
        _logger.LogInformation("CreateTemplate endpoint called");

        try
        {
            // Authenticate user
            var user = await AuthenticateRequest(req);
            if (user == null)
            {
                return await CreateErrorResponse(req, HttpStatusCode.Unauthorized, "Authentication required");
            }

            // Parse request body (case-insensitive to handle camelCase from client)
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var request = await JsonSerializer.DeserializeAsync<CreateTemplateRequest>(req.Body, options);
            if (request == null)
            {
                return await CreateErrorResponse(req, HttpStatusCode.BadRequest, "Invalid request body");
            }

            // Validate required fields
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return await CreateErrorResponse(req, HttpStatusCode.BadRequest, "Template name is required");
            }

            if (string.IsNullOrWhiteSpace(request.JsonData))
            {
                return await CreateErrorResponse(req, HttpStatusCode.BadRequest, "Template data is required");
            }

            // Create template
            var template = await _templateService.CreateTemplateAsync(user.Id, request);

            // Return response
            var response = req.CreateResponse(HttpStatusCode.Created);

            // Add CORS headers BEFORE writing content
            AddCorsHeaders(response, req);
            response.Headers.Add("Content-Type", "application/json; charset=utf-8");

            // Manually serialize and write
            var templateResponse = MapToResponse(template);
            var json = JsonSerializer.Serialize(templateResponse);
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
            _logger.LogError(ex, "Error creating template");
            return await CreateErrorResponse(req, HttpStatusCode.InternalServerError, "An error occurred");
        }
    }

    /// <summary>
    /// GET /api/templates - Get all templates for the authenticated user
    /// </summary>
    [Function("GetTemplates")]
    public async Task<HttpResponseData> GetTemplates(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "templates")] HttpRequestData req)
    {
        _logger.LogInformation("GetTemplates endpoint called");

        try
        {
            // Authenticate user
            var user = await AuthenticateRequest(req);
            if (user == null)
            {
                return await CreateErrorResponse(req, HttpStatusCode.Unauthorized, "Authentication required");
            }

            // Get templates
            var templates = await _templateService.GetUserTemplatesAsync(user.Id);

            // Return response
            var response = req.CreateResponse(HttpStatusCode.OK);

            // Add CORS headers BEFORE writing content
            AddCorsHeaders(response, req);
            response.Headers.Add("Content-Type", "application/json; charset=utf-8");

            // Manually serialize and write
            var listResponse = new TemplateListResponse
            {
                Templates = templates.Select(MapToResponse).ToList(),
                TotalCount = templates.Count
            };
            var json = JsonSerializer.Serialize(listResponse);
            await response.WriteStringAsync(json);

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting templates");
            return await CreateErrorResponse(req, HttpStatusCode.InternalServerError, "An error occurred");
        }
    }

    /// <summary>
    /// GET /api/templates/{id} - Get a single template by ID
    /// </summary>
    [Function("GetTemplate")]
    public async Task<HttpResponseData> GetTemplate(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "templates/{id}")] HttpRequestData req,
        string id)
    {
        _logger.LogInformation("GetTemplate endpoint called for ID: {Id}", id);

        try
        {
            // Authenticate user (optional for public templates)
            var user = await AuthenticateRequest(req);

            // Parse template ID
            if (!Guid.TryParse(id, out var templateId))
            {
                return await CreateErrorResponse(req, HttpStatusCode.BadRequest, "Invalid template ID");
            }

            // Get template
            var template = await _templateService.GetTemplateByIdAsync(templateId, user?.Id);

            if (template == null)
            {
                return await CreateErrorResponse(req, HttpStatusCode.NotFound, "Template not found");
            }

            // Return response
            var response = req.CreateResponse(HttpStatusCode.OK);

            // Add CORS headers BEFORE writing content
            AddCorsHeaders(response, req);
            response.Headers.Add("Content-Type", "application/json; charset=utf-8");

            // Manually serialize and write
            var templateResponse = MapToResponse(template);
            var json = JsonSerializer.Serialize(templateResponse);
            await response.WriteStringAsync(json);

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting template");
            return await CreateErrorResponse(req, HttpStatusCode.InternalServerError, "An error occurred");
        }
    }

    /// <summary>
    /// PUT /api/templates/{id} - Update a template
    /// </summary>
    [Function("UpdateTemplate")]
    public async Task<HttpResponseData> UpdateTemplate(
        [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "templates/{id}")] HttpRequestData req,
        string id)
    {
        _logger.LogInformation("UpdateTemplate endpoint called for ID: {Id}", id);

        try
        {
            // Authenticate user
            var user = await AuthenticateRequest(req);
            if (user == null)
            {
                return await CreateErrorResponse(req, HttpStatusCode.Unauthorized, "Authentication required");
            }

            // Parse template ID
            if (!Guid.TryParse(id, out var templateId))
            {
                return await CreateErrorResponse(req, HttpStatusCode.BadRequest, "Invalid template ID");
            }

            // Parse request body (case-insensitive to handle camelCase from client)
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var request = await JsonSerializer.DeserializeAsync<UpdateTemplateRequest>(req.Body, options);
            if (request == null)
            {
                return await CreateErrorResponse(req, HttpStatusCode.BadRequest, "Invalid request body");
            }

            // Update template
            var template = await _templateService.UpdateTemplateAsync(templateId, user.Id, request);

            if (template == null)
            {
                return await CreateErrorResponse(req, HttpStatusCode.NotFound, "Template not found or not authorized");
            }

            // Return response
            var response = req.CreateResponse(HttpStatusCode.OK);

            // Add CORS headers BEFORE writing content
            AddCorsHeaders(response, req);
            response.Headers.Add("Content-Type", "application/json; charset=utf-8");

            // Manually serialize and write
            var templateResponse = MapToResponse(template);
            var json = JsonSerializer.Serialize(templateResponse);
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
            _logger.LogError(ex, "Error updating template");
            return await CreateErrorResponse(req, HttpStatusCode.InternalServerError, "An error occurred");
        }
    }

    /// <summary>
    /// DELETE /api/templates/{id} - Delete a template (soft delete)
    /// </summary>
    [Function("DeleteTemplate")]
    public async Task<HttpResponseData> DeleteTemplate(
        [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "templates/{id}")] HttpRequestData req,
        string id)
    {
        _logger.LogInformation("DeleteTemplate endpoint called for ID: {Id}", id);

        try
        {
            // Authenticate user
            var user = await AuthenticateRequest(req);
            if (user == null)
            {
                return await CreateErrorResponse(req, HttpStatusCode.Unauthorized, "Authentication required");
            }

            // Parse template ID
            if (!Guid.TryParse(id, out var templateId))
            {
                return await CreateErrorResponse(req, HttpStatusCode.BadRequest, "Invalid template ID");
            }

            // Delete template
            var success = await _templateService.DeleteTemplateAsync(templateId, user.Id);

            if (!success)
            {
                return await CreateErrorResponse(req, HttpStatusCode.NotFound, "Template not found or not authorized");
            }

            // Return success response
            var response = req.CreateResponse(HttpStatusCode.NoContent);
            AddCorsHeaders(response, req);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting template");
            return await CreateErrorResponse(req, HttpStatusCode.InternalServerError, "An error occurred");
        }
    }

    // Helper methods

    private async Task<User?> AuthenticateRequest(HttpRequestData req)
    {
        var token = ExtractBearerToken(req);
        if (string.IsNullOrEmpty(token))
        {
            return null;
        }

        return await _authService.GetUserFromTokenAsync(token);
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

    private static TemplateResponse MapToResponse(Template template)
    {
        return new TemplateResponse
        {
            Id = template.Id,
            UserId = template.UserId,
            Name = template.Name,
            Description = template.Description,
            JsonData = template.JsonData,
            Category = template.Category,
            Tags = template.Tags,
            IsPublic = template.IsPublic,
            IsFeatured = template.IsFeatured,
            CreatedAt = template.CreatedAt,
            UpdatedAt = template.UpdatedAt,
            Version = template.Version,
            UsageCount = template.UsageCount
        };
    }
}
