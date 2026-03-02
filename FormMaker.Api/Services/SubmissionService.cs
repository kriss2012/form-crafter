using FormMaker.Api.Data;
using FormMaker.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace FormMaker.Api.Services;

/// <summary>
/// Service for managing form submissions
/// </summary>
public class SubmissionService
{
    private readonly FormMakerDbContext _context;

    public SubmissionService(FormMakerDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Submit a form response
    /// </summary>
    public async Task<Submission> SubmitFormAsync(
        Guid formId,
        string jsonData,
        string? ipAddress = null,
        string? userAgent = null,
        string? submitterEmail = null,
        Guid? userId = null)
    {
        // Verify form exists and is accepting submissions
        var form = await _context.Forms
            .FirstOrDefaultAsync(f => f.Id == formId);

        if (form == null)
        {
            throw new ArgumentException("Form not found", nameof(formId));
        }

        if (!form.IsActive)
        {
            throw new InvalidOperationException("This form is no longer accepting submissions");
        }

        // Check expiration
        if (form.ExpiresAt.HasValue && form.ExpiresAt.Value < DateTime.UtcNow)
        {
            throw new InvalidOperationException("This form has expired");
        }

        // Check submission limit
        if (form.MaxSubmissions.HasValue && form.SubmissionCount >= form.MaxSubmissions.Value)
        {
            throw new InvalidOperationException("This form has reached its maximum number of submissions");
        }

        // Create submission
        var submission = new Submission
        {
            FormId = formId,
            JsonData = jsonData,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            SubmitterEmail = submitterEmail,
            UserId = userId,
            SubmittedAt = DateTime.UtcNow
        };

        _context.Submissions.Add(submission);

        // Increment form submission count
        form.SubmissionCount++;

        await _context.SaveChangesAsync();

        return submission;
    }

    /// <summary>
    /// Get all submissions for a form
    /// </summary>
    public async Task<List<Submission>> GetSubmissionsByFormAsync(Guid formId, int? limit = null, int? offset = null)
    {
        var query = _context.Submissions
            .Where(s => s.FormId == formId)
            .OrderByDescending(s => s.SubmittedAt);

        if (offset.HasValue)
        {
            query = (IOrderedQueryable<Submission>)query.Skip(offset.Value);
        }

        if (limit.HasValue)
        {
            query = (IOrderedQueryable<Submission>)query.Take(limit.Value);
        }

        return await query.ToListAsync();
    }

    /// <summary>
    /// Get a single submission by ID
    /// </summary>
    public async Task<Submission?> GetSubmissionByIdAsync(Guid submissionId)
    {
        return await _context.Submissions
            .Include(s => s.Form)
            .FirstOrDefaultAsync(s => s.Id == submissionId);
    }

    /// <summary>
    /// Get submission count for a form
    /// </summary>
    public async Task<int> GetSubmissionCountAsync(Guid formId)
    {
        return await _context.Submissions
            .Where(s => s.FormId == formId)
            .CountAsync();
    }

    /// <summary>
    /// Mark a submission as reviewed
    /// </summary>
    public async Task<Submission> MarkAsReviewedAsync(Guid submissionId, string? reviewNotes = null)
    {
        var submission = await _context.Submissions.FindAsync(submissionId);
        if (submission == null)
        {
            throw new ArgumentException("Submission not found", nameof(submissionId));
        }

        submission.IsReviewed = true;
        submission.ReviewedAt = DateTime.UtcNow;
        submission.ReviewNotes = reviewNotes;

        await _context.SaveChangesAsync();
        return submission;
    }

    /// <summary>
    /// Delete a submission (soft delete)
    /// </summary>
    public async Task DeleteSubmissionAsync(Guid submissionId)
    {
        var submission = await _context.Submissions.FindAsync(submissionId);
        if (submission == null)
        {
            throw new ArgumentException("Submission not found", nameof(submissionId));
        }

        submission.IsDeleted = true;
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Get submissions with filters
    /// </summary>
    public async Task<(List<Submission> submissions, int totalCount)> GetFilteredSubmissionsAsync(
        Guid formId,
        DateTime? startDate = null,
        DateTime? endDate = null,
        bool? isReviewed = null,
        string? emailSearch = null,
        int page = 1,
        int pageSize = 20)
    {
        var query = _context.Submissions.Where(s => s.FormId == formId);

        // Apply filters
        if (startDate.HasValue)
        {
            query = query.Where(s => s.SubmittedAt >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(s => s.SubmittedAt <= endDate.Value);
        }

        if (isReviewed.HasValue)
        {
            query = query.Where(s => s.IsReviewed == isReviewed.Value);
        }

        if (!string.IsNullOrWhiteSpace(emailSearch))
        {
            query = query.Where(s => s.SubmitterEmail != null && s.SubmitterEmail.Contains(emailSearch));
        }

        var totalCount = await query.CountAsync();

        var submissions = await query
            .OrderByDescending(s => s.SubmittedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (submissions, totalCount);
    }

    /// <summary>
    /// Export submissions to CSV format
    /// </summary>
    public async Task<string> ExportSubmissionsToCsvAsync(Guid formId)
    {
        var submissions = await _context.Submissions
            .Where(s => s.FormId == formId)
            .OrderByDescending(s => s.SubmittedAt)
            .ToListAsync();

        if (submissions.Count == 0)
        {
            return "No submissions to export";
        }

        // Build CSV
        var csv = new System.Text.StringBuilder();
        csv.AppendLine("Submission ID,Submitted At,Email,IP Address,Reviewed,Form Data");

        foreach (var submission in submissions)
        {
            csv.AppendLine($"{submission.Id}," +
                          $"{submission.SubmittedAt:yyyy-MM-dd HH:mm:ss}," +
                          $"\"{submission.SubmitterEmail ?? "N/A"}\"," +
                          $"\"{submission.IpAddress ?? "N/A"}\"," +
                          $"{submission.IsReviewed}," +
                          $"\"{submission.JsonData.Replace("\"", "\"\"")}\"");
        }

        return csv.ToString();
    }
}
