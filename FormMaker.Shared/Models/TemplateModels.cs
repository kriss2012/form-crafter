namespace FormMaker.Shared.Models;

/// <summary>
/// Request model for creating a new template
/// </summary>
public class CreateTemplateRequest
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    public required string JsonData { get; set; }
    public string? Category { get; set; }
    public string? Tags { get; set; }
    public bool IsPublic { get; set; } = false;
}

/// <summary>
/// Request model for updating a template
/// </summary>
public class UpdateTemplateRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? JsonData { get; set; }
    public string? Category { get; set; }
    public string? Tags { get; set; }
    public bool? IsPublic { get; set; }
}

/// <summary>
/// Response model for template data
/// </summary>
public class TemplateResponse
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public required string JsonData { get; set; }
    public string? Category { get; set; }
    public string? Tags { get; set; }
    public bool IsPublic { get; set; }
    public bool IsFeatured { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int Version { get; set; }
    public int UsageCount { get; set; }
}

/// <summary>
/// List of templates response
/// </summary>
public class TemplateListResponse
{
    public required List<TemplateResponse> Templates { get; set; }
    public int TotalCount { get; set; }
}

/// <summary>
/// Error response model
/// </summary>
public class ErrorResponse
{
    public required string Error { get; set; }
}
