namespace FormMaker.Api.Data.Entities;

/// <summary>
/// Form entity for shareable form instances
/// </summary>
public class Form
{
    /// <summary>
    /// Unique identifier for the form
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Template this form is based on
    /// </summary>
    public Guid TemplateId { get; set; }

    /// <summary>
    /// Unique share link slug (e.g., "abc123xyz")
    /// </summary>
    public required string ShareLink { get; set; }

    /// <summary>
    /// Display title for the form
    /// </summary>
    public required string Title { get; set; }

    /// <summary>
    /// Optional description shown to form fillers
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Whether this form is publicly accessible
    /// </summary>
    public bool IsPublic { get; set; } = true;

    /// <summary>
    /// Whether this form is currently active (accepting submissions)
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Maximum number of submissions allowed (null = unlimited)
    /// </summary>
    public int? MaxSubmissions { get; set; }

    /// <summary>
    /// Current number of submissions received
    /// </summary>
    public int SubmissionCount { get; set; } = 0;

    /// <summary>
    /// Whether authentication is required to fill this form
    /// </summary>
    public bool RequireAuth { get; set; } = false;

    /// <summary>
    /// When the form was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When the form expires (null = no expiration)
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// Number of times the form has been viewed
    /// </summary>
    public int ViewCount { get; set; } = 0;

    /// <summary>
    /// When the form was last accessed
    /// </summary>
    public DateTime? LastAccessedAt { get; set; }

    /// <summary>
    /// Email address to notify when submissions are received
    /// </summary>
    public string? NotificationEmail { get; set; }

    /// <summary>
    /// Whether to send email notifications for new submissions
    /// </summary>
    public bool EnableNotifications { get; set; } = false;

    /// <summary>
    /// Soft delete flag
    /// </summary>
    public bool IsDeleted { get; set; } = false;

    /// <summary>
    /// Navigation property: Template this form is based on
    /// </summary>
    public Template? Template { get; set; }

    /// <summary>
    /// Navigation property: Submissions for this form
    /// </summary>
    public ICollection<Submission> Submissions { get; set; } = new List<Submission>();
}
