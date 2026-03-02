using FormMaker.Shared.Enums;

namespace FormMaker.Shared.Models;

/// <summary>
/// Styling and visual properties for form elements
/// </summary>
public class ElementProperties
{
    // Text styling
    public int FontSize { get; set; } = 18;
    public string FontFamily { get; set; } = "Roboto";
    public string Color { get; set; } = "#1a1a1a";
    public bool Bold { get; set; } = false;
    public bool Italic { get; set; } = false;
    public bool Underline { get; set; } = false;
    public TextAlignment Alignment { get; set; } = TextAlignment.Left;
    public int LineHeight { get; set; } = 24;
    public int LetterSpacing { get; set; } = 0;

    // Background
    public string BackgroundColor { get; set; } = "transparent";
    public double BackgroundOpacity { get; set; } = 1.0;

    // Border
    public string BorderStyle { get; set; } = "none"; // none, solid, dashed, dotted
    public int BorderWidth { get; set; } = 1;
    public string BorderColor { get; set; } = "#cccccc";
    public int BorderRadius { get; set; } = 0;

    // Spacing
    public int PaddingTop { get; set; } = 0;
    public int PaddingRight { get; set; } = 0;
    public int PaddingBottom { get; set; } = 0;
    public int PaddingLeft { get; set; } = 0;
    public int MarginTop { get; set; } = 0;
    public int MarginRight { get; set; } = 0;
    public int MarginBottom { get; set; } = 0;
    public int MarginLeft { get; set; } = 0;

    // Shadow
    public string BoxShadow { get; set; } = "none"; // none, small, medium, large
    public string ShadowColor { get; set; } = "#000000";

    // Other
    public double Opacity { get; set; } = 1.0;
    public int Rotation { get; set; } = 0; // degrees
    public int ZIndex { get; set; } = 0;

    /// <summary>
    /// Helper method to get padding as CSS string
    /// </summary>
    public string GetPaddingCss()
    {
        return $"{PaddingTop}px {PaddingRight}px {PaddingBottom}px {PaddingLeft}px";
    }

    /// <summary>
    /// Helper method to get margin as CSS string
    /// </summary>
    public string GetMarginCss()
    {
        return $"{MarginTop}px {MarginRight}px {MarginBottom}px {MarginLeft}px";
    }

    /// <summary>
    /// Creates a deep copy of the properties
    /// </summary>
    public ElementProperties Clone()
    {
        return (ElementProperties)this.MemberwiseClone();
    }
}
