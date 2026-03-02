using FormMaker.Shared.Enums;

namespace FormMaker.Shared.Models.Elements;

/// <summary>
/// Date picker input field
/// </summary>
public class DatePickerElement : FormElement
{
    public DatePickerElement()
    {
        Type = ElementType.DatePicker;
        Width = 300;
        Height = 56;
        Placeholder = "Select a date...";
        DateFormat = "MM/dd/yyyy";
    }

    /// <summary>
    /// Default date value
    /// </summary>
    public DateTime? DefaultValue { get; set; }

    /// <summary>
    /// Minimum allowed date
    /// </summary>
    public DateTime? MinDate { get; set; }

    /// <summary>
    /// Maximum allowed date
    /// </summary>
    public DateTime? MaxDate { get; set; }

    /// <summary>
    /// Date format string (e.g., MM/dd/yyyy, yyyy-MM-dd)
    /// </summary>
    public string DateFormat { get; set; }

    /// <summary>
    /// Include time picker along with date
    /// </summary>
    public bool IncludeTime { get; set; } = false;

    /// <summary>
    /// First day of the week (0 = Sunday, 1 = Monday)
    /// </summary>
    public int FirstDayOfWeek { get; set; } = 0;

    public override FormElement Clone()
    {
        return new DatePickerElement
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
            MinDate = this.MinDate,
            MaxDate = this.MaxDate,
            DateFormat = this.DateFormat,
            IncludeTime = this.IncludeTime,
            FirstDayOfWeek = this.FirstDayOfWeek
        };
    }

    public override string GetDisplayName()
    {
        return "Date Picker";
    }
}
