namespace FormMaker.Shared.Models;

/// <summary>
/// Result of validating a form element
/// </summary>
public class ValidationResult
{
    /// <summary>
    /// ID of the element that was validated
    /// </summary>
    public Guid ElementId { get; set; }

    /// <summary>
    /// Whether the validation passed
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// List of validation error messages
    /// </summary>
    public List<string> Errors { get; set; } = new();

    /// <summary>
    /// Value that was validated
    /// </summary>
    public object? Value { get; set; }
}

/// <summary>
/// Result of validating an entire form
/// </summary>
public class FormValidationResult
{
    /// <summary>
    /// Whether all fields in the form are valid
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// Validation results for each element
    /// </summary>
    public Dictionary<Guid, ValidationResult> ElementResults { get; set; } = new();

    /// <summary>
    /// Count of total errors
    /// </summary>
    public int ErrorCount => ElementResults.Values.Sum(r => r.Errors.Count);

    /// <summary>
    /// Get all error messages
    /// </summary>
    public List<string> GetAllErrors()
    {
        return ElementResults.Values
            .SelectMany(r => r.Errors)
            .ToList();
    }

    /// <summary>
    /// Get errors for a specific element
    /// </summary>
    public List<string> GetErrorsForElement(Guid elementId)
    {
        return ElementResults.TryGetValue(elementId, out var result)
            ? result.Errors
            : new List<string>();
    }
}
