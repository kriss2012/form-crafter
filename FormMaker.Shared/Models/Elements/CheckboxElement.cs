using FormMaker.Shared.Enums;

namespace FormMaker.Shared.Models.Elements;

/// <summary>
/// Checkbox input with label
/// </summary>
public class CheckboxElement : FormElement
{
    public CheckboxElement()
    {
        Type = ElementType.Checkbox;
        Width = 200;
        Height = 48;
        Label = "Checkbox Label";
    }

    /// <summary>
    /// Whether the checkbox is checked by default
    /// </summary>
    public bool DefaultChecked { get; set; } = false;

    /// <summary>
    /// Position of label relative to checkbox
    /// </summary>
    public string LabelPosition { get; set; } = "right"; // left, right, top, bottom

    public override FormElement Clone()
    {
        return new CheckboxElement
        {
            Id = Guid.NewGuid(),
            Type = this.Type,
            X = this.X + 10,
            Y = this.Y + 10,
            Width = this.Width,
            Height = this.Height,
            Properties = this.Properties.Clone(),
            Label = this.Label,
            IsRequired = this.IsRequired,
            DefaultChecked = this.DefaultChecked,
            LabelPosition = this.LabelPosition
        };
    }

    public override string GetDisplayName()
    {
        return "Checkbox";
    }
}
