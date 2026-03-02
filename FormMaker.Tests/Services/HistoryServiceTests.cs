using FluentAssertions;
using FormMaker.Client.Services;
using FormMaker.Shared.Enums;
using FormMaker.Shared.Models;
using FormMaker.Shared.Models.Elements;
using Xunit;

namespace FormMaker.Tests.Services;

public class HistoryServiceTests
{
    private readonly HistoryService _service;

    public HistoryServiceTests()
    {
        _service = new HistoryService();
    }

    #region RecordState Tests

    [Fact]
    public void RecordState_AddsToUndoStack()
    {
        // Arrange
        var template = new FormTemplate { Name = "Test Form" };

        // Act
        _service.RecordState(template);

        // Assert
        _service.CanUndo.Should().BeTrue();
        _service.UndoCount.Should().Be(1);
    }

    [Fact]
    public void RecordState_ClearsRedoStack()
    {
        // Arrange
        var template1 = new FormTemplate { Name = "Form 1" };
        var template2 = new FormTemplate { Name = "Form 2" };

        _service.RecordState(template1);
        _service.Undo(template2);  // This should add to redo stack

        // Act
        _service.RecordState(template2);  // Recording new state should clear redo

        // Assert
        _service.CanRedo.Should().BeFalse();
        _service.RedoCount.Should().Be(0);
    }

    [Fact]
    public void RecordState_TriggersOnStateChangedEvent()
    {
        // Arrange
        var template = new FormTemplate { Name = "Test Form" };
        var eventTriggered = false;
        _service.OnStateChanged += () => eventTriggered = true;

        // Act
        _service.RecordState(template);

        // Assert
        eventTriggered.Should().BeTrue();
    }

    [Fact]
    public void RecordState_LimitsHistoryTo50Items()
    {
        // Arrange
        var template = new FormTemplate { Name = "Test Form" };

        // Act
        for (int i = 0; i < 60; i++)
        {
            template.Name = $"Form {i}";
            _service.RecordState(template);
        }

        // Assert
        _service.UndoCount.Should().Be(50);
    }

    [Fact]
    public void RecordState_WithMultipleStates_MaintainsOrder()
    {
        // Arrange
        var template1 = new FormTemplate { Name = "Form 1" };
        var template2 = new FormTemplate { Name = "Form 2" };
        var template3 = new FormTemplate { Name = "Form 3" };

        // Act
        _service.RecordState(template1);
        _service.RecordState(template2);

        // Assert
        _service.UndoCount.Should().Be(2);

        // Undo should return states in reverse order
        var undone1 = _service.Undo(template3);
        undone1.Should().NotBeNull();
        undone1!.Name.Should().Be("Form 2");

        var undone2 = _service.Undo(template2);
        undone2.Should().NotBeNull();
        undone2!.Name.Should().Be("Form 1");
    }

    #endregion

    #region Undo Tests

    [Fact]
    public void Undo_WithNoHistory_ReturnsNull()
    {
        // Arrange
        var currentTemplate = new FormTemplate { Name = "Current" };

        // Act
        var result = _service.Undo(currentTemplate);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void Undo_ReturnsPreviousState()
    {
        // Arrange
        var previousTemplate = new FormTemplate { Name = "Previous" };
        var currentTemplate = new FormTemplate { Name = "Current" };

        _service.RecordState(previousTemplate);

        // Act
        var result = _service.Undo(currentTemplate);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Previous");
    }

    [Fact]
    public void Undo_MovesCurrentStateToRedoStack()
    {
        // Arrange
        var previousTemplate = new FormTemplate { Name = "Previous" };
        var currentTemplate = new FormTemplate { Name = "Current" };

        _service.RecordState(previousTemplate);

        // Act
        _service.Undo(currentTemplate);

        // Assert
        _service.CanRedo.Should().BeTrue();
        _service.RedoCount.Should().Be(1);
    }

    [Fact]
    public void Undo_TriggersOnStateChangedEvent()
    {
        // Arrange
        var previousTemplate = new FormTemplate { Name = "Previous" };
        var currentTemplate = new FormTemplate { Name = "Current" };
        var eventTriggered = false;

        _service.RecordState(previousTemplate);
        _service.OnStateChanged += () => eventTriggered = true;

        // Act
        _service.Undo(currentTemplate);

        // Assert
        eventTriggered.Should().BeTrue();
    }

    [Fact]
    public void Undo_DecrementsUndoCount()
    {
        // Arrange
        var template1 = new FormTemplate { Name = "Form 1" };
        var template2 = new FormTemplate { Name = "Form 2" };

        _service.RecordState(template1);
        _service.RecordState(template2);

        var initialCount = _service.UndoCount;

        // Act
        _service.Undo(new FormTemplate());

        // Assert
        _service.UndoCount.Should().Be(initialCount - 1);
    }

    [Fact]
    public void Undo_PreservesElementsAndProperties()
    {
        // Arrange
        var previousTemplate = new FormTemplate
        {
            Name = "Test Form",
            PageSize = PageSize.A4
        };

        previousTemplate.Elements.Add(new LabelElement
        {
            Text = "Test Label",
            X = 100,
            Y = 200
        });

        var currentTemplate = new FormTemplate { Name = "Current" };

        _service.RecordState(previousTemplate);

        // Act
        var result = _service.Undo(currentTemplate);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Test Form");
        result.PageSize.Should().Be(PageSize.A4);
        result.Elements.Should().HaveCount(1);

        var label = result.Elements[0] as LabelElement;
        label.Should().NotBeNull();
        label!.Text.Should().Be("Test Label");
        label.X.Should().Be(100);
        label.Y.Should().Be(200);
    }

    #endregion

    #region Redo Tests

    [Fact]
    public void Redo_WithNoRedoHistory_ReturnsNull()
    {
        // Act
        var result = _service.Redo();

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void Redo_AfterUndo_ReturnsUndoneState()
    {
        // Arrange
        var template1 = new FormTemplate { Name = "Form 1" };
        var template2 = new FormTemplate { Name = "Form 2" };

        _service.RecordState(template1);

        var undoResult = _service.Undo(template2);

        // Act
        var redoResult = _service.Redo();

        // Assert
        redoResult.Should().NotBeNull();
        redoResult!.Name.Should().Be("Form 2");
    }

    [Fact]
    public void Redo_MovesStateBackToUndoStack()
    {
        // Arrange
        var template1 = new FormTemplate { Name = "Form 1" };
        var template2 = new FormTemplate { Name = "Form 2" };

        _service.RecordState(template1);
        _service.Undo(template2);

        var undoCountBefore = _service.UndoCount;

        // Act
        _service.Redo();

        // Assert
        _service.UndoCount.Should().Be(undoCountBefore + 1);
    }

    [Fact]
    public void Redo_TriggersOnStateChangedEvent()
    {
        // Arrange
        var template1 = new FormTemplate { Name = "Form 1" };
        var template2 = new FormTemplate { Name = "Form 2" };
        var eventTriggered = false;

        _service.RecordState(template1);
        _service.Undo(template2);
        _service.OnStateChanged += () => eventTriggered = true;

        // Act
        _service.Redo();

        // Assert
        eventTriggered.Should().BeTrue();
    }

    [Fact]
    public void Redo_DecrementsRedoCount()
    {
        // Arrange
        var template1 = new FormTemplate { Name = "Form 1" };
        var template2 = new FormTemplate { Name = "Form 2" };

        _service.RecordState(template1);
        _service.Undo(template2);

        var initialRedoCount = _service.RedoCount;

        // Act
        _service.Redo();

        // Assert
        _service.RedoCount.Should().Be(initialRedoCount - 1);
    }

    #endregion

    #region Clear Tests

    [Fact]
    public void Clear_RemovesAllHistory()
    {
        // Arrange
        var template1 = new FormTemplate { Name = "Form 1" };
        var template2 = new FormTemplate { Name = "Form 2" };

        _service.RecordState(template1);
        _service.Undo(template2);

        // Act
        _service.Clear();

        // Assert
        _service.CanUndo.Should().BeFalse();
        _service.CanRedo.Should().BeFalse();
        _service.UndoCount.Should().Be(0);
        _service.RedoCount.Should().Be(0);
    }

    [Fact]
    public void Clear_TriggersOnStateChangedEvent()
    {
        // Arrange
        var template = new FormTemplate { Name = "Test" };
        var eventTriggered = false;

        _service.RecordState(template);
        _service.OnStateChanged += () => eventTriggered = true;

        // Act
        _service.Clear();

        // Assert
        eventTriggered.Should().BeTrue();
    }

    #endregion

    #region CanUndo and CanRedo Tests

    [Fact]
    public void CanUndo_WithNoHistory_ReturnsFalse()
    {
        // Assert
        _service.CanUndo.Should().BeFalse();
    }

    [Fact]
    public void CanUndo_WithHistory_ReturnsTrue()
    {
        // Arrange
        var template = new FormTemplate { Name = "Test" };

        // Act
        _service.RecordState(template);

        // Assert
        _service.CanUndo.Should().BeTrue();
    }

    [Fact]
    public void CanRedo_WithNoRedoHistory_ReturnsFalse()
    {
        // Assert
        _service.CanRedo.Should().BeFalse();
    }

    [Fact]
    public void CanRedo_AfterUndo_ReturnsTrue()
    {
        // Arrange
        var template1 = new FormTemplate { Name = "Form 1" };
        var template2 = new FormTemplate { Name = "Form 2" };

        _service.RecordState(template1);

        // Act
        _service.Undo(template2);

        // Assert
        _service.CanRedo.Should().BeTrue();
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void UndoRedoCycle_MaintainsDataIntegrity()
    {
        // Arrange
        var form1 = new FormTemplate { Name = "Form 1", PageSize = PageSize.Letter };
        var form2 = new FormTemplate { Name = "Form 2", PageSize = PageSize.A4 };
        var form3 = new FormTemplate { Name = "Form 3", PageSize = PageSize.Legal };

        // Record states
        _service.RecordState(form1);
        _service.RecordState(form2);

        // Act - Undo twice
        var undo1 = _service.Undo(form3);
        var undo2 = _service.Undo(form2);

        // Redo once
        var redo1 = _service.Redo();

        // Assert
        undo1!.Name.Should().Be("Form 2");
        undo1.PageSize.Should().Be(PageSize.A4);

        undo2!.Name.Should().Be("Form 1");
        undo2.PageSize.Should().Be(PageSize.Letter);

        redo1!.Name.Should().Be("Form 2");
        redo1.PageSize.Should().Be(PageSize.A4);
    }

    [Fact]
    public void MultipleUndoRedo_WorksCorrectly()
    {
        // Arrange
        for (int i = 0; i < 10; i++)
        {
            var form = new FormTemplate { Name = $"Form {i}" };
            _service.RecordState(form);
        }

        // Act - Undo 5 times
        for (int i = 0; i < 5; i++)
        {
            _service.Undo(new FormTemplate());
        }

        // Redo 3 times
        for (int i = 0; i < 3; i++)
        {
            _service.Redo();
        }

        // Assert
        _service.UndoCount.Should().Be(8);  // 10 - 5 + 3
        _service.RedoCount.Should().Be(2);  // 5 - 3
    }

    #endregion
}
