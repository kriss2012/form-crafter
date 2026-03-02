using FormMaker.Shared.Enums;
using FormMaker.Shared.Models.Elements;
using System.Text.Json.Serialization;

namespace FormMaker.Shared.Models;

/// <summary>
/// Base class for all form elements
/// </summary>
[JsonDerivedType(typeof(LabelElement), typeDiscriminator: "label")]
[JsonDerivedType(typeof(TextInputElement), typeDiscriminator: "textInput")]
[JsonDerivedType(typeof(CheckboxElement), typeDiscriminator: "checkbox")]
[JsonDerivedType(typeof(ImageElement), typeDiscriminator: "image")]
[JsonDerivedType(typeof(TextAreaElement), typeDiscriminator: "textArea")]
[JsonDerivedType(typeof(DropdownElement), typeDiscriminator: "dropdown")]
[JsonDerivedType(typeof(RadioGroupElement), typeDiscriminator: "radioGroup")]
[JsonDerivedType(typeof(DatePickerElement), typeDiscriminator: "datePicker")]
[JsonDerivedType(typeof(FileUploadElement), typeDiscriminator: "fileUpload")]
[JsonDerivedType(typeof(SignatureElement), typeDiscriminator: "signature")]
[JsonDerivedType(typeof(DividerElement), typeDiscriminator: "divider")]
[JsonDerivedType(typeof(TableElement), typeDiscriminator: "table")]
public abstract class FormElement
{
    /// <summary>
    /// Unique identifier for the element
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Type of the element
    /// </summary>
    public ElementType Type { get; set; }

    /// <summary>
    /// X position on canvas (in pixels)
    /// </summary>
    public int X { get; set; }

    /// <summary>
    /// Y position on canvas (in pixels)
    /// </summary>
    public int Y { get; set; }

    /// <summary>
    /// Width of the element (in pixels)
    /// </summary>
    public int Width { get; set; } = 200;

    /// <summary>
    /// Height of the element (in pixels)
    /// </summary>
    public int Height { get; set; } = 40;

    /// <summary>
    /// Visual and styling properties
    /// </summary>
    public ElementProperties Properties { get; set; } = new ElementProperties();

    /// <summary>
    /// Whether the element is currently selected in the editor
    /// </summary>
    public bool IsSelected { get; set; } = false;

    /// <summary>
    /// Whether the element is locked (cannot be moved or edited)
    /// </summary>
    public bool IsLocked { get; set; } = false;

    /// <summary>
    /// Optional label or name for the element
    /// </summary>
    public string? Label { get; set; }

    /// <summary>
    /// Whether this field is required for form submission
    /// </summary>
    public bool IsRequired { get; set; } = false;

    /// <summary>
    /// Placeholder text for input elements
    /// </summary>
    public string? Placeholder { get; set; }

    /// <summary>
    /// Validation rules for this element
    /// </summary>
    public List<ValidationRule> ValidationRules { get; set; } = new();

    /// <summary>
    /// Custom error message for required field validation
    /// </summary>
    public string? RequiredErrorMessage { get; set; }

    /// <summary>
    /// Creates a deep copy of the element
    /// </summary>
    public abstract FormElement Clone();

    /// <summary>
    /// Gets the display name for this element type
    /// </summary>
    public virtual string GetDisplayName()
    {
        return Type.ToString();
    }
}
