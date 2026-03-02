namespace FormMaker.Api.Data.Entities;

/// <summary>
/// Template entity for storing form templates
/// </summary>
public class Template
{
    /// <summary>
    /// Unique identifier for the template
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// User who owns this template
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Template name
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Optional description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// JSON-serialized FormTemplate data
    /// </summary>
    public required string JsonData { get; set; }

    /// <summary>
    /// Category for organization (e.g., Business, Legal, HR)
    /// </summary>
    public string? Category { get; set; }

    /// <summary>
    /// Tags for searchability (comma-separated or JSON array)
    /// </summary>
    public string? Tags { get; set; }

    /// <summary>
    /// Whether this template is publicly available
    /// </summary>
    public bool IsPublic { get; set; } = false;

    /// <summary>
    /// Whether this is a featured template in the gallery
    /// </summary>
    public bool IsFeatured { get; set; } = false;

    /// <summary>
    /// When the template was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When the template was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Version number for tracking changes
    /// </summary>
    public int Version { get; set; } = 1;

    /// <summary>
    /// Soft delete flag
    /// </summary>
    public bool IsDeleted { get; set; } = false;

    /// <summary>
    /// Number of times this template has been used
    /// </summary>
    public int UsageCount { get; set; } = 0;

    /// <summary>
    /// Navigation property: Owner of this template
    /// </summary>
    public User? User { get; set; }

    /// <summary>
    /// Navigation property: Forms created from this template
    /// </summary>
    public ICollection<Form> Forms { get; set; } = new List<Form>();
}
