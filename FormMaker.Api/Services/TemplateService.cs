using FormMaker.Api.Data;
using FormMaker.Api.Data.Entities;
using FormMaker.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace FormMaker.Api.Services;

/// <summary>
/// Service for managing form templates
/// </summary>
public class TemplateService
{
    private readonly FormMakerDbContext _dbContext;

    public TemplateService(FormMakerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Creates a new template for a user
    /// </summary>
    public async Task<Template> CreateTemplateAsync(Guid userId, CreateTemplateRequest request)
    {
        var template = new Template
        {
            UserId = userId,
            Name = request.Name,
            Description = request.Description,
            JsonData = request.JsonData,
            Category = request.Category,
            Tags = request.Tags,
            IsPublic = request.IsPublic,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _dbContext.Templates.Add(template);
        await _dbContext.SaveChangesAsync();

        return template;
    }

    /// <summary>
    /// Gets all templates for a user
    /// </summary>
    public async Task<List<Template>> GetUserTemplatesAsync(Guid userId, bool includePublic = true)
    {
        var query = _dbContext.Templates.AsQueryable();

        if (includePublic)
        {
            // Get user's templates + public templates
            query = query.Where(t => t.UserId == userId || t.IsPublic);
        }
        else
        {
            // Get only user's templates
            query = query.Where(t => t.UserId == userId);
        }

        return await query
            .OrderByDescending(t => t.UpdatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Gets a single template by ID
    /// </summary>
    public async Task<Template?> GetTemplateByIdAsync(Guid templateId, Guid? requestingUserId = null)
    {
        var template = await _dbContext.Templates.FindAsync(templateId);

        if (template == null)
        {
            return null;
        }

        // Check permissions: owner can see, or template is public
        if (requestingUserId.HasValue && template.UserId != requestingUserId.Value && !template.IsPublic)
        {
            return null; // Not authorized
        }

        return template;
    }

    /// <summary>
    /// Updates a template
    /// </summary>
    public async Task<Template?> UpdateTemplateAsync(Guid templateId, Guid userId, UpdateTemplateRequest request)
    {
        var template = await _dbContext.Templates.FindAsync(templateId);

        if (template == null || template.UserId != userId)
        {
            return null; // Not found or not authorized
        }

        // Update fields if provided
        if (request.Name != null)
            template.Name = request.Name;

        if (request.Description != null)
            template.Description = request.Description;

        if (request.JsonData != null)
        {
            template.JsonData = request.JsonData;
            template.Version++; // Increment version on data changes
        }

        if (request.Category != null)
            template.Category = request.Category;

        if (request.Tags != null)
            template.Tags = request.Tags;

        if (request.IsPublic.HasValue)
            template.IsPublic = request.IsPublic.Value;

        template.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        return template;
    }

    /// <summary>
    /// Deletes a template (soft delete)
    /// </summary>
    public async Task<bool> DeleteTemplateAsync(Guid templateId, Guid userId)
    {
        var template = await _dbContext.Templates.FindAsync(templateId);

        if (template == null || template.UserId != userId)
        {
            return false; // Not found or not authorized
        }

        // Soft delete
        template.IsDeleted = true;
        await _dbContext.SaveChangesAsync();

        return true;
    }

    /// <summary>
    /// Increments the usage count for a template
    /// </summary>
    public async Task IncrementUsageCountAsync(Guid templateId)
    {
        var template = await _dbContext.Templates.FindAsync(templateId);
        if (template != null)
        {
            template.UsageCount++;
            await _dbContext.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Verifies user owns a template
    /// </summary>
    public async Task<bool> UserOwnsTemplateAsync(Guid templateId, Guid userId)
    {
        var template = await _dbContext.Templates.FindAsync(templateId);
        return template != null && template.UserId == userId;
    }
}
