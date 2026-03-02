using FormMaker.Shared.Enums;

namespace FormMaker.Shared.Models.Elements;

/// <summary>
/// Static text label or heading
/// </summary>
public class LabelElement : FormElement
{
    public LabelElement()
    {
        Type = ElementType.Label;
        Width = 200;
        Height = 30;
        Text = "Label Text";
        Properties = new ElementProperties
        {
            FontSize = 18,
            Bold = false
        };
    }

    /// <summary>
    /// The text content to display
    /// </summary>
    public string Text { get; set; } = "Label";

    /// <summary>
    /// Heading level (H1-H6) or paragraph (P)
    /// </summary>
    public string HeadingLevel { get; set; } = "P"; // H1, H2, H3, H4, H5, H6, P

    /// <summary>
    /// Whether text should wrap or overflow
    /// </summary>
    public bool WordWrap { get; set; } = true;

    public override FormElement Clone()
    {
        return new LabelElement
        {
            Id = Guid.NewGuid(),
            Type = this.Type,
            X = this.X + 10,
            Y = this.Y + 10,
            Width = this.Width,
            Height = this.Height,
            Properties = this.Properties.Clone(),
            Label = this.Label,
            Text = this.Text,
            HeadingLevel = this.HeadingLevel,
            WordWrap = this.WordWrap
        };
    }

    public override string GetDisplayName()
    {
        return HeadingLevel == "P" ? "Label" : HeadingLevel;
    }
}
