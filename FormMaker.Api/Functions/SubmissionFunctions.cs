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
/// Azure Functions for form submission operations
/// </summary>
public class SubmissionFunctions
{
    private readonly ILogger<SubmissionFunctions> _logger;
    private readonly SubmissionService _submissionService;
    private readonly FormService _formService;
    private readonly AuthService _authService;

    public SubmissionFunctions(
        ILogger<SubmissionFunctions> logger,
        SubmissionService submissionService,
        FormService formService,
        AuthService authService)
    {
        _logger = logger;
        _submissionService = submissionService;
        _formService = formService;
        _authService = authService;
    }

    /// <summary>
    /// POST /api/forms/{shareLink}/submit - Submit a form (public endpoint)
    /// </summary>
    [Function("SubmitForm")]
    public async Task<HttpResponseData> SubmitForm(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "forms/{shareLink}/submit")] HttpRequestData req,
        string shareLink)
    {
        _logger.LogInformation("SubmitForm endpoint called for share link: {ShareLink}", shareLink);

        try
        {
            // Get form by share link
            var form = await _formService.GetFormByShareLinkAsync(shareLink);

            if (form == null)
            {
                return await CreateErrorResponse(req, HttpStatusCode.NotFound, "Form not found or expired");
            }

            // Parse request body (case-insensitive to handle camelCase from client)
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var request = await JsonSerializer.DeserializeAsync<SubmitFormRequest>(req.Body, options);
            if (request == null || string.IsNullOrWhiteSpace(request.JsonData))
            {
                return await CreateErrorResponse(req, HttpStatusCode.BadRequest, "Form data is required");
            }

            // Get IP address and User-Agent
            var ipAddress = GetClientIpAddress(req);
            var userAgent = GetUserAgent(req);

            // Check if user is authenticated (optional)
            var user = await TryAuthenticateRequest(req);
            var userId = user?.Id;

            // Submit the form
            var submission = await _submissionService.SubmitFormAsync(
                form.Id,
                request.JsonData,
                ipAddress,
                userAgent,
                request.SubmitterEmail,
                userId);

            // Return success response
            var response = req.CreateResponse(HttpStatusCode.Created);
            await response.WriteAsJsonAsync(new SubmitFormResponse
            {
                SubmissionId = submission.Id,
                Message = "Form submitted successfully"
            });
            return response;
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Form submission validation failed");
            return await CreateErrorResponse(req, HttpStatusCode.BadRequest, ex.Message);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Invalid JSON in request body");
            return await CreateErrorResponse(req, HttpStatusCode.BadRequest, "Invalid JSON format");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting form");
            return await CreateErrorResponse(req, HttpStatusCode.InternalServerError, "An error occurred");
        }
    }

    /// <summary>
    /// GET /api/submissions/form/{formId} - Get all submissions for a form (requires auth)
    /// </summary>
    [Function("GetSubmissionsByForm")]
    public async Task<HttpResponseData> GetSubmissionsByForm(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "submissions/form/{formId}")] HttpRequestData req,
        string formId)
    {
        _logger.LogInformation("GetSubmissionsByForm endpoint called for form: {FormId}", formId);

        try
        {
            // Authenticate user
            var user = await AuthenticateRequest(req);
            if (user == null)
            {
                return await CreateErrorResponse(req, HttpStatusCode.Unauthorized, "Authentication required");
            }

            // Parse form ID
            if (!Guid.TryParse(formId, out var formGuid))
            {
                return await CreateErrorResponse(req, HttpStatusCode.BadRequest, "Invalid form ID");
            }

            // Parse query parameters
            var query = System.Web.HttpUtility.ParseQueryString(req.Url.Query);
            var page = int.TryParse(query["page"], out var p) ? p : 1;
            var pageSize = int.TryParse(query["pageSize"], out var ps) ? ps : 20;
            var startDate = DateTime.TryParse(query["startDate"], out var sd) ? sd : (DateTime?)null;
            var endDate = DateTime.TryParse(query["endDate"], out var ed) ? ed : (DateTime?)null;
            var isReviewed = bool.TryParse(query["isReviewed"], out var ir) ? ir : (bool?)null;
            var emailSearch = query["emailSearch"];

            // Get submissions with filters
            var (submissions, totalCount) = await _submissionService.GetFilteredSubmissionsAsync(
                formGuid,
                startDate,
                endDate,
                isReviewed,
                emailSearch,
                page,
                pageSize);

            // Return response
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(new SubmissionListResponse
            {
                Submissions = submissions.Select(MapToResponse).ToList(),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            });
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting submissions");
            return await CreateErrorResponse(req, HttpStatusCode.InternalServerError, "An error occurred");
        }
    }

    /// <summary>
    /// GET /api/submissions/{id} - Get a single submission by ID (requires auth)
    /// </summary>
    [Function("GetSubmissionById")]
    public async Task<HttpResponseData> GetSubmissionById(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "submissions/{id}")] HttpRequestData req,
        string id)
    {
        _logger.LogInformation("GetSubmissionById endpoint called for ID: {Id}", id);

        try
        {
            // Authenticate user
            var user = await AuthenticateRequest(req);
            if (user == null)
            {
                return await CreateErrorResponse(req, HttpStatusCode.Unauthorized, "Authentication required");
            }

            // Parse submission ID
            if (!Guid.TryParse(id, out var submissionGuid))
            {
                return await CreateErrorResponse(req, HttpStatusCode.BadRequest, "Invalid submission ID");
            }

            // Get submission
            var submission = await _submissionService.GetSubmissionByIdAsync(submissionGuid);

            if (submission == null)
            {
                return await CreateErrorResponse(req, HttpStatusCode.NotFound, "Submission not found");
            }

            // Return response
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(MapToResponse(submission));
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting submission");
            return await CreateErrorResponse(req, HttpStatusCode.InternalServerError, "An error occurred");
        }
    }

    /// <summary>
    /// PUT /api/submissions/{id}/review - Mark submission as reviewed (requires auth)
    /// </summary>
    [Function("ReviewSubmission")]
    public async Task<HttpResponseData> ReviewSubmission(
        [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "submissions/{id}/review")] HttpRequestData req,
        string id)
    {
        _logger.LogInformation("ReviewSubmission endpoint called for ID: {Id}", id);

        try
        {
            // Authenticate user
            var user = await AuthenticateRequest(req);
            if (user == null)
            {
                return await CreateErrorResponse(req, HttpStatusCode.Unauthorized, "Authentication required");
            }

            // Parse submission ID
            if (!Guid.TryParse(id, out var submissionGuid))
            {
                return await CreateErrorResponse(req, HttpStatusCode.BadRequest, "Invalid submission ID");
            }

            // Parse request body (case-insensitive to handle camelCase from client)
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var request = await JsonSerializer.DeserializeAsync<ReviewSubmissionRequest>(req.Body, options);

            // Mark as reviewed
            var submission = await _submissionService.MarkAsReviewedAsync(submissionGuid, request?.ReviewNotes);

            // Return response
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(MapToResponse(submission));
            return response;
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Submission not found");
            return await CreateErrorResponse(req, HttpStatusCode.NotFound, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reviewing submission");
            return await CreateErrorResponse(req, HttpStatusCode.InternalServerError, "An error occurred");
        }
    }

    /// <summary>
    /// DELETE /api/submissions/{id} - Delete a submission (requires auth)
    /// </summary>
    [Function("DeleteSubmission")]
    public async Task<HttpResponseData> DeleteSubmission(
        [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "submissions/{id}")] HttpRequestData req,
        string id)
    {
        _logger.LogInformation("DeleteSubmission endpoint called for ID: {Id}", id);

        try
        {
            // Authenticate user
            var user = await AuthenticateRequest(req);
            if (user == null)
            {
                return await CreateErrorResponse(req, HttpStatusCode.Unauthorized, "Authentication required");
            }

            // Parse submission ID
            if (!Guid.TryParse(id, out var submissionGuid))
            {
                return await CreateErrorResponse(req, HttpStatusCode.BadRequest, "Invalid submission ID");
            }

            // Delete submission
            await _submissionService.DeleteSubmissionAsync(submissionGuid);

            // Return success response
            var response = req.CreateResponse(HttpStatusCode.NoContent);
            return response;
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Submission not found");
            return await CreateErrorResponse(req, HttpStatusCode.NotFound, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting submission");
            return await CreateErrorResponse(req, HttpStatusCode.InternalServerError, "An error occurred");
        }
    }

    /// <summary>
    /// GET /api/submissions/form/{formId}/export - Export submissions to CSV (requires auth)
    /// </summary>
    [Function("ExportSubmissions")]
    public async Task<HttpResponseData> ExportSubmissions(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "submissions/form/{formId}/export")] HttpRequestData req,
        string formId)
    {
        _logger.LogInformation("ExportSubmissions endpoint called for form: {FormId}", formId);

        try
        {
            // Authenticate user
            var user = await AuthenticateRequest(req);
            if (user == null)
            {
                return await CreateErrorResponse(req, HttpStatusCode.Unauthorized, "Authentication required");
            }

            // Parse form ID
            if (!Guid.TryParse(formId, out var formGuid))
            {
                return await CreateErrorResponse(req, HttpStatusCode.BadRequest, "Invalid form ID");
            }

            // Export to CSV
            var csv = await _submissionService.ExportSubmissionsToCsvAsync(formGuid);

            // Return CSV file
            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "text/csv");
            response.Headers.Add("Content-Disposition", $"attachment; filename=submissions-{formGuid}.csv");
            await response.WriteStringAsync(csv);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting submissions");
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

    private async Task<User?> TryAuthenticateRequest(HttpRequestData req)
    {
        try
        {
            return await AuthenticateRequest(req);
        }
        catch
        {
            return null;
        }
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

    private static string? GetClientIpAddress(HttpRequestData req)
    {
        // Try X-Forwarded-For header first (for proxies)
        if (req.Headers.TryGetValues("X-Forwarded-For", out var forwardedFor))
        {
            var ip = forwardedFor.FirstOrDefault()?.Split(',').FirstOrDefault()?.Trim();
            if (!string.IsNullOrEmpty(ip))
            {
                return ip;
            }
        }

        // Try X-Real-IP header
        if (req.Headers.TryGetValues("X-Real-IP", out var realIp))
        {
            return realIp.FirstOrDefault();
        }

        return null;
    }

    private static string? GetUserAgent(HttpRequestData req)
    {
        if (req.Headers.TryGetValues("User-Agent", out var userAgent))
        {
            return userAgent.FirstOrDefault();
        }

        return null;
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

    private static SubmissionResponse MapToResponse(Submission submission)
    {
        return new SubmissionResponse
        {
            Id = submission.Id,
            FormId = submission.FormId,
            JsonData = submission.JsonData,
            SubmittedAt = submission.SubmittedAt,
            IpAddress = submission.IpAddress,
            UserAgent = submission.UserAgent,
            UserId = submission.UserId,
            SubmitterEmail = submission.SubmitterEmail,
            IsReviewed = submission.IsReviewed,
            ReviewedAt = submission.ReviewedAt,
            ReviewNotes = submission.ReviewNotes
        };
    }
}
