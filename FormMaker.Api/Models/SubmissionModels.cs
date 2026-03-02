namespace FormMaker.Api.Models;

/// <summary>
/// Request model for submitting a form
/// </summary>
public class SubmitFormRequest
{
    public required string JsonData { get; set; }
    public string? SubmitterEmail { get; set; }
}

/// <summary>
/// Response model for submission data
/// </summary>
public class SubmissionResponse
{
    public Guid Id { get; set; }
    public Guid FormId { get; set; }
    public required string JsonData { get; set; }
    public DateTime SubmittedAt { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public Guid? UserId { get; set; }
    public string? SubmitterEmail { get; set; }
    public bool IsReviewed { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? ReviewNotes { get; set; }
}

/// <summary>
/// List of submissions response
/// </summary>
public class SubmissionListResponse
{
    public required List<SubmissionResponse> Submissions { get; set; }
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

/// <summary>
/// Request model for marking submission as reviewed
/// </summary>
public class ReviewSubmissionRequest
{
    public string? ReviewNotes { get; set; }
}

/// <summary>
/// Response for successful submission
/// </summary>
public class SubmitFormResponse
{
    public Guid SubmissionId { get; set; }
    public required string Message { get; set; }
}
