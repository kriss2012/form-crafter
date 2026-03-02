using Microsoft.JSInterop;

namespace FormMaker.Client.Services;

/// <summary>
/// Service for managing accessibility features and screen reader announcements
/// </summary>
public class AccessibilityService
{
    private readonly IJSRuntime _jsRuntime;

    public AccessibilityService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    /// <summary>
    /// Announces a message to screen readers using ARIA live region
    /// </summary>
    /// <param name="message">The message to announce</param>
    /// <param name="priority">The priority level: "polite" (default) or "assertive"</param>
    public async Task AnnounceAsync(string message, string priority = "polite")
    {
        await _jsRuntime.InvokeVoidAsync("accessibilityHelper.announce", message, priority);
    }

    /// <summary>
    /// Announces that an element has been selected
    /// </summary>
    public async Task AnnounceElementSelectedAsync(string elementType, int x, int y)
    {
        await AnnounceAsync($"{elementType} selected at position {x}, {y}");
    }

    /// <summary>
    /// Announces that an element has started being dragged
    /// </summary>
    public async Task AnnounceDragStartAsync(string elementType)
    {
        await AnnounceAsync($"Started dragging {elementType}. Use arrow keys to move.");
    }

    /// <summary>
    /// Announces that an element has been moved
    /// </summary>
    public async Task AnnounceDragEndAsync(string elementType, int x, int y)
    {
        await AnnounceAsync($"{elementType} moved to position {x}, {y}");
    }

    /// <summary>
    /// Announces that an element has started being resized
    /// </summary>
    public async Task AnnounceResizeStartAsync(string elementType)
    {
        await AnnounceAsync($"Started resizing {elementType}");
    }

    /// <summary>
    /// Announces that an element has been resized
    /// </summary>
    public async Task AnnounceResizeEndAsync(string elementType, int width, int height)
    {
        await AnnounceAsync($"{elementType} resized to {width} by {height} pixels");
    }

    /// <summary>
    /// Announces that an element has been added to the canvas
    /// </summary>
    public async Task AnnounceElementAddedAsync(string elementType, int x, int y)
    {
        await AnnounceAsync($"{elementType} added to canvas at position {x}, {y}");
    }

    /// <summary>
    /// Announces that an element has been deleted
    /// </summary>
    public async Task AnnounceElementDeletedAsync(string elementType)
    {
        await AnnounceAsync($"{elementType} deleted");
    }

    /// <summary>
    /// Announces that multiple elements have been deleted
    /// </summary>
    public async Task AnnounceMultipleElementsDeletedAsync(int count)
    {
        await AnnounceAsync($"{count} elements deleted");
    }

    /// <summary>
    /// Announces validation errors
    /// </summary>
    public async Task AnnounceValidationErrorAsync(string message)
    {
        await AnnounceAsync(message, "assertive");
    }

    /// <summary>
    /// Announces successful actions
    /// </summary>
    public async Task AnnounceSuccessAsync(string message)
    {
        await AnnounceAsync(message);
    }
}
