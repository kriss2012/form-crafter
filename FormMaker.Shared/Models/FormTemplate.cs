using FormMaker.Shared.Enums;
using System.Text.Json.Serialization;

namespace FormMaker.Shared.Models;

/// <summary>
/// A complete form template containing all elements and settings
/// </summary>
public class FormTemplate
{
    /// <summary>
    /// Unique identifier for the template
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Name of the form template
    /// </summary>
    public string Name { get; set; } = "Untitled Form";

    /// <summary>
    /// Optional description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// All elements in the form (Legacy - for backward compatibility)
    /// Use Pages for multi-page forms
    /// </summary>
    public List<FormElement> Elements { get; set; } = new();

    /// <summary>
    /// Pages in the form (for multi-page support)
    /// </summary>
    public List<FormPage> Pages { get; set; } = new();

    /// <summary>
    /// Index of the currently active page in the editor (0-based)
    /// </summary>
    [JsonIgnore]
    public int CurrentPageIndex { get; set; } = 0;

    /// <summary>
    /// Whether this form uses multi-page mode
    /// </summary>
    public bool IsMultiPage { get; set; } = false;

    /// <summary>
    /// Page size for the form
    /// </summary>
    public PageSize PageSize { get; set; } = PageSize.Letter;

    /// <summary>
    /// Custom width if PageSize is Custom (in pixels)
    /// </summary>
    public int? CustomWidth { get; set; }

    /// <summary>
    /// Custom height if PageSize is Custom (in pixels)
    /// </summary>
    public int? CustomHeight { get; set; }

    /// <summary>
    /// Margin sizes (in pixels)
    /// </summary>
    public int MarginTop { get; set; } = 48; // 0.5 inch
    public int MarginRight { get; set; } = 48;
    public int MarginBottom { get; set; } = 48;
    public int MarginLeft { get; set; } = 48;

    /// <summary>
    /// Background color of the form
    /// </summary>
    public string BackgroundColor { get; set; } = "#ffffff";

    /// <summary>
    /// Category for organization
    /// </summary>
    public string? Category { get; set; }

    /// <summary>
    /// Tags for searchability
    /// </summary>
    public List<string> Tags { get; set; } = new();

    /// <summary>
    /// When the template was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When the template was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Version number for tracking changes
    /// </summary>
    public int Version { get; set; } = 1;

    /// <summary>
    /// Whether this is a public template (in template gallery)
    /// </summary>
    public bool IsPublic { get; set; } = false;

    /// <summary>
    /// Gets the actual width in pixels based on PageSize
    /// </summary>
    [JsonIgnore]
    public int WidthInPixels
    {
        get
        {
            return PageSize switch
            {
                PageSize.A4 => 794, // 210mm at 96 DPI
                PageSize.Letter => 816, // 8.5in at 96 DPI
                PageSize.Legal => 816, // 8.5in at 96 DPI
                PageSize.A3 => 1123, // 297mm at 96 DPI
                PageSize.Custom => CustomWidth ?? 816,
                _ => 816
            };
        }
    }

    /// <summary>
    /// Gets the actual height in pixels based on PageSize
    /// </summary>
    [JsonIgnore]
    public int HeightInPixels
    {
        get
        {
            return PageSize switch
            {
                PageSize.A4 => 1123, // 297mm at 96 DPI
                PageSize.Letter => 1056, // 11in at 96 DPI
                PageSize.Legal => 1344, // 14in at 96 DPI
                PageSize.A3 => 1587, // 420mm at 96 DPI
                PageSize.Custom => CustomHeight ?? 1056,
                _ => 1056
            };
        }
    }

    /// <summary>
    /// Adds an element to the form
    /// </summary>
    public void AddElement(FormElement element)
    {
        Elements.Add(element);
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Removes an element from the form
    /// </summary>
    public bool RemoveElement(Guid elementId)
    {
        var element = Elements.FirstOrDefault(e => e.Id == elementId);
        if (element != null)
        {
            Elements.Remove(element);
            UpdatedAt = DateTime.UtcNow;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Gets an element by ID
    /// </summary>
    public FormElement? GetElement(Guid elementId)
    {
        return Elements.FirstOrDefault(e => e.Id == elementId);
    }

    /// <summary>
    /// Clears all selected elements
    /// </summary>
    public void ClearSelection()
    {
        foreach (var element in Elements)
        {
            element.IsSelected = false;
        }
    }

    /// <summary>
    /// Gets all currently selected elements
    /// </summary>
    public List<FormElement> GetSelectedElements()
    {
        return Elements.Where(e => e.IsSelected).ToList();
    }

    /// <summary>
    /// Updates the timestamp
    /// </summary>
    public void MarkAsUpdated()
    {
        UpdatedAt = DateTime.UtcNow;
    }

    // ===== Multi-Page Support Methods =====

    /// <summary>
    /// Gets the currently active page
    /// </summary>
    [JsonIgnore]
    public FormPage? CurrentPage
    {
        get
        {
            EnsureMultiPageInitialized();
            if (Pages.Count == 0) return null;
            if (CurrentPageIndex < 0 || CurrentPageIndex >= Pages.Count)
                CurrentPageIndex = 0;
            return Pages[CurrentPageIndex];
        }
    }

    /// <summary>
    /// Ensures the form is initialized for multi-page mode
    /// Migrates legacy single-page forms if needed
    /// </summary>
    public void EnsureMultiPageInitialized()
    {
        // If already multi-page with pages AND has legacy elements, migrate them
        if (IsMultiPage && Pages.Count > 0 && Elements.Count > 0)
        {
            // Migrate legacy elements to the current page
            var currentPage = Pages[CurrentPageIndex >= 0 && CurrentPageIndex < Pages.Count ? CurrentPageIndex : 0];
            foreach (var element in Elements)
            {
                if (!currentPage.Elements.Contains(element))
                {
                    currentPage.Elements.Add(element);
                }
            }
            Elements.Clear(); // Clear legacy list after migration
            return;
        }

        // If already multi-page with pages, we're good
        if (IsMultiPage && Pages.Count > 0)
            return;

        // If not multi-page but has elements, migrate to single page
        if (!IsMultiPage && Elements.Count > 0)
        {
            var page = new FormPage
            {
                Name = "Page 1",
                Order = 0,
                Elements = new List<FormElement>(Elements)
            };
            Pages = new List<FormPage> { page };
            IsMultiPage = true;
            Elements.Clear(); // Clear legacy list after migration
            return;
        }

        // If multi-page but no pages, create default page
        if (IsMultiPage && Pages.Count == 0)
        {
            AddPage();
        }

        // If not multi-page and no elements, create default page
        if (!IsMultiPage && Elements.Count == 0)
        {
            IsMultiPage = true;
            AddPage();
        }
    }

    /// <summary>
    /// Adds a new page to the form
    /// </summary>
    public FormPage AddPage(string? name = null)
    {
        var pageNumber = Pages.Count + 1;
        var page = new FormPage
        {
            Name = name ?? $"Page {pageNumber}",
            Order = Pages.Count
        };
        Pages.Add(page);
        UpdatedAt = DateTime.UtcNow;
        return page;
    }

    /// <summary>
    /// Removes a page from the form
    /// </summary>
    public bool RemovePage(Guid pageId)
    {
        var page = Pages.FirstOrDefault(p => p.Id == pageId);
        if (page != null && Pages.Count > 1) // Don't allow deleting the last page
        {
            Pages.Remove(page);
            // Reorder remaining pages
            for (int i = 0; i < Pages.Count; i++)
            {
                Pages[i].Order = i;
            }
            // Adjust current page index if needed
            if (CurrentPageIndex >= Pages.Count)
            {
                CurrentPageIndex = Pages.Count - 1;
            }
            UpdatedAt = DateTime.UtcNow;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Duplicates a page
    /// </summary>
    public FormPage? DuplicatePage(Guid pageId)
    {
        var page = Pages.FirstOrDefault(p => p.Id == pageId);
        if (page != null)
        {
            var clone = page.Clone();
            clone.Order = Pages.Count;
            Pages.Add(clone);
            UpdatedAt = DateTime.UtcNow;
            return clone;
        }
        return null;
    }

    /// <summary>
    /// Gets a page by ID
    /// </summary>
    public FormPage? GetPage(Guid pageId)
    {
        return Pages.FirstOrDefault(p => p.Id == pageId);
    }

    /// <summary>
    /// Switches to a specific page
    /// </summary>
    public void SwitchToPage(int pageIndex)
    {
        if (pageIndex >= 0 && pageIndex < Pages.Count)
        {
            CurrentPageIndex = pageIndex;
        }
    }

    /// <summary>
    /// Switches to a specific page by ID
    /// </summary>
    public void SwitchToPage(Guid pageId)
    {
        var index = Pages.FindIndex(p => p.Id == pageId);
        if (index >= 0)
        {
            CurrentPageIndex = index;
        }
    }

    /// <summary>
    /// Moves to the next page
    /// </summary>
    public bool NextPage()
    {
        if (CurrentPageIndex < Pages.Count - 1)
        {
            CurrentPageIndex++;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Moves to the previous page
    /// </summary>
    public bool PreviousPage()
    {
        if (CurrentPageIndex > 0)
        {
            CurrentPageIndex--;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Reorders pages
    /// </summary>
    public void ReorderPages(List<Guid> pageIds)
    {
        var reorderedPages = new List<FormPage>();
        for (int i = 0; i < pageIds.Count; i++)
        {
            var page = Pages.FirstOrDefault(p => p.Id == pageIds[i]);
            if (page != null)
            {
                page.Order = i;
                reorderedPages.Add(page);
            }
        }
        Pages = reorderedPages;
        UpdatedAt = DateTime.UtcNow;
    }
}
