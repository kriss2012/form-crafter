using System.Text.Json.Serialization;

namespace FormMaker.Shared.Models;

/// <summary>
/// Represents a single page in a multi-page form
/// </summary>
public class FormPage
{
    /// <summary>
    /// Unique identifier for the page
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Name/title of the page
    /// </summary>
    public string Name { get; set; } = "Page 1";

    /// <summary>
    /// Order/position of the page in the form (0-based index)
    /// </summary>
    public int Order { get; set; } = 0;

    /// <summary>
    /// All elements on this page
    /// </summary>
    public List<FormElement> Elements { get; set; } = new();

    /// <summary>
    /// When the page was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When the page was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Adds an element to the page
    /// </summary>
    public void AddElement(FormElement element)
    {
        Elements.Add(element);
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Removes an element from the page
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
    /// Clears all selected elements on this page
    /// </summary>
    public void ClearSelection()
    {
        foreach (var element in Elements)
        {
            element.IsSelected = false;
        }
    }

    /// <summary>
    /// Gets all currently selected elements on this page
    /// </summary>
    public List<FormElement> GetSelectedElements()
    {
        return Elements.Where(e => e.IsSelected).ToList();
    }

    /// <summary>
    /// Creates a deep copy of this page
    /// </summary>
    public FormPage Clone()
    {
        var clone = new FormPage
        {
            Id = Guid.NewGuid(),
            Name = $"{Name} (Copy)",
            Order = Order,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Clone all elements
        foreach (var element in Elements)
        {
            var clonedElement = element.Clone();
            clone.Elements.Add(clonedElement);
        }

        return clone;
    }

    /// <summary>
    /// Updates the timestamp
    /// </summary>
    public void MarkAsUpdated()
    {
        UpdatedAt = DateTime.UtcNow;
    }
}
