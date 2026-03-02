namespace FormMaker.Api.Models;

/// <summary>
/// Request model for creating a new shareable form
/// </summary>
public class CreateFormRequest
{
    public required Guid TemplateId { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    public bool RequireAuth { get; set; } = false;
    public int? MaxSubmissions { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public string? NotificationEmail { get; set; }
    public bool EnableNotifications { get; set; } = false;
}

/// <summary>
/// Request model for updating form settings
/// </summary>
public class UpdateFormRequest
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public bool? IsActive { get; set; }
    public bool? IsPublic { get; set; }
    public int? MaxSubmissions { get; set; }
    public DateTime? ExpiresAt { get; set; }
}

/// <summary>
/// Response model for form data
/// </summary>
public class FormResponse
{
    public Guid Id { get; set; }
    public Guid TemplateId { get; set; }
    public required string ShareLink { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    public bool IsPublic { get; set; }
    public bool IsActive { get; set; }
    public int? MaxSubmissions { get; set; }
    public int SubmissionCount { get; set; }
    public bool RequireAuth { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public int ViewCount { get; set; }
    public DateTime? LastAccessedAt { get; set; }
    public string? NotificationEmail { get; set; }
    public bool EnableNotifications { get; set; }
}

/// <summary>
/// Response for getting a form by share link (includes template data)
/// </summary>
public class PublicFormResponse
{
    public Guid Id { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    public required string TemplateJsonData { get; set; }
    public bool RequireAuth { get; set; }
}

/// <summary>
/// List of forms response
/// </summary>
public class FormListResponse
{
    public required List<FormResponse> Forms { get; set; }
    public int TotalCount { get; set; }
}
