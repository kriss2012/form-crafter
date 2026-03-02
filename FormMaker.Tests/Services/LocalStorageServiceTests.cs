using FluentAssertions;
using FormMaker.Client.Services;
using FormMaker.Shared.Enums;
using FormMaker.Shared.Models;
using Microsoft.JSInterop;
using Microsoft.JSInterop.Infrastructure;
using Moq;
using System.Text.Json;
using Xunit;

namespace FormMaker.Tests.Services;

public class LocalStorageServiceTests
{
    private readonly Mock<IJSRuntime> _jsRuntimeMock;
    private readonly LocalStorageService _service;

    public LocalStorageServiceTests()
    {
        _jsRuntimeMock = new Mock<IJSRuntime>();
        _service = new LocalStorageService(_jsRuntimeMock.Object);
    }

    #region SaveFormAsync Tests

    [Fact]
    public async Task SaveFormAsync_WithValidForm_SavesToLocalStorage()
    {
        // Arrange
        var form = new FormTemplate
        {
            Id = Guid.NewGuid(),
            Name = "Test Form",
            PageSize = PageSize.Letter
        };

        string? formKey = null;
        string? formJson = null;
        var callCount = 0;

        _jsRuntimeMock
            .Setup(x => x.InvokeAsync<IJSVoidResult>("localStorage.setItem", It.IsAny<object[]>()))
            .Callback<string, object[]>((method, args) =>
            {
                // First call is for the form, second is for the forms list
                if (callCount == 0)
                {
                    formKey = args[0].ToString();
                    formJson = args[1].ToString();
                }
                callCount++;
            })
            .Returns(ValueTask.FromResult<IJSVoidResult>(null!));

        _jsRuntimeMock
            .Setup(x => x.InvokeAsync<string>("localStorage.getItem", It.IsAny<object[]>()))
            .ReturnsAsync((string?)null);

        // Act
        await _service.SaveFormAsync(form);

        // Assert
        formKey.Should().Be($"formmaker_form_{form.Id}");
        formJson.Should().NotBeNullOrEmpty();

        var savedForm = JsonSerializer.Deserialize<FormTemplate>(formJson!);
        savedForm.Should().NotBeNull();
        savedForm!.Id.Should().Be(form.Id);
        savedForm.Name.Should().Be("Test Form");
    }

    [Fact]
    public async Task SaveFormAsync_UpdatesUpdatedAtTimestamp()
    {
        // Arrange
        var originalTime = DateTime.UtcNow.AddDays(-1);
        var form = new FormTemplate
        {
            Id = Guid.NewGuid(),
            UpdatedAt = originalTime
        };

        _jsRuntimeMock
            .Setup(x => x.InvokeAsync<IJSVoidResult>("localStorage.setItem", It.IsAny<object[]>()))
            .Returns(ValueTask.FromResult<IJSVoidResult>(null!));

        _jsRuntimeMock
            .Setup(x => x.InvokeAsync<string>("localStorage.getItem", It.IsAny<object[]>()))
            .ReturnsAsync((string?)null);

        // Act
        await _service.SaveFormAsync(form);

        // Assert
        form.UpdatedAt.Should().BeAfter(originalTime);
        form.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task SaveFormAsync_WithNullForm_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _service.SaveFormAsync(null!));
    }

    [Fact]
    public async Task SaveFormAsync_UpdatesFormsList()
    {
        // Arrange
        var form = new FormTemplate
        {
            Id = Guid.NewGuid(),
            Name = "Test Form",
            Description = "Test Description"
        };

        _jsRuntimeMock
            .Setup(x => x.InvokeAsync<IJSVoidResult>("localStorage.setItem", It.IsAny<object[]>()))
            .Returns(ValueTask.FromResult<IJSVoidResult>(null!));

        _jsRuntimeMock
            .Setup(x => x.InvokeAsync<string>("localStorage.getItem", It.Is<object[]>(args => args[0].ToString() == "formmaker_forms_list")))
            .ReturnsAsync((string?)null);

        // Act
        await _service.SaveFormAsync(form);

        // Assert
        _jsRuntimeMock.Verify(
            x => x.InvokeAsync<IJSVoidResult>("localStorage.setItem",
                It.Is<object[]>(args => args[0].ToString() == "formmaker_forms_list")),
            Times.Once);
    }

    #endregion

    #region LoadFormAsync Tests

    [Fact]
    public async Task LoadFormAsync_WithExistingForm_ReturnsForm()
    {
        // Arrange
        var formId = Guid.NewGuid();
        var form = new FormTemplate
        {
            Id = formId,
            Name = "Test Form",
            PageSize = PageSize.A4
        };

        var json = JsonSerializer.Serialize(form);

        _jsRuntimeMock
            .Setup(x => x.InvokeAsync<string>("localStorage.getItem", It.IsAny<object[]>()))
            .ReturnsAsync(json);

        // Act
        var result = await _service.LoadFormAsync(formId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(formId);
        result.Name.Should().Be("Test Form");
        result.PageSize.Should().Be(PageSize.A4);
    }

    [Fact]
    public async Task LoadFormAsync_WithNonExistentForm_ReturnsNull()
    {
        // Arrange
        var formId = Guid.NewGuid();

        _jsRuntimeMock
            .Setup(x => x.InvokeAsync<string>("localStorage.getItem", It.IsAny<object[]>()))
            .ReturnsAsync((string?)null);

        // Act
        var result = await _service.LoadFormAsync(formId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task LoadFormAsync_WithEmptyJson_ReturnsNull()
    {
        // Arrange
        var formId = Guid.NewGuid();

        _jsRuntimeMock
            .Setup(x => x.InvokeAsync<string>("localStorage.getItem", It.IsAny<object[]>()))
            .ReturnsAsync(string.Empty);

        // Act
        var result = await _service.LoadFormAsync(formId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task LoadFormAsync_WithInvalidJson_ReturnsNull()
    {
        // Arrange
        var formId = Guid.NewGuid();

        _jsRuntimeMock
            .Setup(x => x.InvokeAsync<string>("localStorage.getItem", It.IsAny<object[]>()))
            .ReturnsAsync("{ invalid json }");

        // Act
        var result = await _service.LoadFormAsync(formId);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetAllFormsAsync Tests

    [Fact]
    public async Task GetAllFormsAsync_WithExistingForms_ReturnsList()
    {
        // Arrange
        var metadata = new List<FormMetadata>
        {
            new FormMetadata { Id = Guid.NewGuid(), Name = "Form 1" },
            new FormMetadata { Id = Guid.NewGuid(), Name = "Form 2" }
        };

        var json = JsonSerializer.Serialize(metadata);

        _jsRuntimeMock
            .Setup(x => x.InvokeAsync<string>("localStorage.getItem", It.IsAny<object[]>()))
            .ReturnsAsync(json);

        // Act
        var result = await _service.GetAllFormsAsync();

        // Assert
        result.Should().HaveCount(2);
        result[0].Name.Should().Be("Form 1");
        result[1].Name.Should().Be("Form 2");
    }

    [Fact]
    public async Task GetAllFormsAsync_WithNoForms_ReturnsEmptyList()
    {
        // Arrange
        _jsRuntimeMock
            .Setup(x => x.InvokeAsync<string>("localStorage.getItem", It.IsAny<object[]>()))
            .ReturnsAsync((string?)null);

        // Act
        var result = await _service.GetAllFormsAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllFormsAsync_WithInvalidJson_ReturnsEmptyList()
    {
        // Arrange
        _jsRuntimeMock
            .Setup(x => x.InvokeAsync<string>("localStorage.getItem", It.IsAny<object[]>()))
            .ReturnsAsync("{ invalid json }");

        // Act
        var result = await _service.GetAllFormsAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    #endregion

    #region DeleteFormAsync Tests

    [Fact]
    public async Task DeleteFormAsync_RemovesFormFromStorage()
    {
        // Arrange
        var formId = Guid.NewGuid();
        var expectedKey = $"formmaker_form_{formId}";

        _jsRuntimeMock
            .Setup(x => x.InvokeAsync<IJSVoidResult>("localStorage.removeItem", It.IsAny<object[]>()))
            .Returns(ValueTask.FromResult<IJSVoidResult>(null!));

        _jsRuntimeMock
            .Setup(x => x.InvokeAsync<string>("localStorage.getItem", It.IsAny<object[]>()))
            .ReturnsAsync((string?)null);

        _jsRuntimeMock
            .Setup(x => x.InvokeAsync<IJSVoidResult>("localStorage.setItem", It.IsAny<object[]>()))
            .Returns(ValueTask.FromResult<IJSVoidResult>(null!));

        // Act
        await _service.DeleteFormAsync(formId);

        // Assert
        _jsRuntimeMock.Verify(
            x => x.InvokeAsync<IJSVoidResult>("localStorage.removeItem",
                It.Is<object[]>(args => args[0].ToString() == expectedKey)),
            Times.Once);
    }

    [Fact]
    public async Task DeleteFormAsync_UpdatesFormsList()
    {
        // Arrange
        var formId = Guid.NewGuid();

        _jsRuntimeMock
            .Setup(x => x.InvokeAsync<IJSVoidResult>("localStorage.removeItem", It.IsAny<object[]>()))
            .Returns(ValueTask.FromResult<IJSVoidResult>(null!));

        _jsRuntimeMock
            .Setup(x => x.InvokeAsync<string>("localStorage.getItem", It.IsAny<object[]>()))
            .ReturnsAsync((string?)null);

        _jsRuntimeMock
            .Setup(x => x.InvokeAsync<IJSVoidResult>("localStorage.setItem", It.IsAny<object[]>()))
            .Returns(ValueTask.FromResult<IJSVoidResult>(null!));

        // Act
        await _service.DeleteFormAsync(formId);

        // Assert
        _jsRuntimeMock.Verify(
            x => x.InvokeAsync<IJSVoidResult>("localStorage.setItem",
                It.Is<object[]>(args => args[0].ToString() == "formmaker_forms_list")),
            Times.Once);
    }

    #endregion

    #region FormExistsAsync Tests

    [Fact]
    public async Task FormExistsAsync_WithExistingForm_ReturnsTrue()
    {
        // Arrange
        var formId = Guid.NewGuid();

        _jsRuntimeMock
            .Setup(x => x.InvokeAsync<string>("localStorage.getItem", It.IsAny<object[]>()))
            .ReturnsAsync("{ \"id\": \"test\" }");

        // Act
        var result = await _service.FormExistsAsync(formId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task FormExistsAsync_WithNonExistentForm_ReturnsFalse()
    {
        // Arrange
        var formId = Guid.NewGuid();

        _jsRuntimeMock
            .Setup(x => x.InvokeAsync<string>("localStorage.getItem", It.IsAny<object[]>()))
            .ReturnsAsync((string?)null);

        // Act
        var result = await _service.FormExistsAsync(formId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task FormExistsAsync_WithEmptyJson_ReturnsFalse()
    {
        // Arrange
        var formId = Guid.NewGuid();

        _jsRuntimeMock
            .Setup(x => x.InvokeAsync<string>("localStorage.getItem", It.IsAny<object[]>()))
            .ReturnsAsync(string.Empty);

        // Act
        var result = await _service.FormExistsAsync(formId);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region ClearAllFormsAsync Tests

    [Fact]
    public async Task ClearAllFormsAsync_DeletesAllFormsAndList()
    {
        // Arrange
        var forms = new List<FormMetadata>
        {
            new FormMetadata { Id = Guid.NewGuid(), Name = "Form 1" },
            new FormMetadata { Id = Guid.NewGuid(), Name = "Form 2" }
        };

        var json = JsonSerializer.Serialize(forms);

        _jsRuntimeMock
            .SetupSequence(x => x.InvokeAsync<string>("localStorage.getItem", It.IsAny<object[]>()))
            .ReturnsAsync(json)  // First call returns the forms list
            .ReturnsAsync((string?)null)  // Subsequent calls return null
            .ReturnsAsync((string?)null);

        _jsRuntimeMock
            .Setup(x => x.InvokeAsync<IJSVoidResult>("localStorage.removeItem", It.IsAny<object[]>()))
            .Returns(ValueTask.FromResult<IJSVoidResult>(null!));

        _jsRuntimeMock
            .Setup(x => x.InvokeAsync<IJSVoidResult>("localStorage.setItem", It.IsAny<object[]>()))
            .Returns(ValueTask.FromResult<IJSVoidResult>(null!));

        // Act
        await _service.ClearAllFormsAsync();

        // Assert
        // Should delete each form (2 forms) + the forms list
        _jsRuntimeMock.Verify(
            x => x.InvokeAsync<IJSVoidResult>("localStorage.removeItem", It.IsAny<object[]>()),
            Times.AtLeast(3));
    }

    [Fact]
    public async Task ClearAllFormsAsync_WithNoForms_DoesNotThrow()
    {
        // Arrange
        _jsRuntimeMock
            .Setup(x => x.InvokeAsync<string>("localStorage.getItem", It.IsAny<object[]>()))
            .ReturnsAsync((string?)null);

        _jsRuntimeMock
            .Setup(x => x.InvokeAsync<IJSVoidResult>("localStorage.removeItem", It.IsAny<object[]>()))
            .Returns(ValueTask.FromResult<IJSVoidResult>(null!));

        // Act
        Func<Task> act = async () => await _service.ClearAllFormsAsync();

        // Assert
        await act.Should().NotThrowAsync();
    }

    #endregion
}
