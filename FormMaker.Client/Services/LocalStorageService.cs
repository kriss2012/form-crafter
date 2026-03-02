using Microsoft.JSInterop;
using System.Text.Json;
using FormMaker.Shared.Models;

namespace FormMaker.Client.Services;

/// <summary>
/// Service for saving and loading forms to/from browser localStorage
/// </summary>
public class LocalStorageService
{
    private readonly IJSRuntime _jsRuntime;
    private const string STORAGE_KEY_PREFIX = "formmaker_form_";
    private const string FORMS_LIST_KEY = "formmaker_forms_list";

    public LocalStorageService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    /// <summary>
    /// Save a form template to localStorage
    /// </summary>
    public async Task SaveFormAsync(FormTemplate form)
    {
        if (form == null) throw new ArgumentNullException(nameof(form));

        // Update timestamp
        form.UpdatedAt = DateTime.UtcNow;

        // Serialize to JSON
        var json = JsonSerializer.Serialize(form, new JsonSerializerOptions
        {
            WriteIndented = false
        });

        // Save to localStorage
        var key = STORAGE_KEY_PREFIX + form.Id.ToString();
        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", key, json);

        // Update the forms list
        await UpdateFormsListAsync(form);
    }

    /// <summary>
    /// Load a form template from localStorage
    /// </summary>
    public async Task<FormTemplate?> LoadFormAsync(Guid formId)
    {
        var key = STORAGE_KEY_PREFIX + formId.ToString();

        try
        {
            var json = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", key);

            if (string.IsNullOrEmpty(json))
                return null;

            var form = JsonSerializer.Deserialize<FormTemplate>(json);
            return form;
        }
        catch (Exception)
        {
            return null;
        }
    }

    /// <summary>
    /// Get all saved forms (metadata only, not full form data)
    /// </summary>
    public async Task<List<FormMetadata>> GetAllFormsAsync()
    {
        try
        {
            var json = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", FORMS_LIST_KEY);

            if (string.IsNullOrEmpty(json))
                return new List<FormMetadata>();

            var forms = JsonSerializer.Deserialize<List<FormMetadata>>(json);
            return forms ?? new List<FormMetadata>();
        }
        catch (Exception)
        {
            return new List<FormMetadata>();
        }
    }

    /// <summary>
    /// Delete a form from localStorage
    /// </summary>
    public async Task DeleteFormAsync(Guid formId)
    {
        var key = STORAGE_KEY_PREFIX + formId.ToString();
        await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", key);

        // Update the forms list
        await RemoveFromFormsListAsync(formId);
    }

    /// <summary>
    /// Check if a form exists in localStorage
    /// </summary>
    public async Task<bool> FormExistsAsync(Guid formId)
    {
        var key = STORAGE_KEY_PREFIX + formId.ToString();
        var json = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", key);
        return !string.IsNullOrEmpty(json);
    }

    /// <summary>
    /// Clear all forms from localStorage (use with caution!)
    /// </summary>
    public async Task ClearAllFormsAsync()
    {
        var forms = await GetAllFormsAsync();

        foreach (var form in forms)
        {
            await DeleteFormAsync(form.Id);
        }

        await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", FORMS_LIST_KEY);
    }

    /// <summary>
    /// Update the list of saved forms (for quick retrieval without loading full data)
    /// </summary>
    private async Task UpdateFormsListAsync(FormTemplate form)
    {
        var forms = await GetAllFormsAsync();

        // Remove existing entry if present
        forms.RemoveAll(f => f.Id == form.Id);

        // Add updated metadata
        forms.Add(new FormMetadata
        {
            Id = form.Id,
            Name = form.Name,
            Description = form.Description,
            ElementCount = form.Elements.Count,
            PageSize = form.PageSize,
            CreatedAt = form.CreatedAt,
            UpdatedAt = form.UpdatedAt,
            Category = form.Category,
            Tags = form.Tags
        });

        // Sort by updated date (most recent first)
        forms = forms.OrderByDescending(f => f.UpdatedAt).ToList();

        // Save updated list
        var json = JsonSerializer.Serialize(forms);
        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", FORMS_LIST_KEY, json);
    }

    /// <summary>
    /// Remove a form from the forms list
    /// </summary>
    private async Task RemoveFromFormsListAsync(Guid formId)
    {
        var forms = await GetAllFormsAsync();
        forms.RemoveAll(f => f.Id == formId);

        var json = JsonSerializer.Serialize(forms);
        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", FORMS_LIST_KEY, json);
    }

    /// <summary>
    /// Generic method to get an item from localStorage
    /// </summary>
    public virtual async Task<T?> GetItemAsync<T>(string key)
    {
        var json = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", key);
        if (string.IsNullOrEmpty(json))
            return default;

        return JsonSerializer.Deserialize<T>(json);
    }

    /// <summary>
    /// Generic method to set an item in localStorage
    /// </summary>
    public virtual async Task SetItemAsync<T>(string key, T value)
    {
        var json = JsonSerializer.Serialize(value);
        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", key, json);
    }

    /// <summary>
    /// Generic method to remove an item from localStorage
    /// </summary>
    public virtual async Task RemoveItemAsync(string key)
    {
        await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", key);
    }
}

/// <summary>
/// Lightweight metadata for form list view (doesn't include full element data)
/// </summary>
public class FormMetadata
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "Untitled Form";
    public string? Description { get; set; }
    public int ElementCount { get; set; }
    public FormMaker.Shared.Enums.PageSize PageSize { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string? Category { get; set; }
    public List<string> Tags { get; set; } = new();
}
