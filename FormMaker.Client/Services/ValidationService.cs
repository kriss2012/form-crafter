using FormMaker.Shared.Models;
using FormMaker.Shared.Models.Elements;
using System.Text.RegularExpressions;

namespace FormMaker.Client.Services;

public class ValidationService
{
    // Common regex patterns
    private static readonly Regex EmailRegex = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);
    private static readonly Regex PhoneRegex = new(@"^[\d\s\-\(\)\+\.]+$", RegexOptions.Compiled);
    private static readonly Regex UrlRegex = new(@"^https?:\/\/(www\.)?[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6}\b([-a-zA-Z0-9()@:%_\+.~#?&//=]*)$", RegexOptions.Compiled);

    /// <summary>
    /// Validates a single form element with a given value
    /// </summary>
    public ValidationResult ValidateElement(FormElement element, object? value)
    {
        var result = new ValidationResult
        {
            ElementId = element.Id,
            Value = value,
            IsValid = true
        };

        // Check if required field is empty
        if (element.IsRequired)
        {
            if (!ValidateRequired(value))
            {
                var errorMessage = element.RequiredErrorMessage ?? GetDefaultRequiredMessage(element);
                result.Errors.Add(errorMessage);
                result.IsValid = false;
            }
        }

        // If value is empty and not required, skip further validation
        if (IsValueEmpty(value) && !element.IsRequired)
        {
            return result;
        }

        // Apply element-specific validation rules
        foreach (var rule in element.ValidationRules)
        {
            var ruleResult = ValidateRule(rule, value, element);
            if (!ruleResult.isValid)
            {
                result.Errors.Add(ruleResult.errorMessage);
                result.IsValid = false;
            }
        }

        // Apply built-in validation based on element type
        ApplyElementTypeValidation(element, value, result);

        return result;
    }

    /// <summary>
    /// Validates an entire form
    /// </summary>
    public FormValidationResult ValidateForm(FormTemplate template, Dictionary<Guid, object?> formValues)
    {
        var formResult = new FormValidationResult
        {
            IsValid = true
        };

        // Get all validatable elements
        var validatableElements = template.Elements
            .Where(e => IsValidatableElement(e))
            .ToList();

        foreach (var element in validatableElements)
        {
            // Get the value for this element (or null if not provided)
            formValues.TryGetValue(element.Id, out var value);

            // Validate the element
            var elementResult = ValidateElement(element, value);
            formResult.ElementResults[element.Id] = elementResult;

            if (!elementResult.IsValid)
            {
                formResult.IsValid = false;
            }
        }

        return formResult;
    }

    private bool ValidateRequired(object? value)
    {
        if (value == null)
            return false;

        if (value is string str)
            return !string.IsNullOrWhiteSpace(str);

        if (value is System.Collections.ICollection collection)
            return collection.Count > 0;

        return true;
    }

    private (bool isValid, string errorMessage) ValidateRule(ValidationRule rule, object? value, FormElement element)
    {
        var valueStr = value?.ToString() ?? string.Empty;

        switch (rule.Type)
        {
            case ValidationType.Required:
                if (!ValidateRequired(value))
                {
                    return (false, rule.ErrorMessage.Or($"{GetFieldLabel(element)} is required"));
                }
                break;

            case ValidationType.Email:
                if (!string.IsNullOrWhiteSpace(valueStr) && !EmailRegex.IsMatch(valueStr))
                {
                    return (false, rule.ErrorMessage.Or("Please enter a valid email address"));
                }
                break;

            case ValidationType.Phone:
                if (!string.IsNullOrWhiteSpace(valueStr) && !PhoneRegex.IsMatch(valueStr))
                {
                    return (false, rule.ErrorMessage.Or("Please enter a valid phone number"));
                }
                break;

            case ValidationType.Url:
                if (!string.IsNullOrWhiteSpace(valueStr) && !UrlRegex.IsMatch(valueStr))
                {
                    return (false, rule.ErrorMessage.Or("Please enter a valid URL"));
                }
                break;

            case ValidationType.Number:
                if (!string.IsNullOrWhiteSpace(valueStr) && !double.TryParse(valueStr, out _))
                {
                    return (false, rule.ErrorMessage.Or("Please enter a valid number"));
                }
                break;

            case ValidationType.MinLength:
                if (rule.MinValue.HasValue && valueStr.Length < rule.MinValue.Value)
                {
                    return (false, rule.ErrorMessage.Or($"Minimum length is {rule.MinValue.Value} characters"));
                }
                break;

            case ValidationType.MaxLength:
                if (rule.MaxValue.HasValue && valueStr.Length > rule.MaxValue.Value)
                {
                    return (false, rule.ErrorMessage.Or($"Maximum length is {rule.MaxValue.Value} characters"));
                }
                break;

            case ValidationType.Range:
                if (double.TryParse(valueStr, out var numValue))
                {
                    if (rule.MinValue.HasValue && numValue < rule.MinValue.Value)
                    {
                        return (false, rule.ErrorMessage.Or($"Value must be at least {rule.MinValue.Value}"));
                    }
                    if (rule.MaxValue.HasValue && numValue > rule.MaxValue.Value)
                    {
                        return (false, rule.ErrorMessage.Or($"Value must be at most {rule.MaxValue.Value}"));
                    }
                }
                break;

            case ValidationType.Pattern:
                if (!string.IsNullOrWhiteSpace(rule.Pattern) && !string.IsNullOrWhiteSpace(valueStr))
                {
                    try
                    {
                        var regex = new Regex(rule.Pattern);
                        if (!regex.IsMatch(valueStr))
                        {
                            return (false, rule.ErrorMessage.Or("Value does not match the required pattern"));
                        }
                    }
                    catch
                    {
                        return (false, "Invalid validation pattern");
                    }
                }
                break;
        }

        return (true, string.Empty);
    }

    private void ApplyElementTypeValidation(FormElement element, object? value, ValidationResult result)
    {
        switch (element)
        {
            case TextInputElement textInput:
                ApplyTextInputValidation(textInput, value, result);
                break;

            case TextAreaElement textArea:
                ApplyTextAreaValidation(textArea, value, result);
                break;

            case DropdownElement dropdown:
                ApplyDropdownValidation(dropdown, value, result);
                break;

            case DatePickerElement datePicker:
                ApplyDatePickerValidation(datePicker, value, result);
                break;

            case FileUploadElement fileUpload:
                ApplyFileUploadValidation(fileUpload, value, result);
                break;
        }
    }

    private void ApplyTextInputValidation(TextInputElement textInput, object? value, ValidationResult result)
    {
        var valueStr = value?.ToString() ?? string.Empty;

        // MaxLength validation
        if (textInput.MaxLength.HasValue && valueStr.Length > textInput.MaxLength.Value)
        {
            result.Errors.Add($"Maximum length is {textInput.MaxLength.Value} characters");
            result.IsValid = false;
        }

        // Input type specific validation
        switch (textInput.InputType?.ToLower())
        {
            case "email":
                if (!string.IsNullOrWhiteSpace(valueStr) && !EmailRegex.IsMatch(valueStr))
                {
                    result.Errors.Add("Please enter a valid email address");
                    result.IsValid = false;
                }
                break;

            case "url":
                if (!string.IsNullOrWhiteSpace(valueStr) && !UrlRegex.IsMatch(valueStr))
                {
                    result.Errors.Add("Please enter a valid URL");
                    result.IsValid = false;
                }
                break;

            case "number":
                if (!string.IsNullOrWhiteSpace(valueStr) && !double.TryParse(valueStr, out _))
                {
                    result.Errors.Add("Please enter a valid number");
                    result.IsValid = false;
                }
                break;

            case "tel":
                if (!string.IsNullOrWhiteSpace(valueStr) && !PhoneRegex.IsMatch(valueStr))
                {
                    result.Errors.Add("Please enter a valid phone number");
                    result.IsValid = false;
                }
                break;
        }
    }

    private void ApplyTextAreaValidation(TextAreaElement textArea, object? value, ValidationResult result)
    {
        var valueStr = value?.ToString() ?? string.Empty;

        if (textArea.MaxLength.HasValue && valueStr.Length > textArea.MaxLength.Value)
        {
            result.Errors.Add($"Maximum length is {textArea.MaxLength.Value} characters");
            result.IsValid = false;
        }
    }

    private void ApplyDropdownValidation(DropdownElement dropdown, object? value, ValidationResult result)
    {
        if (dropdown.IsRequired && dropdown.AllowMultiple)
        {
            // For multi-select, value should be a collection
            if (value is System.Collections.ICollection collection && collection.Count == 0)
            {
                result.Errors.Add("Please select at least one option");
                result.IsValid = false;
            }
        }
    }

    private void ApplyDatePickerValidation(DatePickerElement datePicker, object? value, ValidationResult result)
    {
        if (value is DateTime dateValue)
        {
            if (datePicker.MinDate.HasValue && dateValue < datePicker.MinDate.Value)
            {
                result.Errors.Add($"Date must be after {datePicker.MinDate.Value:d}");
                result.IsValid = false;
            }

            if (datePicker.MaxDate.HasValue && dateValue > datePicker.MaxDate.Value)
            {
                result.Errors.Add($"Date must be before {datePicker.MaxDate.Value:d}");
                result.IsValid = false;
            }
        }
    }

    private void ApplyFileUploadValidation(FileUploadElement fileUpload, object? value, ValidationResult result)
    {
        // File validation would be handled client-side typically
        // This is a placeholder for server-side validation if needed
    }

    private bool IsValidatableElement(FormElement element)
    {
        return element is TextInputElement or
               TextAreaElement or
               CheckboxElement or
               DropdownElement or
               RadioGroupElement or
               DatePickerElement or
               FileUploadElement or
               SignatureElement;
    }

    private bool IsValueEmpty(object? value)
    {
        if (value == null)
            return true;

        if (value is string str)
            return string.IsNullOrWhiteSpace(str);

        if (value is System.Collections.ICollection collection)
            return collection.Count == 0;

        return false;
    }

    private string GetFieldLabel(FormElement element)
    {
        return element.Label ?? element.Placeholder ?? element.GetDisplayName();
    }

    private string GetDefaultRequiredMessage(FormElement element)
    {
        var label = GetFieldLabel(element);
        return $"{label} is required";
    }
}

/// <summary>
/// Extension method for string null coalescing with default
/// </summary>
public static class StringExtensions
{
    public static string Or(this string? value, string defaultValue)
    {
        return string.IsNullOrWhiteSpace(value) ? defaultValue : value;
    }
}
