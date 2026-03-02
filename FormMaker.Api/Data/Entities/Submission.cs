namespace FormMaker.Api.Data.Entities;

/// <summary>
/// Submission entity for storing form responses
/// </summary>
public class Submission
{
    /// <summary>
    /// Unique identifier for the submission
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Form this submission belongs to
    /// </summary>
    public Guid FormId { get; set; }

    /// <summary>
    /// JSON-serialized form data (field values)
    /// </summary>
    public required string JsonData { get; set; }

    /// <summary>
    /// When the submission was received
    /// </summary>
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// IP address of the submitter
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// User agent string of the submitter's browser
    /// </summary>
    public string? UserAgent { get; set; }

    /// <summary>
    /// User ID if the submitter was authenticated (optional)
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    /// Email address of the submitter (if provided)
    /// </summary>
    public string? SubmitterEmail { get; set; }

    /// <summary>
    /// Whether this submission has been reviewed
    /// </summary>
    public bool IsReviewed { get; set; } = false;

    /// <summary>
    /// When the submission was reviewed
    /// </summary>
    public DateTime? ReviewedAt { get; set; }

    /// <summary>
    /// Notes added by the reviewer
    /// </summary>
    public string? ReviewNotes { get; set; }

    /// <summary>
    /// Soft delete flag
    /// </summary>
    public bool IsDeleted { get; set; } = false;

    /// <summary>
    /// Navigation property: Form this submission belongs to
    /// </summary>
    public Form? Form { get; set; }
}
