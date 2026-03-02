using FormMaker.Api.Data;
using FormMaker.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace FormMaker.Api.Services;

/// <summary>
/// Service for managing shareable forms
/// </summary>
public class FormService
{
    private readonly FormMakerDbContext _context;

    public FormService(FormMakerDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Create a new shareable form from a template
    /// </summary>
    public async Task<Form> CreateFormAsync(
        Guid templateId,
        string title,
        string? description,
        bool requireAuth = false,
        int? maxSubmissions = null,
        DateTime? expiresAt = null,
        string? notificationEmail = null,
        bool enableNotifications = false)
    {
        // Verify template exists and belongs to a user
        var template = await _context.Templates
            .FirstOrDefaultAsync(t => t.Id == templateId);

        if (template == null)
        {
            throw new ArgumentException("Template not found", nameof(templateId));
        }

        // Generate unique share link
        var shareLink = GenerateShareLink();
        var retryCount = 0;
        while (await _context.Forms.AnyAsync(f => f.ShareLink == shareLink) && retryCount < 10)
        {
            shareLink = GenerateShareLink();
            retryCount++;
        }

        if (retryCount >= 10)
        {
            throw new InvalidOperationException("Unable to generate unique share link");
        }

        var form = new Form
        {
            TemplateId = templateId,
            ShareLink = shareLink,
            Title = title,
            Description = description,
            RequireAuth = requireAuth,
            MaxSubmissions = maxSubmissions,
            ExpiresAt = expiresAt,
            NotificationEmail = notificationEmail,
            EnableNotifications = enableNotifications,
            IsPublic = true,
            IsActive = true
        };

        _context.Forms.Add(form);
        await _context.SaveChangesAsync();

        return form;
    }

    /// <summary>
    /// Get a form by its share link (public access)
    /// </summary>
    public async Task<Form?> GetFormByShareLinkAsync(string shareLink)
    {
        var form = await _context.Forms
            .Include(f => f.Template)
            .FirstOrDefaultAsync(f => f.ShareLink == shareLink && f.IsPublic && f.IsActive);

        if (form != null)
        {
            // Check expiration
            if (form.ExpiresAt.HasValue && form.ExpiresAt.Value < DateTime.UtcNow)
            {
                return null; // Form has expired
            }

            // Check submission limit
            if (form.MaxSubmissions.HasValue && form.SubmissionCount >= form.MaxSubmissions.Value)
            {
                return null; // Form has reached max submissions
            }

            // Update view count
            form.ViewCount++;
            form.LastAccessedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        return form;
    }

    /// <summary>
    /// Get all forms for a specific template
    /// </summary>
    public async Task<List<Form>> GetFormsByTemplateAsync(Guid templateId)
    {
        return await _context.Forms
            .Where(f => f.TemplateId == templateId)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Get a form by ID (requires ownership check)
    /// </summary>
    public async Task<Form?> GetFormByIdAsync(Guid formId)
    {
        return await _context.Forms
            .Include(f => f.Template)
            .Include(f => f.Submissions)
            .FirstOrDefaultAsync(f => f.Id == formId);
    }

    /// <summary>
    /// Update form settings
    /// </summary>
    public async Task<Form> UpdateFormAsync(Guid formId, string? title, string? description, bool? isActive, bool? isPublic, int? maxSubmissions, DateTime? expiresAt)
    {
        var form = await _context.Forms.FindAsync(formId);
        if (form == null)
        {
            throw new ArgumentException("Form not found", nameof(formId));
        }

        if (title != null) form.Title = title;
        if (description != null) form.Description = description;
        if (isActive.HasValue) form.IsActive = isActive.Value;
        if (isPublic.HasValue) form.IsPublic = isPublic.Value;
        if (maxSubmissions.HasValue) form.MaxSubmissions = maxSubmissions.Value;
        if (expiresAt.HasValue) form.ExpiresAt = expiresAt.Value;

        await _context.SaveChangesAsync();
        return form;
    }

    /// <summary>
    /// Soft delete a form
    /// </summary>
    public async Task DeleteFormAsync(Guid formId)
    {
        var form = await _context.Forms.FindAsync(formId);
        if (form == null)
        {
            throw new ArgumentException("Form not found", nameof(formId));
        }

        form.IsDeleted = true;
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Generate a random share link (8 characters)
    /// </summary>
    private static string GenerateShareLink()
    {
        const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, 8)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }

    /// <summary>
    /// Increment submission count for a form
    /// </summary>
    public async Task IncrementSubmissionCountAsync(Guid formId)
    {
        var form = await _context.Forms.FindAsync(formId);
        if (form != null)
        {
            form.SubmissionCount++;
            await _context.SaveChangesAsync();
        }
    }
}
