using FormMaker.Shared.Enums;

namespace FormMaker.Shared.Models.Elements;

/// <summary>
/// File upload input element
/// </summary>
public class FileUploadElement : FormElement
{
    public FileUploadElement()
    {
        Type = ElementType.FileUpload;
        Width = 400;
        Height = 56;
        AllowedExtensions = new List<string> { ".pdf", ".doc", ".docx", ".jpg", ".png" };
        MaxFileSize = 10; // MB
        Multiple = false;
    }

    /// <summary>
    /// Allowed file extensions (e.g., .pdf, .jpg)
    /// </summary>
    public List<string> AllowedExtensions { get; set; }

    /// <summary>
    /// Maximum file size in megabytes
    /// </summary>
    public int MaxFileSize { get; set; }

    /// <summary>
    /// Allow multiple file uploads
    /// </summary>
    public bool Multiple { get; set; }

    /// <summary>
    /// Button text for upload
    /// </summary>
    public string ButtonText { get; set; } = "Choose File";

    /// <summary>
    /// Helper text to display
    /// </summary>
    public string HelperText { get; set; } = "Select a file to upload";

    public override FormElement Clone()
    {
        return new FileUploadElement
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
            AllowedExtensions = new List<string>(this.AllowedExtensions),
            MaxFileSize = this.MaxFileSize,
            Multiple = this.Multiple,
            ButtonText = this.ButtonText,
            HelperText = this.HelperText
        };
    }

    public override string GetDisplayName()
    {
        return "File Upload";
    }
}
