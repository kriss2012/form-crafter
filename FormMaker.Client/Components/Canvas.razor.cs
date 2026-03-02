using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using FormMaker.Shared.Models;
using FormMaker.Shared.Models.Elements;
using FormMaker.Shared.Enums;

namespace FormMaker.Client.Components;

public partial class Canvas : IAsyncDisposable
{
    [Parameter]
    public FormTemplate CurrentTemplate { get; set; } = new FormTemplate();

    [Parameter]
    public EventCallback<FormElement> OnElementSelected { get; set; }

    [Parameter]
    public EventCallback<FormElement> OnElementDropped { get; set; }

    [Inject]
    private IJSRuntime JSRuntime { get; set; } = default!;

    [Inject]
    private FormMaker.Client.Services.AccessibilityService AccessibilityService { get; set; } = default!;

    // Helper method to get current page elements for multi-page support
    private List<FormElement> GetCurrentPageElements()
    {
        // If multi-page mode is enabled and we have pages, use current page
        if (CurrentTemplate.IsMultiPage && CurrentTemplate.CurrentPage != null)
        {
            return CurrentTemplate.CurrentPage.Elements;
        }

        // Fallback to legacy Elements list for backward compatibility
        return CurrentTemplate.Elements;
    }

    private DotNetObjectReference<Canvas>? dotNetHelper;
    private DotNetObjectReference<Canvas>? resizeDotNetHelper;
    private DotNetObjectReference<Canvas>? multiSelectDotNetHelper;
    private DotNetObjectReference<Canvas>? rotateDotNetHelper;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            dotNetHelper = DotNetObjectReference.Create(this);
            resizeDotNetHelper = DotNetObjectReference.Create(this);
            multiSelectDotNetHelper = DotNetObjectReference.Create(this);
            rotateDotNetHelper = DotNetObjectReference.Create(this);
            await JSRuntime.InvokeVoidAsync("canvasDragHandler.initialize", "main-canvas", dotNetHelper);
            await JSRuntime.InvokeVoidAsync("resizeHandler.initialize", "main-canvas", resizeDotNetHelper);
            await JSRuntime.InvokeVoidAsync("multiSelectHandler.initialize", "main-canvas", multiSelectDotNetHelper);
            await JSRuntime.InvokeVoidAsync("rotationHandler.initialize", "main-canvas", rotateDotNetHelper);
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (dotNetHelper != null)
        {
            await JSRuntime.InvokeVoidAsync("canvasDragHandler.cleanup");
            dotNetHelper.Dispose();
        }

        if (resizeDotNetHelper != null)
        {
            await JSRuntime.InvokeVoidAsync("resizeHandler.cleanup");
            resizeDotNetHelper.Dispose();
        }

        if (multiSelectDotNetHelper != null)
        {
            await JSRuntime.InvokeVoidAsync("multiSelectHandler.cleanup");
            multiSelectDotNetHelper.Dispose();
        }

        if (rotateDotNetHelper != null)
        {
            await JSRuntime.InvokeVoidAsync("rotationHandler.cleanup");
            rotateDotNetHelper.Dispose();
        }
    }

    private string GetPageSizeClass()
    {
        return CurrentTemplate.PageSize switch
        {
            PageSize.A4 => "a4",
            PageSize.Letter => "letter",
            PageSize.Legal => "legal",
            _ => ""
        };
    }

    private void HandleCanvasClick(MouseEventArgs e)
    {
        // Deselect all elements when clicking on empty canvas
        if (CurrentTemplate.IsMultiPage && CurrentTemplate.CurrentPage != null)
        {
            CurrentTemplate.CurrentPage.ClearSelection();
        }
        else
        {
            CurrentTemplate.ClearSelection();
        }
        StateHasChanged();
    }

    private async void HandleElementClick(FormElement element)
    {
        // Check if Ctrl key is pressed for multi-selection
        // Note: JavaScript interop will be needed to detect Ctrl key
        // For now, implement basic single selection
        if (CurrentTemplate.IsMultiPage && CurrentTemplate.CurrentPage != null)
        {
            CurrentTemplate.CurrentPage.ClearSelection();
        }
        else
        {
            CurrentTemplate.ClearSelection();
        }
        element.IsSelected = true;

        // Announce element selection to screen readers
        await AccessibilityService.AnnounceElementSelectedAsync(element.GetDisplayName(), element.X, element.Y);

        await OnElementSelected.InvokeAsync(element);
        StateHasChanged();
    }

    [JSInvokable]
    public async Task OnElementClickWithModifiers(string elementId, bool ctrlKey, bool shiftKey)
    {
        // Search in current page elements (multi-page support)
        var currentElements = GetCurrentPageElements();
        var element = currentElements.FirstOrDefault(e => e.Id.ToString() == elementId);
        if (element == null) return;

        if (ctrlKey)
        {
            // Multi-select: Toggle selection of clicked element
            element.IsSelected = !element.IsSelected;
        }
        else
        {
            // Single select: Clear other selections on current page only
            if (CurrentTemplate.IsMultiPage && CurrentTemplate.CurrentPage != null)
            {
                CurrentTemplate.CurrentPage.ClearSelection();
            }
            else
            {
                CurrentTemplate.ClearSelection();
            }
            element.IsSelected = true;
        }

        // Notify parent with the clicked element
        await OnElementSelected.InvokeAsync(element);
        StateHasChanged();
    }

    private async Task HandleMouseDown(MouseEventArgs e, FormElement element)
    {
        // Start dragging
        isDragging = true;
        draggedElement = element;

        // Calculate offset from element's top-left corner
        dragOffsetX = (int)e.OffsetX;
        dragOffsetY = (int)e.OffsetY;

        // Initialize drag position to current element position
        dragX = element.X;
        dragY = element.Y;

        // Announce drag start to screen readers
        await AccessibilityService.AnnounceDragStartAsync(element.GetDisplayName());

        // Notify JavaScript to start tracking
        await JSRuntime.InvokeVoidAsync("canvasDragHandler.startDrag", element.Id, dragOffsetX, dragOffsetY);

        StateHasChanged();
    }

    [JSInvokable]
    public void OnDragMove(double x, double y)
    {
        if (isDragging && draggedElement != null)
        {
            int newX = (int)x;
            int newY = (int)y;

            // Apply grid snapping
            int snapX = SnapToGrid(newX);
            int snapY = SnapToGrid(newY);

            // Constrain to canvas bounds
            dragX = Math.Max(0, Math.Min(snapX, CurrentTemplate.WidthInPixels - draggedElement.Width));
            dragY = Math.Max(0, Math.Min(snapY, CurrentTemplate.HeightInPixels - draggedElement.Height));

            // Check for alignment guides
            CheckAlignmentGuidesForDrag(dragX, dragY, draggedElement);

            // Check for smart spacing guides
            CalculateSmartGuides(dragX, dragY, draggedElement);

            StateHasChanged();
        }
    }

    [JSInvokable]
    public async Task OnDragEnd(double x, double y)
    {
        if (isDragging && draggedElement != null)
        {
            int newX = (int)x;
            int newY = (int)y;

            // Apply grid snapping
            int snapX = SnapToGrid(newX);
            int snapY = SnapToGrid(newY);

            // Finalize the element position with bounds checking
            draggedElement.X = Math.Max(0, Math.Min(snapX, CurrentTemplate.WidthInPixels - draggedElement.Width));
            draggedElement.Y = Math.Max(0, Math.Min(snapY, CurrentTemplate.HeightInPixels - draggedElement.Height));

            CurrentTemplate.MarkAsUpdated();

            // Announce drag end to screen readers
            await AccessibilityService.AnnounceDragEndAsync(draggedElement.GetDisplayName(), draggedElement.X, draggedElement.Y);

            // Notify parent component
            await OnElementDropped.InvokeAsync(draggedElement);

            // Reset drag state
            isDragging = false;
            draggedElement = null;
            showAlignmentGuides = false;
            verticalGuidePosition = null;
            horizontalGuidePosition = null;
            spacingGuides.Clear();

            StateHasChanged();
        }
    }

    private int SnapToGrid(int value)
    {
        return (int)(Math.Round((double)value / GRID_SIZE) * GRID_SIZE);
    }

    private void CheckAlignmentGuides(FormElement element)
    {
        int canvasWidth = CurrentTemplate.WidthInPixels;
        int canvasHeight = CurrentTemplate.HeightInPixels;
        int centerX = canvasWidth / 2;
        int centerY = canvasHeight / 2;
        int elementCenterX = element.X + element.Width / 2;
        int elementCenterY = element.Y + element.Height / 2;

        // Check for center alignment
        if (Math.Abs(elementCenterX - centerX) < SNAP_THRESHOLD)
        {
            element.X = centerX - element.Width / 2;
            verticalGuidePosition = centerX;
            showAlignmentGuides = true;
        }
        else
        {
            verticalGuidePosition = null;
        }

        if (Math.Abs(elementCenterY - centerY) < SNAP_THRESHOLD)
        {
            element.Y = centerY - element.Height / 2;
            horizontalGuidePosition = centerY;
            showAlignmentGuides = true;
        }
        else
        {
            horizontalGuidePosition = null;
        }

        if (verticalGuidePosition == null && horizontalGuidePosition == null)
        {
            showAlignmentGuides = false;
        }
    }

    private void CheckAlignmentGuidesForDrag(int x, int y, FormElement element)
    {
        int canvasWidth = CurrentTemplate.WidthInPixels;
        int canvasHeight = CurrentTemplate.HeightInPixels;
        int centerX = canvasWidth / 2;
        int centerY = canvasHeight / 2;
        int elementCenterX = x + element.Width / 2;
        int elementCenterY = y + element.Height / 2;

        // Check for center alignment
        if (Math.Abs(elementCenterX - centerX) < SNAP_THRESHOLD)
        {
            dragX = centerX - element.Width / 2;
            verticalGuidePosition = centerX;
            showAlignmentGuides = true;
        }
        else
        {
            verticalGuidePosition = null;
        }

        if (Math.Abs(elementCenterY - centerY) < SNAP_THRESHOLD)
        {
            dragY = centerY - element.Height / 2;
            horizontalGuidePosition = centerY;
            showAlignmentGuides = true;
        }
        else
        {
            horizontalGuidePosition = null;
        }

        if (verticalGuidePosition == null && horizontalGuidePosition == null)
        {
            showAlignmentGuides = false;
        }
    }

    private RenderFragment RenderElement(FormElement element) => builder =>
    {
        // Get box shadow CSS based on preset
        var boxShadow = element.Properties.BoxShadow switch
        {
            "small" => $"0 1px 3px rgba(0,0,0,0.12), 0 1px 2px rgba(0,0,0,0.24)",
            "medium" => $"0 3px 6px rgba(0,0,0,0.15), 0 2px 4px rgba(0,0,0,0.12)",
            "large" => $"0 10px 20px rgba(0,0,0,0.15), 0 3px 6px rgba(0,0,0,0.10)",
            _ => "none"
        };

        // Apply background color with opacity
        var bgColor = element.Properties.BackgroundColor;
        if (element.Properties.BackgroundOpacity < 1.0 && bgColor != "transparent")
        {
            // Convert hex to rgba if needed
            if (bgColor.StartsWith("#"))
            {
                var hex = bgColor.TrimStart('#');
                if (hex.Length == 6)
                {
                    var r = Convert.ToInt32(hex.Substring(0, 2), 16);
                    var g = Convert.ToInt32(hex.Substring(2, 2), 16);
                    var b = Convert.ToInt32(hex.Substring(4, 2), 16);
                    bgColor = $"rgba({r},{g},{b},{element.Properties.BackgroundOpacity})";
                }
            }
        }

        var style = $"width: 100%; height: 100%; " +
                    $"font-size: {element.Properties.FontSize}px; " +
                    $"font-family: {element.Properties.FontFamily}; " +
                    $"color: {element.Properties.Color}; " +
                    $"background-color: {bgColor}; " +
                    $"border: {element.Properties.BorderWidth}px {element.Properties.BorderStyle} {element.Properties.BorderColor}; " +
                    $"border-radius: {element.Properties.BorderRadius}px; " +
                    $"padding: {element.Properties.GetPaddingCss()}; " +
                    $"margin: {element.Properties.GetMarginCss()}; " +
                    $"text-align: {element.Properties.Alignment.ToString().ToLower()}; " +
                    $"font-weight: {(element.Properties.Bold ? "bold" : "normal")}; " +
                    $"font-style: {(element.Properties.Italic ? "italic" : "normal")}; " +
                    $"text-decoration: {(element.Properties.Underline ? "underline" : "none")}; " +
                    $"opacity: {element.Properties.Opacity}; " +
                    $"box-shadow: {boxShadow}; " +
                    $"transform: rotate({element.Properties.Rotation}deg); " +
                    $"pointer-events: none;"; // Prevent child elements from blocking drag

        switch (element)
        {
            case TextInputElement textInput:
                builder.OpenElement(0, "input");
                builder.AddAttribute(1, "type", textInput.InputType);
                builder.AddAttribute(2, "placeholder", textInput.Placeholder);
                builder.AddAttribute(3, "value", textInput.DefaultValue);
                builder.AddAttribute(4, "required", textInput.IsRequired);
                builder.AddAttribute(5, "style", style);
                builder.AddAttribute(6, "disabled", true); // Disabled in editor mode
                builder.CloseElement();
                break;

            case LabelElement label:
                var tagName = label.HeadingLevel.ToLower();
                builder.OpenElement(0, tagName);
                builder.AddAttribute(1, "style", style);
                builder.AddContent(2, label.Text);
                builder.CloseElement();
                break;

            case CheckboxElement checkbox:
                builder.OpenElement(0, "label");
                builder.AddAttribute(1, "style", style + " display: flex; align-items: center; gap: 8px;");

                builder.OpenElement(2, "input");
                builder.AddAttribute(3, "type", "checkbox");
                builder.AddAttribute(4, "checked", checkbox.DefaultChecked);
                builder.AddAttribute(5, "disabled", true);
                builder.AddAttribute(6, "style", "width: 24px; height: 24px;");
                builder.CloseElement();

                builder.OpenElement(7, "span");
                builder.AddContent(8, checkbox.Label ?? "Checkbox");
                builder.CloseElement();

                builder.CloseElement();
                break;

            case ImageElement image:
                builder.OpenElement(0, "div");
                builder.AddAttribute(1, "style", style + " display: flex; align-items: center; justify-content: center; overflow: hidden;");

                if (!string.IsNullOrEmpty(image.ImageData))
                {
                    builder.OpenElement(2, "img");
                    builder.AddAttribute(3, "src", image.ImageData);
                    builder.AddAttribute(4, "alt", image.AltText);
                    builder.AddAttribute(5, "style", $"max-width: 100%; max-height: 100%; object-fit: {image.ObjectFit};");
                    builder.CloseElement();
                }
                else if (!string.IsNullOrEmpty(image.ImageUrl))
                {
                    builder.OpenElement(2, "img");
                    builder.AddAttribute(3, "src", image.ImageUrl);
                    builder.AddAttribute(4, "alt", image.AltText);
                    builder.AddAttribute(5, "style", $"max-width: 100%; max-height: 100%; object-fit: {image.ObjectFit};");
                    builder.CloseElement();
                }
                else
                {
                    builder.OpenElement(2, "div");
                    builder.AddAttribute(3, "style", "color: #999; text-align: center;");
                    builder.AddContent(4, "📷 Image Placeholder");
                    builder.CloseElement();
                }

                builder.CloseElement();
                break;

            case TextAreaElement textArea:
                builder.OpenElement(0, "textarea");
                builder.AddAttribute(1, "placeholder", textArea.Placeholder);
                builder.AddAttribute(2, "rows", textArea.Rows);
                builder.AddAttribute(3, "value", textArea.DefaultValue);
                builder.AddAttribute(4, "required", textArea.IsRequired);
                builder.AddAttribute(5, "maxlength", textArea.MaxLength);
                builder.AddAttribute(6, "style", style + $" resize: {(textArea.Resizable ? "both" : "none")}; white-space: {(textArea.WrapText ? "pre-wrap" : "pre")};");
                builder.AddAttribute(7, "disabled", true); // Disabled in editor mode
                builder.CloseElement();
                break;

            case DropdownElement dropdown:
                builder.OpenElement(0, "select");
                builder.AddAttribute(1, "required", dropdown.IsRequired);
                builder.AddAttribute(2, "multiple", dropdown.AllowMultiple);
                builder.AddAttribute(3, "style", style);
                builder.AddAttribute(4, "disabled", true); // Disabled in editor mode

                // Placeholder option
                if (!string.IsNullOrEmpty(dropdown.Placeholder))
                {
                    builder.OpenElement(5, "option");
                    builder.AddAttribute(6, "value", "");
                    builder.AddAttribute(7, "disabled", true);
                    builder.AddAttribute(8, "selected", string.IsNullOrEmpty(dropdown.DefaultValue));
                    builder.AddContent(9, dropdown.Placeholder);
                    builder.CloseElement();
                }

                // Options
                foreach (var option in dropdown.Options)
                {
                    builder.OpenElement(10, "option");
                    builder.AddAttribute(11, "value", option);
                    builder.AddAttribute(12, "selected", option == dropdown.DefaultValue);
                    builder.AddContent(13, option);
                    builder.CloseElement();
                }

                builder.CloseElement();
                break;

            case DatePickerElement datePicker:
                builder.OpenElement(0, "input");
                builder.AddAttribute(1, "type", datePicker.IncludeTime ? "datetime-local" : "date");
                builder.AddAttribute(2, "placeholder", datePicker.Placeholder);
                builder.AddAttribute(3, "value", datePicker.DefaultValue?.ToString(datePicker.DateFormat));
                builder.AddAttribute(4, "required", datePicker.IsRequired);
                if (datePicker.MinDate.HasValue)
                    builder.AddAttribute(5, "min", datePicker.MinDate.Value.ToString("yyyy-MM-dd"));
                if (datePicker.MaxDate.HasValue)
                    builder.AddAttribute(6, "max", datePicker.MaxDate.Value.ToString("yyyy-MM-dd"));
                builder.AddAttribute(7, "style", style);
                builder.AddAttribute(8, "disabled", true); // Disabled in editor mode
                builder.CloseElement();
                break;

            case DividerElement divider:
                builder.OpenElement(0, "hr");
                var borderStyle = divider.Style.ToLower() switch
                {
                    "dashed" => "dashed",
                    "dotted" => "dotted",
                    _ => "solid"
                };
                var dividerStyle = $"width: 100%; height: 0; border: none; border-top: {divider.Thickness}px {borderStyle} {divider.Color}; margin: 0; padding: 0;";
                builder.AddAttribute(1, "style", dividerStyle);
                builder.CloseElement();
                break;

            case RadioGroupElement radioGroup:
                builder.OpenElement(0, "div");
                var radioGroupStyle = style + $" display: flex; flex-direction: {(radioGroup.Layout == "Horizontal" ? "row" : "column")}; gap: 12px; align-items: flex-start;";
                builder.AddAttribute(1, "style", radioGroupStyle);

                // Render label if present
                if (!string.IsNullOrEmpty(radioGroup.Label))
                {
                    builder.OpenElement(2, "div");
                    builder.AddAttribute(3, "style", "font-weight: 600; margin-bottom: 8px;");
                    builder.AddContent(4, radioGroup.Label);
                    builder.CloseElement();
                }

                // Render radio buttons
                int sequence = 10;
                foreach (var option in radioGroup.Options)
                {
                    builder.OpenElement(sequence++, "label");
                    builder.AddAttribute(sequence++, "style", "display: flex; align-items: center; gap: 8px; cursor: pointer;");

                    builder.OpenElement(sequence++, "input");
                    builder.AddAttribute(sequence++, "type", "radio");
                    builder.AddAttribute(sequence++, "name", $"radio_{radioGroup.Id}");
                    builder.AddAttribute(sequence++, "value", option);
                    builder.AddAttribute(sequence++, "checked", option == radioGroup.SelectedValue);
                    builder.AddAttribute(sequence++, "disabled", true); // Disabled in editor mode
                    builder.AddAttribute(sequence++, "style", "width: 20px; height: 20px;");
                    builder.CloseElement();

                    builder.OpenElement(sequence++, "span");
                    builder.AddContent(sequence++, option);
                    builder.CloseElement();

                    builder.CloseElement(); // label
                }

                builder.CloseElement(); // div
                break;

            case FileUploadElement fileUpload:
                builder.OpenElement(0, "div");
                builder.AddAttribute(1, "style", style + " display: flex; flex-direction: column; gap: 8px;");

                // Label if present
                if (!string.IsNullOrEmpty(fileUpload.Label))
                {
                    builder.OpenElement(2, "label");
                    builder.AddAttribute(3, "style", "font-weight: 600;");
                    builder.AddContent(4, fileUpload.Label);
                    if (fileUpload.IsRequired)
                    {
                        builder.OpenElement(5, "span");
                        builder.AddAttribute(6, "style", "color: red; margin-left: 4px;");
                        builder.AddContent(7, "*");
                        builder.CloseElement();
                    }
                    builder.CloseElement();
                }

                // File input button
                builder.OpenElement(8, "div");
                builder.AddAttribute(9, "style", "display: flex; align-items: center; gap: 12px; padding: 12px; border: 2px dashed #ccc; border-radius: 4px; background-color: #f9f9f9;");

                builder.OpenElement(10, "input");
                builder.AddAttribute(11, "type", "file");
                builder.AddAttribute(12, "multiple", fileUpload.Multiple);
                builder.AddAttribute(13, "disabled", true); // Disabled in editor mode
                builder.AddAttribute(14, "style", "display: none;");
                builder.AddAttribute(15, "id", $"file_{fileUpload.Id}");
                builder.CloseElement();

                // Custom button
                builder.OpenElement(16, "button");
                builder.AddAttribute(17, "type", "button");
                builder.AddAttribute(18, "disabled", true);
                builder.AddAttribute(19, "style", "padding: 8px 16px; background-color: #1976d2; color: white; border: none; border-radius: 4px; cursor: not-allowed;");
                builder.AddContent(20, fileUpload.ButtonText);
                builder.CloseElement();

                // Helper text
                builder.OpenElement(21, "span");
                builder.AddAttribute(22, "style", "color: #666; font-size: 14px;");
                builder.AddContent(23, fileUpload.HelperText);
                builder.CloseElement();

                builder.CloseElement(); // div (upload area)

                // File restrictions text
                if (fileUpload.AllowedExtensions.Count > 0)
                {
                    builder.OpenElement(24, "div");
                    builder.AddAttribute(25, "style", "font-size: 12px; color: #999;");
                    builder.AddContent(26, $"Allowed: {string.Join(", ", fileUpload.AllowedExtensions)} | Max size: {fileUpload.MaxFileSize}MB");
                    builder.CloseElement();
                }

                builder.CloseElement(); // div (container)
                break;

            case SignatureElement signature:
                builder.OpenElement(0, "div");
                builder.AddAttribute(1, "style", style + " display: flex; flex-direction: column; gap: 8px;");

                // Label if present
                if (!string.IsNullOrEmpty(signature.Label))
                {
                    builder.OpenElement(2, "label");
                    builder.AddAttribute(3, "style", "font-weight: 600;");
                    builder.AddContent(4, signature.Label);
                    if (signature.IsRequired)
                    {
                        builder.OpenElement(5, "span");
                        builder.AddAttribute(6, "style", "color: red; margin-left: 4px;");
                        builder.AddContent(7, "*");
                        builder.CloseElement();
                    }
                    builder.CloseElement();
                }

                // Signature canvas or saved signature
                builder.OpenElement(8, "div");
                builder.AddAttribute(9, "style", $"position: relative; border: 2px solid {signature.BorderColor}; border-radius: 4px; background-color: {signature.BackgroundColor}; display: flex; align-items: center; justify-content: center;");

                if (!string.IsNullOrEmpty(signature.SignatureData))
                {
                    // Show saved signature
                    builder.OpenElement(10, "img");
                    builder.AddAttribute(11, "src", signature.SignatureData);
                    builder.AddAttribute(12, "alt", "Signature");
                    builder.AddAttribute(13, "style", "max-width: 100%; max-height: 100%; object-fit: contain;");
                    builder.CloseElement();
                }
                else
                {
                    // Show placeholder in editor mode
                    builder.OpenElement(10, "div");
                    builder.AddAttribute(11, "style", "color: #999; text-align: center; padding: 20px;");
                    builder.AddContent(12, "✍️ Signature Pad (Editor Mode)");
                    builder.CloseElement();
                }

                builder.CloseElement(); // div (signature area)

                // Instructions
                builder.OpenElement(13, "div");
                builder.AddAttribute(14, "style", "font-size: 12px; color: #999;");
                builder.AddContent(15, "Sign above the line");
                builder.CloseElement();

                builder.CloseElement(); // div (container)
                break;

            case TableElement table:
                builder.OpenElement(0, "div");
                builder.AddAttribute(1, "style", style + " overflow: auto;");

                // Label if present
                if (!string.IsNullOrEmpty(table.Label))
                {
                    builder.OpenElement(2, "label");
                    builder.AddAttribute(3, "style", "font-weight: 600; display: block; margin-bottom: 8px;");
                    builder.AddContent(4, table.Label);
                    if (table.IsRequired)
                    {
                        builder.OpenElement(5, "span");
                        builder.AddAttribute(6, "style", "color: red; margin-left: 4px;");
                        builder.AddContent(7, "*");
                        builder.CloseElement();
                    }
                    builder.CloseElement();
                }

                // Table
                builder.OpenElement(8, "table");
                builder.AddAttribute(9, "style", $"width: 100%; border-collapse: collapse; border: 1px solid {table.BorderColor};");

                // Headers
                if (table.ShowHeaders)
                {
                    builder.OpenElement(10, "thead");
                    builder.OpenElement(11, "tr");
                    int headerSeq = 12;
                    for (int col = 0; col < table.Columns; col++)
                    {
                        builder.OpenElement(headerSeq++, "th");
                        builder.AddAttribute(headerSeq++, "style", $"border: 1px solid {table.BorderColor}; padding: 8px; background-color: #f5f5f5; text-align: left; font-weight: 600;");
                        builder.AddContent(headerSeq++, table.Headers.Count > col ? table.Headers[col] : $"Column {col + 1}");
                        builder.CloseElement();
                    }
                    builder.CloseElement(); // tr
                    builder.CloseElement(); // thead
                }

                // Rows
                builder.OpenElement(100, "tbody");
                int rowSeq = 101;
                for (int row = 0; row < table.Rows && row < table.CellData.Count; row++)
                {
                    builder.OpenElement(rowSeq++, "tr");
                    for (int col = 0; col < table.Columns && col < table.CellData[row].Count; col++)
                    {
                        builder.OpenElement(rowSeq++, "td");
                        builder.AddAttribute(rowSeq++, "style", $"border: 1px solid {table.BorderColor}; padding: 8px;");
                        builder.AddContent(rowSeq++, table.CellData[row][col]);
                        builder.CloseElement();
                    }
                    builder.CloseElement(); // tr
                }
                builder.CloseElement(); // tbody

                builder.CloseElement(); // table
                builder.CloseElement(); // div (container)
                break;

            default:
                builder.OpenElement(0, "div");
                builder.AddAttribute(1, "style", style);
                builder.AddContent(2, $"Unknown element type: {element.Type}");
                builder.CloseElement();
                break;
        }
    };

    // Resize functionality
    private async Task HandleResizeStart(MouseEventArgs e, FormElement element, string handleType)
    {
        isResizing = true;
        resizedElement = element;

        // Initialize resize dimensions
        resizeWidth = element.Width;
        resizeHeight = element.Height;
        resizeX = element.X;
        resizeY = element.Y;

        // Announce resize start to screen readers
        await AccessibilityService.AnnounceResizeStartAsync(element.GetDisplayName());

        // Get canvas-relative coordinates
        var canvasRect = await JSRuntime.InvokeAsync<object>("eval",
            $"document.querySelector('[data-canvas-id=\"main-canvas\"]').getBoundingClientRect()");

        // Notify JavaScript to start tracking resize
        await JSRuntime.InvokeVoidAsync("resizeHandler.startResize",
            element.Id,
            handleType,
            e.ClientX,
            e.ClientY,
            element.Width,
            element.Height,
            element.X,
            element.Y);

        StateHasChanged();
    }

    [JSInvokable]
    public void OnResize(double width, double height, double left, double top)
    {
        if (isResizing && resizedElement != null)
        {
            resizeWidth = (int)width;
            resizeHeight = (int)height;
            resizeX = (int)left;
            resizeY = (int)top;

            StateHasChanged();
        }
    }

    [JSInvokable]
    public async Task OnResizeEnd(double width, double height, double left, double top)
    {
        if (isResizing && resizedElement != null)
        {
            // Apply final dimensions
            resizedElement.Width = (int)width;
            resizedElement.Height = (int)height;
            resizedElement.X = (int)left;
            resizedElement.Y = (int)top;

            CurrentTemplate.MarkAsUpdated();

            // Announce resize end to screen readers
            await AccessibilityService.AnnounceResizeEndAsync(resizedElement.GetDisplayName(), resizedElement.Width, resizedElement.Height);

            // Notify parent component
            await OnElementDropped.InvokeAsync(resizedElement);

            // Reset resize state
            isResizing = false;
            resizedElement = null;

            StateHasChanged();
        }
    }

    // Rotation functionality
    private async Task HandleRotateStart(MouseEventArgs e, FormElement element)
    {
        isRotating = true;
        rotatedElement = element;
        initialRotation = element.Properties.Rotation;

        // Calculate center point of element in viewport coordinates
        int centerX = element.X + element.Width / 2;
        int centerY = element.Y + element.Height / 2;

        // Get canvas position
        var canvasBounds = await JSRuntime.InvokeAsync<object>("eval",
            "(() => { const canvas = document.querySelector('[data-canvas-id=\"main-canvas\"]'); const rect = canvas.getBoundingClientRect(); return { left: rect.left, top: rect.top }; })()");

        var boundsDict = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, double>>(
            System.Text.Json.JsonSerializer.Serialize(canvasBounds));

        double canvasLeft = boundsDict?["left"] ?? 0;
        double canvasTop = boundsDict?["top"] ?? 0;

        // Calculate center in viewport coordinates
        double viewportCenterX = canvasLeft + (centerX * zoomLevel);
        double viewportCenterY = canvasTop + (centerY * zoomLevel);

        // Start rotation tracking
        initialAngle = await JSRuntime.InvokeAsync<double>("rotationHandler.startRotate",
            element.Id.ToString(),
            viewportCenterX,
            viewportCenterY,
            e.ClientX,
            e.ClientY);

        StateHasChanged();
    }

    [JSInvokable]
    public void OnRotateMove(double angle)
    {
        if (isRotating && rotatedElement != null)
        {
            // Calculate rotation delta
            double angleDelta = angle - initialAngle;
            double newRotation = initialRotation + angleDelta;

            // Normalize to -180 to 180
            while (newRotation > 180) newRotation -= 360;
            while (newRotation < -180) newRotation += 360;

            rotatedElement.Properties.Rotation = (int)Math.Round(newRotation);

            StateHasChanged();
        }
    }

    [JSInvokable]
    public async Task OnRotateEnd(double angle)
    {
        if (isRotating && rotatedElement != null)
        {
            // Calculate final rotation
            double angleDelta = angle - initialAngle;
            double newRotation = initialRotation + angleDelta;

            // Normalize to -180 to 180
            while (newRotation > 180) newRotation -= 360;
            while (newRotation < -180) newRotation += 360;

            rotatedElement.Properties.Rotation = (int)Math.Round(newRotation);

            CurrentTemplate.MarkAsUpdated();

            // Notify parent component
            await OnElementDropped.InvokeAsync(rotatedElement);

            // Reset rotation state
            isRotating = false;
            rotatedElement = null;

            StateHasChanged();
        }
    }

    // Zoom functionality
    private double zoomLevel = 1.0; // 100%
    private const double MIN_ZOOM = 0.5; // 50%
    private const double MAX_ZOOM = 2.0; // 200%
    private const double ZOOM_STEP = 0.1; // 10% per step

    public void ZoomIn()
    {
        if (zoomLevel < MAX_ZOOM)
        {
            zoomLevel = Math.Min(zoomLevel + ZOOM_STEP, MAX_ZOOM);
            StateHasChanged();
        }
    }

    public void ZoomOut()
    {
        if (zoomLevel > MIN_ZOOM)
        {
            zoomLevel = Math.Max(zoomLevel - ZOOM_STEP, MIN_ZOOM);
            StateHasChanged();
        }
    }

    public void ResetZoom()
    {
        zoomLevel = 1.0;
        StateHasChanged();
    }

    public double GetZoomLevel() => zoomLevel;

    public int GetZoomPercentage() => (int)(zoomLevel * 100);

    // ARIA accessibility helpers
    private string GetElementAriaRole(FormElement element)
    {
        return element switch
        {
            TextInputElement => "textbox",
            LabelElement => "heading",
            CheckboxElement => "checkbox",
            TextAreaElement => "textbox",
            DropdownElement => "combobox",
            RadioGroupElement => "radiogroup",
            DatePickerElement => "textbox",
            FileUploadElement => "group",
            SignatureElement => "group",
            ImageElement => "img",
            DividerElement => "separator",
            TableElement => "table",
            _ => "group"
        };
    }

    private string GetElementAriaLabel(FormElement element)
    {
        var baseLabel = element switch
        {
            TextInputElement textInput => !string.IsNullOrEmpty(textInput.Label) ? textInput.Label : !string.IsNullOrEmpty(textInput.Placeholder) ? $"Text input: {textInput.Placeholder}" : "Text input",
            LabelElement label => $"Heading: {label.Text}",
            CheckboxElement checkbox => !string.IsNullOrEmpty(checkbox.Label) ? checkbox.Label : "Checkbox",
            TextAreaElement textArea => !string.IsNullOrEmpty(textArea.Label) ? textArea.Label : !string.IsNullOrEmpty(textArea.Placeholder) ? $"Text area: {textArea.Placeholder}" : "Text area",
            DropdownElement dropdown => !string.IsNullOrEmpty(dropdown.Label) ? dropdown.Label : "Dropdown",
            RadioGroupElement radioGroup => !string.IsNullOrEmpty(radioGroup.Label) ? radioGroup.Label : "Radio group",
            DatePickerElement datePicker => !string.IsNullOrEmpty(datePicker.Label) ? datePicker.Label : "Date picker",
            FileUploadElement fileUpload => !string.IsNullOrEmpty(fileUpload.Label) ? fileUpload.Label : "File upload",
            SignatureElement signature => !string.IsNullOrEmpty(signature.Label) ? signature.Label : "Signature pad",
            ImageElement image => !string.IsNullOrEmpty(image.AltText) ? image.AltText : "Image",
            DividerElement => "Divider",
            TableElement table => !string.IsNullOrEmpty(table.Label) ? table.Label : "Table",
            _ => $"Form element: {element.Type}"
        };

        var positionInfo = $" at position {element.X}, {element.Y}";
        var requiredInfo = element.IsRequired ? " (required)" : "";

        return $"{baseLabel}{requiredInfo}{positionInfo}";
    }

    // Smart spacing guides - show equal spacing between elements
    private void CalculateSmartGuides(int x, int y, FormElement movingElement)
    {
        spacingGuides.Clear();

        var otherElements = CurrentTemplate.Elements
            .Where(e => e.Id != movingElement.Id)
            .ToArray();

        if (otherElements.Length < 2)
        {
            return; // Need at least 2 other elements to calculate equal spacing
        }

        // Check horizontal spacing (left to right)
        var horizontallySorted = otherElements
            .OrderBy(e => e.X)
            .ToList();

        for (int i = 0; i < horizontallySorted.Count - 1; i++)
        {
            var left = horizontallySorted[i];
            var right = horizontallySorted[i + 1];

            // Calculate spacing between these two elements
            int spacing = right.X - (left.X + left.Width);

            // Check if moving element would create equal spacing
            // Case 1: Moving element fits between left and right
            if (x > left.X + left.Width && x + movingElement.Width < right.X)
            {
                int leftSpacing = x - (left.X + left.Width);
                int rightSpacing = right.X - (x + movingElement.Width);

                if (Math.Abs(leftSpacing - rightSpacing) < SPACING_SNAP_THRESHOLD)
                {
                    // Snap to equal spacing
                    int equalSpacing = (right.X - (left.X + left.Width) - movingElement.Width) / 2;
                    int snappedX = left.X + left.Width + equalSpacing;

                    if (Math.Abs(x - snappedX) < SPACING_SNAP_THRESHOLD)
                    {
                        dragX = snappedX;
                        // Add guide lines showing the equal spacing
                        spacingGuides.Add((left.X + left.Width + equalSpacing / 2, true));
                        spacingGuides.Add((x + movingElement.Width + equalSpacing / 2, true));
                    }
                }
            }
        }

        // Check vertical spacing (top to bottom)
        var verticallySorted = otherElements
            .OrderBy(e => e.Y)
            .ToList();

        for (int i = 0; i < verticallySorted.Count - 1; i++)
        {
            var top = verticallySorted[i];
            var bottom = verticallySorted[i + 1];

            // Calculate spacing between these two elements
            int spacing = bottom.Y - (top.Y + top.Height);

            // Check if moving element would create equal spacing
            // Case 1: Moving element fits between top and bottom
            if (y > top.Y + top.Height && y + movingElement.Height < bottom.Y)
            {
                int topSpacing = y - (top.Y + top.Height);
                int bottomSpacing = bottom.Y - (y + movingElement.Height);

                if (Math.Abs(topSpacing - bottomSpacing) < SPACING_SNAP_THRESHOLD)
                {
                    // Snap to equal spacing
                    int equalSpacing = (bottom.Y - (top.Y + top.Height) - movingElement.Height) / 2;
                    int snappedY = top.Y + top.Height + equalSpacing;

                    if (Math.Abs(y - snappedY) < SPACING_SNAP_THRESHOLD)
                    {
                        dragY = snappedY;
                        // Add guide lines showing the equal spacing
                        spacingGuides.Add((top.Y + top.Height + equalSpacing / 2, false));
                        spacingGuides.Add((y + movingElement.Height + equalSpacing / 2, false));
                    }
                }
            }
        }
    }
}
