using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using FormMaker.Shared.Models;
using FormMaker.Shared.Models.Elements;
using FormMaker.Shared.Enums;

namespace FormMaker.Client.Components;

public partial class PreviewMode
{
    private RenderFragment RenderPreviewElement(FormElement element) => builder =>
    {
        var style = $"width: 100%; height: 100%; " +
                    $"font-size: {element.Properties.FontSize}px; " +
                    $"font-family: {element.Properties.FontFamily}; " +
                    $"color: {element.Properties.Color}; " +
                    $"background-color: {element.Properties.BackgroundColor}; " +
                    $"border: {element.Properties.BorderWidth}px {element.Properties.BorderStyle} {element.Properties.BorderColor}; " +
                    $"border-radius: {element.Properties.BorderRadius}px; " +
                    $"padding: {element.Properties.GetPaddingCss()}; " +
                    $"text-align: {element.Properties.Alignment.ToString().ToLower()}; " +
                    $"font-weight: {(element.Properties.Bold ? "bold" : "normal")}; " +
                    $"font-style: {(element.Properties.Italic ? "italic" : "normal")}; " +
                    $"text-decoration: {(element.Properties.Underline ? "underline" : "none")}; " +
                    $"opacity: {element.Properties.Opacity}; " +
                    $"transform: rotate({element.Properties.Rotation}deg);";

        switch (element)
        {
            case TextInputElement textInput:
                builder.OpenElement(0, "input");
                builder.AddAttribute(1, "type", textInput.InputType);
                builder.AddAttribute(2, "placeholder", textInput.Placeholder);
                builder.AddAttribute(3, "value", GetFieldValue(textInput.Id));
                builder.AddAttribute(4, "required", textInput.IsRequired);
                builder.AddAttribute(5, "style", style);
                builder.AddAttribute(6, "oninput", EventCallback.Factory.Create<ChangeEventArgs>(this, e => SetFieldValue(textInput.Id, e.Value?.ToString() ?? "")));
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
                builder.AddAttribute(1, "style", style + " display: flex; align-items: center; gap: 8px; cursor: pointer;");

                builder.OpenElement(2, "input");
                builder.AddAttribute(3, "type", "checkbox");
                builder.AddAttribute(4, "checked", GetFieldValue(checkbox.Id) == "true");
                builder.AddAttribute(5, "onchange", EventCallback.Factory.Create<ChangeEventArgs>(this, e => SetFieldValue(checkbox.Id, e.Value?.ToString() ?? "false")));
                builder.AddAttribute(6, "style", "width: 24px; height: 24px; cursor: pointer;");
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
                    builder.AddContent(4, "📷 Image");
                    builder.CloseElement();
                }

                builder.CloseElement();
                break;

            case TextAreaElement textArea:
                builder.OpenElement(0, "textarea");
                builder.AddAttribute(1, "placeholder", textArea.Placeholder);
                builder.AddAttribute(2, "rows", textArea.Rows);
                builder.AddAttribute(3, "value", GetFieldValue(textArea.Id));
                builder.AddAttribute(4, "required", textArea.IsRequired);
                builder.AddAttribute(5, "maxlength", textArea.MaxLength);
                builder.AddAttribute(6, "style", style + $" resize: {(textArea.Resizable ? "both" : "none")}; white-space: {(textArea.WrapText ? "pre-wrap" : "pre")};");
                builder.AddAttribute(7, "oninput", EventCallback.Factory.Create<ChangeEventArgs>(this, e => SetFieldValue(textArea.Id, e.Value?.ToString() ?? "")));
                builder.CloseElement();
                break;

            case DropdownElement dropdown:
                builder.OpenElement(0, "select");
                builder.AddAttribute(1, "required", dropdown.IsRequired);
                builder.AddAttribute(2, "multiple", dropdown.AllowMultiple);
                builder.AddAttribute(3, "style", style);
                builder.AddAttribute(4, "value", GetFieldValue(dropdown.Id));
                builder.AddAttribute(5, "onchange", EventCallback.Factory.Create<ChangeEventArgs>(this, e => SetFieldValue(dropdown.Id, e.Value?.ToString() ?? "")));

                // Placeholder option
                if (!string.IsNullOrEmpty(dropdown.Placeholder))
                {
                    builder.OpenElement(6, "option");
                    builder.AddAttribute(7, "value", "");
                    builder.AddAttribute(8, "disabled", true);
                    builder.AddAttribute(9, "selected", string.IsNullOrEmpty(GetFieldValue(dropdown.Id)));
                    builder.AddContent(10, dropdown.Placeholder);
                    builder.CloseElement();
                }

                // Options
                int seq = 20;
                foreach (var option in dropdown.Options)
                {
                    builder.OpenElement(seq++, "option");
                    builder.AddAttribute(seq++, "value", option);
                    builder.AddAttribute(seq++, "selected", option == GetFieldValue(dropdown.Id));
                    builder.AddContent(seq++, option);
                    builder.CloseElement();
                }

                builder.CloseElement();
                break;

            case DatePickerElement datePicker:
                builder.OpenElement(0, "input");
                builder.AddAttribute(1, "type", datePicker.IncludeTime ? "datetime-local" : "date");
                builder.AddAttribute(2, "placeholder", datePicker.Placeholder);
                builder.AddAttribute(3, "value", GetFieldValue(datePicker.Id));
                builder.AddAttribute(4, "required", datePicker.IsRequired);
                if (datePicker.MinDate.HasValue)
                    builder.AddAttribute(5, "min", datePicker.MinDate.Value.ToString("yyyy-MM-dd"));
                if (datePicker.MaxDate.HasValue)
                    builder.AddAttribute(6, "max", datePicker.MaxDate.Value.ToString("yyyy-MM-dd"));
                builder.AddAttribute(7, "style", style);
                builder.AddAttribute(8, "onchange", EventCallback.Factory.Create<ChangeEventArgs>(this, e => SetFieldValue(datePicker.Id, e.Value?.ToString() ?? "")));
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
                    builder.AddAttribute(sequence++, "checked", option == GetFieldValue(radioGroup.Id));
                    builder.AddAttribute(sequence++, "onchange", EventCallback.Factory.Create<ChangeEventArgs>(this, e => SetFieldValue(radioGroup.Id, option)));
                    builder.AddAttribute(sequence++, "style", "width: 20px; height: 20px; cursor: pointer;");
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

                // File input
                builder.OpenElement(8, "input");
                builder.AddAttribute(9, "type", "file");
                builder.AddAttribute(10, "multiple", fileUpload.Multiple);
                builder.AddAttribute(11, "accept", string.Join(",", fileUpload.AllowedExtensions));
                builder.AddAttribute(12, "style", "padding: 12px; border: 2px dashed #ccc; border-radius: 4px; background-color: #f9f9f9; cursor: pointer;");
                builder.CloseElement();

                // Helper text
                if (!string.IsNullOrEmpty(fileUpload.HelperText))
                {
                    builder.OpenElement(13, "div");
                    builder.AddAttribute(14, "style", "font-size: 12px; color: #666;");
                    builder.AddContent(15, fileUpload.HelperText);
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

                // Signature placeholder
                builder.OpenElement(8, "div");
                builder.AddAttribute(9, "style", $"border: 2px solid {signature.BorderColor}; border-radius: 4px; background-color: {signature.BackgroundColor}; min-height: 100px; display: flex; align-items: center; justify-content: center; color: #999;");
                builder.AddContent(10, "✍️ Signature Area");
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
                    builder.CloseElement();
                }

                // Table
                builder.OpenElement(5, "table");
                builder.AddAttribute(6, "style", $"width: 100%; border-collapse: collapse; border: 1px solid {table.BorderColor};");

                // Headers
                if (table.ShowHeaders)
                {
                    builder.OpenElement(7, "thead");
                    builder.OpenElement(8, "tr");
                    int headerSeq = 9;
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
}
