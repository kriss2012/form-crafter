using FormMaker.Shared.Enums;

namespace FormMaker.Shared.Models.Elements;

/// <summary>
/// Signature field element with canvas-based signature pad
/// </summary>
public class SignatureElement : FormElement
{
    public SignatureElement()
    {
        Type = ElementType.Signature;
        Width = 400;
        Height = 150;
        BorderColor = "#000000";
        LineWidth = 2;
        BackgroundColor = "#ffffff";
    }

    /// <summary>
    /// Saved signature as Base64 PNG
    /// </summary>
    public string? SignatureData { get; set; }

    /// <summary>
    /// Border color for signature pad
    /// </summary>
    public string BorderColor { get; set; }

    /// <summary>
    /// Line width for signature drawing
    /// </summary>
    public int LineWidth { get; set; }

    /// <summary>
    /// Background color of signature pad
    /// </summary>
    public string BackgroundColor { get; set; }

    public override FormElement Clone()
    {
        return new SignatureElement
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
            SignatureData = this.SignatureData,
            BorderColor = this.BorderColor,
            LineWidth = this.LineWidth,
            BackgroundColor = this.BackgroundColor
        };
    }

    public override string GetDisplayName()
    {
        return "Signature";
    }
}
