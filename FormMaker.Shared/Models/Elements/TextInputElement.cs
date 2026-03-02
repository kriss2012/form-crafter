using FormMaker.Shared.Enums;

namespace FormMaker.Shared.Models.Elements;

/// <summary>
/// Single-line text input field
/// </summary>
public class TextInputElement : FormElement
{
    public TextInputElement()
    {
        Type = ElementType.TextInput;
        Width = 300;
        Height = 56;
        Placeholder = "Enter text...";
    }

    /// <summary>
    /// Default value for the input
    /// </summary>
    public string? DefaultValue { get; set; }

    /// <summary>
    /// Maximum length of input
    /// </summary>
    public int? MaxLength { get; set; }

    /// <summary>
    /// Input type (text, email, tel, number, etc.)
    /// </summary>
    public string InputType { get; set; } = "text";

    /// <summary>
    /// Pattern for validation (regex)
    /// </summary>
    public string? ValidationPattern { get; set; }

    /// <summary>
    /// Error message to show if validation fails
    /// </summary>
    public string? ValidationMessage { get; set; }

    public override FormElement Clone()
    {
        return new TextInputElement
        {
            Id = Guid.NewGuid(), // New ID for cloned element
            Type = this.Type,
            X = this.X + 10, // Offset slightly
            Y = this.Y + 10,
            Width = this.Width,
            Height = this.Height,
            Properties = this.Properties.Clone(),
            Label = this.Label,
            IsRequired = this.IsRequired,
            Placeholder = this.Placeholder,
            DefaultValue = this.DefaultValue,
            MaxLength = this.MaxLength,
            InputType = this.InputType,
            ValidationPattern = this.ValidationPattern,
            ValidationMessage = this.ValidationMessage
        };
    }

    public override string GetDisplayName()
    {
        return "Text Input";
    }
}
