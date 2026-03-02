using FormMaker.Shared.Enums;

namespace FormMaker.Shared.Models.Elements;

/// <summary>
/// Dropdown/Select input field
/// </summary>
public class DropdownElement : FormElement
{
    public DropdownElement()
    {
        Type = ElementType.Dropdown;
        Width = 300;
        Height = 56;
        Placeholder = "Select an option...";
        Options = new List<string> { "Option 1", "Option 2", "Option 3" };
    }

    /// <summary>
    /// List of options available in the dropdown
    /// </summary>
    public List<string> Options { get; set; }

    /// <summary>
    /// Default selected value
    /// </summary>
    public string? DefaultValue { get; set; }

    /// <summary>
    /// Allow multiple selections
    /// </summary>
    public bool AllowMultiple { get; set; } = false;

    /// <summary>
    /// Show search/filter box for long lists
    /// </summary>
    public bool EnableSearch { get; set; } = false;

    public override FormElement Clone()
    {
        return new DropdownElement
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
            Options = new List<string>(this.Options),
            DefaultValue = this.DefaultValue,
            AllowMultiple = this.AllowMultiple,
            EnableSearch = this.EnableSearch
        };
    }

    public override string GetDisplayName()
    {
        return "Dropdown";
    }
}
