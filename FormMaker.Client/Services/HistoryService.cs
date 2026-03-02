using FormMaker.Shared.Models;
using System.Text.Json;

namespace FormMaker.Client.Services;

public class HistoryService
{
    private readonly Stack<string> _undoStack = new();
    private readonly Stack<string> _redoStack = new();
    private const int MaxHistorySize = 50;

    public bool CanUndo => _undoStack.Count > 0;
    public bool CanRedo => _redoStack.Count > 0;

    public event Action? OnStateChanged;

    /// <summary>
    /// Records the current state for undo functionality
    /// </summary>
    public void RecordState(FormTemplate template)
    {
        // Serialize the template to JSON for deep copy
        var json = JsonSerializer.Serialize(template);

        _undoStack.Push(json);

        // Clear redo stack when new action is performed
        _redoStack.Clear();

        // Limit history size
        if (_undoStack.Count > MaxHistorySize)
        {
            // Remove oldest item
            var items = _undoStack.ToArray();
            _undoStack.Clear();
            for (int i = 0; i < MaxHistorySize; i++)
            {
                _undoStack.Push(items[i]);
            }
        }

        OnStateChanged?.Invoke();
    }

    /// <summary>
    /// Undo the last action and return the previous state
    /// </summary>
    public FormTemplate? Undo(FormTemplate currentTemplate)
    {
        if (!CanUndo)
            return null;

        // Save current state to redo stack
        var currentJson = JsonSerializer.Serialize(currentTemplate);
        _redoStack.Push(currentJson);

        // Get previous state
        var previousJson = _undoStack.Pop();
        var previousTemplate = JsonSerializer.Deserialize<FormTemplate>(previousJson);

        OnStateChanged?.Invoke();
        return previousTemplate;
    }

    /// <summary>
    /// Redo the last undone action
    /// </summary>
    public FormTemplate? Redo()
    {
        if (!CanRedo)
            return null;

        // Get state from redo stack
        var json = _redoStack.Pop();
        var template = JsonSerializer.Deserialize<FormTemplate>(json);

        // Push back to undo stack
        _undoStack.Push(json);

        OnStateChanged?.Invoke();
        return template;
    }

    /// <summary>
    /// Clear all history
    /// </summary>
    public void Clear()
    {
        _undoStack.Clear();
        _redoStack.Clear();
        OnStateChanged?.Invoke();
    }

    /// <summary>
    /// Get the number of undo actions available
    /// </summary>
    public int UndoCount => _undoStack.Count;

    /// <summary>
    /// Get the number of redo actions available
    /// </summary>
    public int RedoCount => _redoStack.Count;
}
