using FormMaker.Shared.Enums;

namespace FormMaker.Shared.Models.Elements;

/// <summary>
/// Divider/Separator line element for visual organization
/// </summary>
public class DividerElement : FormElement
{
    public DividerElement()
    {
        Type = ElementType.Divider;
        Width = 400;
        Height = 2;
        Style = "Solid";
        Thickness = 2;
        Color = "#cccccc";
    }

    /// <summary>
    /// Line style (Solid, Dashed, Dotted)
    /// </summary>
    public string Style { get; set; }

    /// <summary>
    /// Line thickness in pixels
    /// </summary>
    public int Thickness { get; set; }

    /// <summary>
    /// Line color (hex code)
    /// </summary>
    public string Color { get; set; }

    public override FormElement Clone()
    {
        return new DividerElement
        {
            Id = Guid.NewGuid(), // New ID for cloned element
            Type = this.Type,
            X = this.X + 10, // Offset slightly
            Y = this.Y + 10,
            Width = this.Width,
            Height = this.Height,
            Properties = this.Properties.Clone(),
            Style = this.Style,
            Thickness = this.Thickness,
            Color = this.Color
        };
    }

    public override string GetDisplayName()
    {
        return "Divider";
    }
}
