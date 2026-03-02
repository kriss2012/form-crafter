using FormMaker.Shared.Enums;

namespace FormMaker.Shared.Models.Elements;

/// <summary>
/// Radio button group element for single-selection options
/// </summary>
public class RadioGroupElement : FormElement
{
    public RadioGroupElement()
    {
        Type = ElementType.RadioGroup;
        Width = 300;
        Height = 120;
        Options = new List<string> { "Option 1", "Option 2", "Option 3" };
        Layout = "Vertical";
    }

    /// <summary>
    /// List of radio button options
    /// </summary>
    public List<string> Options { get; set; }

    /// <summary>
    /// Default selected value
    /// </summary>
    public string? SelectedValue { get; set; }

    /// <summary>
    /// Layout orientation (Vertical/Horizontal)
    /// </summary>
    public string Layout { get; set; }

    public override FormElement Clone()
    {
        return new RadioGroupElement
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
            Options = new List<string>(this.Options),
            SelectedValue = this.SelectedValue,
            Layout = this.Layout
        };
    }

    public override string GetDisplayName()
    {
        return "Radio Group";
    }
}
