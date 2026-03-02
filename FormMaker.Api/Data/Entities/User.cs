namespace FormMaker.Api.Data.Entities;

/// <summary>
/// User entity for authentication and authorization
/// </summary>
public class User
{
    /// <summary>
    /// Unique identifier for the user
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// User's email address (unique)
    /// </summary>
    public required string Email { get; set; }

    /// <summary>
    /// Hashed password (BCrypt)
    /// </summary>
    public required string PasswordHash { get; set; }

    /// <summary>
    /// User's display name (optional)
    /// </summary>
    public string? DisplayName { get; set; }

    /// <summary>
    /// Whether the user's email is verified
    /// </summary>
    public bool EmailVerified { get; set; } = false;

    /// <summary>
    /// When the user account was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When the user account was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When the user last logged in
    /// </summary>
    public DateTime? LastLoginAt { get; set; }

    /// <summary>
    /// Whether the account is active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Soft delete flag
    /// </summary>
    public bool IsDeleted { get; set; } = false;

    /// <summary>
    /// Navigation property: Templates owned by this user
    /// </summary>
    public ICollection<Template> Templates { get; set; } = new List<Template>();
}
