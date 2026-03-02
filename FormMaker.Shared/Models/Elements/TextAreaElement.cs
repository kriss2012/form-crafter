using FormMaker.Shared.Enums;

namespace FormMaker.Shared.Models.Elements;

/// <summary>
/// Multi-line text area input field
/// </summary>
public class TextAreaElement : FormElement
{
    public TextAreaElement()
    {
        Type = ElementType.TextArea;
        Width = 400;
        Height = 120;
        Placeholder = "Enter text...";
        Rows = 4;
    }

    /// <summary>
    /// Default value for the textarea
    /// </summary>
    public string? DefaultValue { get; set; }

    /// <summary>
    /// Maximum length of input
    /// </summary>
    public int? MaxLength { get; set; }

    /// <summary>
    /// Number of visible text rows
    /// </summary>
    public int Rows { get; set; }

    /// <summary>
    /// Whether to enable text wrapping
    /// </summary>
    public bool WrapText { get; set; } = true;

    /// <summary>
    /// Whether to allow resizing by user
    /// </summary>
    public bool Resizable { get; set; } = false;

    public override FormElement Clone()
    {
        return new TextAreaElement
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
            Rows = this.Rows,
            WrapText = this.WrapText,
            Resizable = this.Resizable
        };
    }

    public override string GetDisplayName()
    {
        return "Text Area";
    }
}
