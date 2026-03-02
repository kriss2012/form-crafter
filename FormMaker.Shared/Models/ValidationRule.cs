namespace FormMaker.Shared.Models;

/// <summary>
/// Represents a validation rule for form elements
/// </summary>
public class ValidationRule
{
    /// <summary>
    /// Type of validation to perform
    /// </summary>
    public ValidationType Type { get; set; }

    /// <summary>
    /// Error message to display when validation fails
    /// </summary>
    public string ErrorMessage { get; set; } = string.Empty;

    /// <summary>
    /// Minimum value for numeric/length validations
    /// </summary>
    public double? MinValue { get; set; }

    /// <summary>
    /// Maximum value for numeric/length validations
    /// </summary>
    public double? MaxValue { get; set; }

    /// <summary>
    /// Regular expression pattern for pattern matching
    /// </summary>
    public string? Pattern { get; set; }

    /// <summary>
    /// Custom validation function name
    /// </summary>
    public string? CustomValidatorName { get; set; }
}

/// <summary>
/// Types of validation rules
/// </summary>
public enum ValidationType
{
    Required,
    Email,
    Phone,
    Url,
    Number,
    MinLength,
    MaxLength,
    Range,
    Pattern,
    Custom
}
