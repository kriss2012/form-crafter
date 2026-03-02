using Microsoft.JSInterop;
using FormMaker.Shared.Models;
using System.Text.Json;

namespace FormMaker.Client.Services;

public class PdfExportService
{
    private readonly IJSRuntime _jsRuntime;

    public PdfExportService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    /// <summary>
    /// Export blank form template as PDF
    /// </summary>
    /// <param name="template">The form template to export</param>
    /// <returns>True if export was successful</returns>
    public async Task<bool> ExportBlankFormAsync(FormTemplate template)
    {
        try
        {
            // Create export data with all necessary properties
            // Include WidthInPixels and HeightInPixels explicitly since they're JsonIgnored
            var exportData = new
            {
                name = template.Name,
                widthInPixels = template.WidthInPixels,
                heightInPixels = template.HeightInPixels,
                isMultiPage = template.IsMultiPage,
                pages = template.Pages,
                elements = template.Elements,
                backgroundColor = template.BackgroundColor
            };

            // Call JavaScript function - JSInterop will handle serialization with camelCase
            var result = await _jsRuntime.InvokeAsync<bool>("pdfExport.exportToPdf",
                exportData,
                false,
                null);

            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error exporting blank form to PDF: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Export filled form with data as PDF
    /// </summary>
    /// <param name="template">The form template to export</param>
    /// <param name="formData">Dictionary of form field values keyed by element ID</param>
    /// <returns>True if export was successful</returns>
    public async Task<bool> ExportFilledFormAsync(FormTemplate template, Dictionary<string, object> formData)
    {
        try
        {
            // Create export data with all necessary properties
            // Include WidthInPixels and HeightInPixels explicitly since they're JsonIgnored
            var exportData = new
            {
                name = template.Name,
                widthInPixels = template.WidthInPixels,
                heightInPixels = template.HeightInPixels,
                isMultiPage = template.IsMultiPage,
                pages = template.Pages,
                elements = template.Elements,
                backgroundColor = template.BackgroundColor
            };

            // Call JavaScript function - JSInterop will handle serialization with camelCase
            var result = await _jsRuntime.InvokeAsync<bool>("pdfExport.exportToPdf",
                exportData,
                true,
                formData);

            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error exporting filled form to PDF: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Export using browser's native print dialog (alternative method)
    /// </summary>
    /// <returns>True if print dialog was opened successfully</returns>
    public async Task<bool> PrintFormAsync()
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("window.print");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error opening print dialog: {ex.Message}");
            return false;
        }
    }
}
