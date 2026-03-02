using FormMaker.Shared.Enums;

namespace FormMaker.Shared.Models.Elements;

/// <summary>
/// Image or logo element
/// </summary>
public class ImageElement : FormElement
{
    public ImageElement()
    {
        Type = ElementType.Image;
        Width = 200;
        Height = 200;
        Label = "Image";
    }

    /// <summary>
    /// Image data as Base64 string (for MVP)
    /// </summary>
    public string? ImageData { get; set; }

    /// <summary>
    /// Image URL (alternative to Base64)
    /// </summary>
    public string? ImageUrl { get; set; }

    /// <summary>
    /// Alt text for accessibility
    /// </summary>
    public string AltText { get; set; } = "Image";

    /// <summary>
    /// Whether to maintain aspect ratio when resizing
    /// </summary>
    public bool MaintainAspectRatio { get; set; } = true;

    /// <summary>
    /// How the image should fit within its bounds
    /// </summary>
    public string ObjectFit { get; set; } = "contain"; // contain, cover, fill, none, scale-down

    public override FormElement Clone()
    {
        return new ImageElement
        {
            Id = Guid.NewGuid(),
            Type = this.Type,
            X = this.X + 10,
            Y = this.Y + 10,
            Width = this.Width,
            Height = this.Height,
            Properties = this.Properties.Clone(),
            Label = this.Label,
            ImageData = this.ImageData,
            ImageUrl = this.ImageUrl,
            AltText = this.AltText,
            MaintainAspectRatio = this.MaintainAspectRatio,
            ObjectFit = this.ObjectFit
        };
    }

    public override string GetDisplayName()
    {
        return "Image";
    }
}
