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
/// Azure Functions for shareable form CRUD operations
/// </summary>
public class FormFunctions
{
    private readonly ILogger<FormFunctions> _logger;
    private readonly FormService _formService;
    private readonly AuthService _authService;

    public FormFunctions(
        ILogger<FormFunctions> logger,
        FormService formService,
        AuthService authService)
    {
        _logger = logger;
        _formService = formService;
        _authService = authService;
    }

    /// <summary>
    /// POST /api/forms - Create a new shareable form from a template
    /// </summary>
    [Function("CreateForm")]
    public async Task<HttpResponseData> CreateForm(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "forms")] HttpRequestData req)
    {
        _logger.LogInformation("CreateForm endpoint called");

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
            var request = await JsonSerializer.DeserializeAsync<CreateFormRequest>(req.Body, options);
            if (request == null)
            {
                return await CreateErrorResponse(req, HttpStatusCode.BadRequest, "Invalid request body");
            }

            // Validate required fields
            if (string.IsNullOrWhiteSpace(request.Title))
            {
                return await CreateErrorResponse(req, HttpStatusCode.BadRequest, "Form title is required");
            }

            // Create form
            var form = await _formService.CreateFormAsync(
                request.TemplateId,
                request.Title,
                request.Description,
                request.RequireAuth,
                request.MaxSubmissions,
                request.ExpiresAt,
                request.NotificationEmail,
                request.EnableNotifications);

            // Return response
            var response = req.CreateResponse(HttpStatusCode.Created);
            await response.WriteAsJsonAsync(MapToResponse(form));
            return response;
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid template ID");
            return await CreateErrorResponse(req, HttpStatusCode.BadRequest, ex.Message);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Invalid JSON in request body");
            return await CreateErrorResponse(req, HttpStatusCode.BadRequest, "Invalid JSON format");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating form");
            return await CreateErrorResponse(req, HttpStatusCode.InternalServerError, "An error occurred");
        }
    }

    /// <summary>
    /// GET /api/forms/public/{shareLink} - Get a form by its share link (public access)
    /// </summary>
    [Function("GetFormByShareLink")]
    public async Task<HttpResponseData> GetFormByShareLink(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "forms/public/{shareLink}")] HttpRequestData req,
        string shareLink)
    {
        _logger.LogInformation("GetFormByShareLink endpoint called for link: {ShareLink}", shareLink);

        try
        {
            var form = await _formService.GetFormByShareLinkAsync(shareLink);

            if (form == null || form.Template == null)
            {
                return await CreateErrorResponse(req, HttpStatusCode.NotFound, "Form not found or expired");
            }

            // Return public response (without sensitive info)
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(new PublicFormResponse
            {
                Id = form.Id,
                Title = form.Title,
                Description = form.Description,
                TemplateJsonData = form.Template.JsonData,
                RequireAuth = form.RequireAuth
            });
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting form by share link");
            return await CreateErrorResponse(req, HttpStatusCode.InternalServerError, "An error occurred");
        }
    }

    /// <summary>
    /// GET /api/forms/template/{templateId} - Get all forms for a specific template
    /// </summary>
    [Function("GetFormsByTemplate")]
    public async Task<HttpResponseData> GetFormsByTemplate(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "forms/template/{templateId}")] HttpRequestData req,
        string templateId)
    {
        _logger.LogInformation("GetFormsByTemplate endpoint called for template: {TemplateId}", templateId);

        try
        {
            // Authenticate user
            var user = await AuthenticateRequest(req);
            if (user == null)
            {
                return await CreateErrorResponse(req, HttpStatusCode.Unauthorized, "Authentication required");
            }

            // Parse template ID
            if (!Guid.TryParse(templateId, out var templateGuid))
            {
                return await CreateErrorResponse(req, HttpStatusCode.BadRequest, "Invalid template ID");
            }

            // Get forms
            var forms = await _formService.GetFormsByTemplateAsync(templateGuid);

            // Return response
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(new FormListResponse
            {
                Forms = forms.Select(MapToResponse).ToList(),
                TotalCount = forms.Count
            });
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting forms by template");
            return await CreateErrorResponse(req, HttpStatusCode.InternalServerError, "An error occurred");
        }
    }

    /// <summary>
    /// GET /api/forms/{id} - Get a specific form by ID
    /// </summary>
    [Function("GetFormById")]
    public async Task<HttpResponseData> GetFormById(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "forms/{id}")] HttpRequestData req,
        string id)
    {
        _logger.LogInformation("GetFormById endpoint called for ID: {Id}", id);

        try
        {
            // Authenticate user
            var user = await AuthenticateRequest(req);
            if (user == null)
            {
                return await CreateErrorResponse(req, HttpStatusCode.Unauthorized, "Authentication required");
            }

            // Parse form ID
            if (!Guid.TryParse(id, out var formGuid))
            {
                return await CreateErrorResponse(req, HttpStatusCode.BadRequest, "Invalid form ID");
            }

            // Get form
            var form = await _formService.GetFormByIdAsync(formGuid);

            if (form == null)
            {
                return await CreateErrorResponse(req, HttpStatusCode.NotFound, "Form not found");
            }

            // Return response
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(MapToResponse(form));
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting form by ID");
            return await CreateErrorResponse(req, HttpStatusCode.InternalServerError, "An error occurred");
        }
    }

    /// <summary>
    /// PUT /api/forms/{id} - Update form settings
    /// </summary>
    [Function("UpdateForm")]
    public async Task<HttpResponseData> UpdateForm(
        [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "forms/{id}")] HttpRequestData req,
        string id)
    {
        _logger.LogInformation("UpdateForm endpoint called for ID: {Id}", id);

        try
        {
            // Authenticate user
            var user = await AuthenticateRequest(req);
            if (user == null)
            {
                return await CreateErrorResponse(req, HttpStatusCode.Unauthorized, "Authentication required");
            }

            // Parse form ID
            if (!Guid.TryParse(id, out var formGuid))
            {
                return await CreateErrorResponse(req, HttpStatusCode.BadRequest, "Invalid form ID");
            }

            // Parse request body (case-insensitive to handle camelCase from client)
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var request = await JsonSerializer.DeserializeAsync<UpdateFormRequest>(req.Body, options);
            if (request == null)
            {
                return await CreateErrorResponse(req, HttpStatusCode.BadRequest, "Invalid request body");
            }

            // Update form
            var form = await _formService.UpdateFormAsync(
                formGuid,
                request.Title,
                request.Description,
                request.IsActive,
                request.IsPublic,
                request.MaxSubmissions,
                request.ExpiresAt);

            // Return response
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(MapToResponse(form));
            return response;
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Form not found");
            return await CreateErrorResponse(req, HttpStatusCode.NotFound, ex.Message);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Invalid JSON in request body");
            return await CreateErrorResponse(req, HttpStatusCode.BadRequest, "Invalid JSON format");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating form");
            return await CreateErrorResponse(req, HttpStatusCode.InternalServerError, "An error occurred");
        }
    }

    /// <summary>
    /// DELETE /api/forms/{id} - Delete a form (soft delete)
    /// </summary>
    [Function("DeleteForm")]
    public async Task<HttpResponseData> DeleteForm(
        [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "forms/{id}")] HttpRequestData req,
        string id)
    {
        _logger.LogInformation("DeleteForm endpoint called for ID: {Id}", id);

        try
        {
            // Authenticate user
            var user = await AuthenticateRequest(req);
            if (user == null)
            {
                return await CreateErrorResponse(req, HttpStatusCode.Unauthorized, "Authentication required");
            }

            // Parse form ID
            if (!Guid.TryParse(id, out var formGuid))
            {
                return await CreateErrorResponse(req, HttpStatusCode.BadRequest, "Invalid form ID");
            }

            // Delete form
            await _formService.DeleteFormAsync(formGuid);

            // Return success response
            var response = req.CreateResponse(HttpStatusCode.NoContent);
            return response;
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Form not found");
            return await CreateErrorResponse(req, HttpStatusCode.NotFound, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting form");
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
        await response.WriteAsJsonAsync(new ErrorResponse
        {
            Error = errorMessage
        });
        return response;
    }

    private static FormResponse MapToResponse(Form form)
    {
        return new FormResponse
        {
            Id = form.Id,
            TemplateId = form.TemplateId,
            ShareLink = form.ShareLink,
            Title = form.Title,
            Description = form.Description,
            IsPublic = form.IsPublic,
            IsActive = form.IsActive,
            MaxSubmissions = form.MaxSubmissions,
            SubmissionCount = form.SubmissionCount,
            RequireAuth = form.RequireAuth,
            CreatedAt = form.CreatedAt,
            ExpiresAt = form.ExpiresAt,
            ViewCount = form.ViewCount,
            LastAccessedAt = form.LastAccessedAt,
            NotificationEmail = form.NotificationEmail,
            EnableNotifications = form.EnableNotifications
        };
    }
}
