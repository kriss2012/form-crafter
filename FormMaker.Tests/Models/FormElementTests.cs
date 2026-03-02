using FluentAssertions;
using FormMaker.Shared.Enums;
using FormMaker.Shared.Models;
using FormMaker.Shared.Models.Elements;
using System.Text.Json;
using Xunit;

namespace FormMaker.Tests.Models;

public class FormElementTests
{
    #region FormTemplate Tests

    [Fact]
    public void FormTemplate_DefaultValues_AreSetCorrectly()
    {
        // Act
        var template = new FormTemplate();

        // Assert
        template.Id.Should().NotBeEmpty();
        template.Name.Should().Be("Untitled Form");
        template.PageSize.Should().Be(PageSize.Letter);
        template.Elements.Should().NotBeNull();
        template.Elements.Should().BeEmpty();
        template.BackgroundColor.Should().Be("#ffffff");
        template.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        template.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void FormTemplate_PageSizeDimensions_AreCorrect()
    {
        // Arrange & Act
        var letterTemplate = new FormTemplate { PageSize = PageSize.Letter };
        var a4Template = new FormTemplate { PageSize = PageSize.A4 };
        var legalTemplate = new FormTemplate { PageSize = PageSize.Legal };

        // Assert
        letterTemplate.WidthInPixels.Should().Be(816);  // 8.5 inches * 96 dpi
        letterTemplate.HeightInPixels.Should().Be(1056);  // 11 inches * 96 dpi

        a4Template.WidthInPixels.Should().Be(794);  // 210mm → pixels
        a4Template.HeightInPixels.Should().Be(1123);  // 297mm → pixels

        legalTemplate.WidthInPixels.Should().Be(816);  // 8.5 inches * 96 dpi
        legalTemplate.HeightInPixels.Should().Be(1344);  // 14 inches * 96 dpi
    }

    [Fact]
    public void FormTemplate_ClearSelection_DeselectsAllElements()
    {
        // Arrange
        var template = new FormTemplate();
        template.Elements.Add(new LabelElement { IsSelected = true });
        template.Elements.Add(new TextInputElement { IsSelected = true });
        template.Elements.Add(new CheckboxElement { IsSelected = true });

        // Act
        template.ClearSelection();

        // Assert
        template.Elements.Should().AllSatisfy(e => e.IsSelected.Should().BeFalse());
    }

    #endregion

    #region FormElement Base Tests

    [Fact]
    public void FormElement_DefaultValues_AreSetCorrectly()
    {
        // Act
        var element = new LabelElement();

        // Assert
        element.Id.Should().NotBeEmpty();
        element.X.Should().Be(0);
        element.Y.Should().Be(0);
        element.Width.Should().Be(200);
        element.Height.Should().Be(30);
        element.IsSelected.Should().BeFalse();
        element.Properties.Should().NotBeNull();
    }

    [Fact]
    public void FormElement_PropertiesDefaultValues_AreCorrect()
    {
        // Act
        var element = new LabelElement();

        // Assert
        element.Properties.FontSize.Should().Be(18);
        element.Properties.Bold.Should().BeFalse();
        element.Properties.Italic.Should().BeFalse();
        element.Properties.Underline.Should().BeFalse();
        element.Properties.Color.Should().Be("#1a1a1a");
        element.Properties.BackgroundColor.Should().Be("transparent");
        element.Properties.Alignment.Should().Be(TextAlignment.Left);
        element.Properties.ZIndex.Should().Be(0);
    }

    #endregion

    #region LabelElement Tests

    [Fact]
    public void LabelElement_DefaultValues_AreCorrect()
    {
        // Act
        var label = new LabelElement();

        // Assert
        label.Text.Should().Be("Label Text");
        label.Width.Should().Be(200);
        label.Height.Should().Be(30);
        label.Type.Should().Be(ElementType.Label);
    }

    [Fact]
    public void LabelElement_CanSetCustomText()
    {
        // Arrange
        var label = new LabelElement();

        // Act
        label.Text = "Custom Label Text";

        // Assert
        label.Text.Should().Be("Custom Label Text");
    }

    #endregion

    #region TextInputElement Tests

    [Fact]
    public void TextInputElement_DefaultValues_AreCorrect()
    {
        // Act
        var input = new TextInputElement();

        // Assert
        input.Placeholder.Should().Be("Enter text...");
        input.MaxLength.Should().BeNull();
        input.IsRequired.Should().BeFalse();
        input.Type.Should().Be(ElementType.TextInput);
    }

    [Fact]
    public void TextInputElement_CanSetAllProperties()
    {
        // Act
        var input = new TextInputElement
        {
            Placeholder = "Enter name",
            DefaultValue = "John Doe",
            MaxLength = 100,
            IsRequired = true
        };

        // Assert
        input.Placeholder.Should().Be("Enter name");
        input.DefaultValue.Should().Be("John Doe");
        input.MaxLength.Should().Be(100);
        input.IsRequired.Should().BeTrue();
    }

    #endregion

    #region CheckboxElement Tests

    [Fact]
    public void CheckboxElement_DefaultValues_AreCorrect()
    {
        // Act
        var checkbox = new CheckboxElement();

        // Assert
        checkbox.Label.Should().Be("Checkbox Label");
        checkbox.DefaultChecked.Should().BeFalse();
        checkbox.IsRequired.Should().BeFalse();
        checkbox.Type.Should().Be(ElementType.Checkbox);
    }

    #endregion

    #region ImageElement Tests

    [Fact]
    public void ImageElement_DefaultValues_AreCorrect()
    {
        // Act
        var image = new ImageElement();

        // Assert
        image.ImageUrl.Should().BeNull();
        image.AltText.Should().Be("Image");
        image.Label.Should().Be("Image");
        image.Width.Should().Be(200);
        image.Height.Should().Be(200);
        image.MaintainAspectRatio.Should().BeTrue();
        image.Type.Should().Be(ElementType.Image);
    }

    #endregion

    #region TextAreaElement Tests

    [Fact]
    public void TextAreaElement_DefaultValues_AreCorrect()
    {
        // Act
        var textArea = new TextAreaElement();

        // Assert
        textArea.Placeholder.Should().Be("Enter text...");
        textArea.Rows.Should().Be(4);
        textArea.MaxLength.Should().BeNull();
        textArea.Resizable.Should().BeFalse();
        textArea.WrapText.Should().BeTrue();
        textArea.Type.Should().Be(ElementType.TextArea);
    }

    #endregion

    #region DropdownElement Tests

    [Fact]
    public void DropdownElement_DefaultValues_AreCorrect()
    {
        // Act
        var dropdown = new DropdownElement();

        // Assert
        dropdown.Placeholder.Should().Be("Select an option...");
        dropdown.Options.Should().NotBeNull();
        dropdown.Options.Should().HaveCount(3);
        dropdown.AllowMultiple.Should().BeFalse();
        dropdown.IsRequired.Should().BeFalse();
        dropdown.Type.Should().Be(ElementType.Dropdown);
    }

    [Fact]
    public void DropdownElement_CanAddOptions()
    {
        // Arrange
        var dropdown = new DropdownElement();
        dropdown.Options.Clear();

        // Act
        dropdown.Options.Add("Option 1");
        dropdown.Options.Add("Option 2");

        // Assert
        dropdown.Options.Should().HaveCount(2);
        dropdown.Options.Should().Contain("Option 1");
        dropdown.Options.Should().Contain("Option 2");
    }

    #endregion

    #region RadioGroupElement Tests

    [Fact]
    public void RadioGroupElement_DefaultValues_AreCorrect()
    {
        // Act
        var radioGroup = new RadioGroupElement();

        // Assert
        radioGroup.Label.Should().BeNull();
        radioGroup.Options.Should().HaveCount(3);
        radioGroup.Options.Should().Contain(new[] { "Option 1", "Option 2", "Option 3" });
        radioGroup.Layout.Should().Be("Vertical");
        radioGroup.IsRequired.Should().BeFalse();
        radioGroup.Type.Should().Be(ElementType.RadioGroup);
    }

    [Fact]
    public void RadioGroupElement_CanSetLayoutToHorizontal()
    {
        // Act
        var radioGroup = new RadioGroupElement
        {
            Layout = "Horizontal"
        };

        // Assert
        radioGroup.Layout.Should().Be("Horizontal");
    }

    #endregion

    #region DatePickerElement Tests

    [Fact]
    public void DatePickerElement_DefaultValues_AreCorrect()
    {
        // Act
        var datePicker = new DatePickerElement();

        // Assert
        datePicker.Placeholder.Should().Be("Select a date...");
        datePicker.DateFormat.Should().Be("MM/dd/yyyy");
        datePicker.IncludeTime.Should().BeFalse();
        datePicker.IsRequired.Should().BeFalse();
        datePicker.Type.Should().Be(ElementType.DatePicker);
    }

    [Fact]
    public void DatePickerElement_CanSetDateRange()
    {
        // Arrange
        var minDate = DateTime.Now;
        var maxDate = DateTime.Now.AddMonths(6);

        // Act
        var datePicker = new DatePickerElement
        {
            MinDate = minDate,
            MaxDate = maxDate
        };

        // Assert
        datePicker.MinDate.Should().Be(minDate);
        datePicker.MaxDate.Should().Be(maxDate);
    }

    #endregion

    #region FileUploadElement Tests

    [Fact]
    public void FileUploadElement_DefaultValues_AreCorrect()
    {
        // Act
        var fileUpload = new FileUploadElement();

        // Assert
        fileUpload.Label.Should().BeNull();
        fileUpload.ButtonText.Should().Be("Choose File");
        fileUpload.Multiple.Should().BeFalse();
        fileUpload.MaxFileSize.Should().Be(10);
        fileUpload.AllowedExtensions.Should().HaveCount(5);
        fileUpload.AllowedExtensions.Should().Contain(new[] { ".pdf", ".doc", ".docx", ".jpg", ".png" });
        fileUpload.Type.Should().Be(ElementType.FileUpload);
    }

    [Fact]
    public void FileUploadElement_CanSetAllowedExtensions()
    {
        // Act
        var fileUpload = new FileUploadElement();
        fileUpload.AllowedExtensions.Clear();
        fileUpload.AllowedExtensions.Add(".pdf");
        fileUpload.AllowedExtensions.Add(".doc");
        fileUpload.AllowedExtensions.Add(".docx");

        // Assert
        fileUpload.AllowedExtensions.Should().HaveCount(3);
        fileUpload.AllowedExtensions.Should().Contain(".pdf");
    }

    #endregion

    #region SignatureElement Tests

    [Fact]
    public void SignatureElement_DefaultValues_AreCorrect()
    {
        // Act
        var signature = new SignatureElement();

        // Assert
        signature.BorderColor.Should().Be("#000000");
        signature.LineWidth.Should().Be(2);
        signature.IsRequired.Should().BeFalse();
        signature.Type.Should().Be(ElementType.Signature);
    }

    #endregion

    #region DividerElement Tests

    [Fact]
    public void DividerElement_DefaultValues_AreCorrect()
    {
        // Act
        var divider = new DividerElement();

        // Assert
        divider.Thickness.Should().Be(2);
        divider.Color.Should().Be("#cccccc");
        divider.Style.Should().Be("Solid");
        divider.Type.Should().Be(ElementType.Divider);
    }

    [Fact]
    public void DividerElement_CanSetDifferentStyles()
    {
        // Act
        var dashedDivider = new DividerElement { Style = "Dashed" };
        var dottedDivider = new DividerElement { Style = "Dotted" };

        // Assert
        dashedDivider.Style.Should().Be("Dashed");
        dottedDivider.Style.Should().Be("Dotted");
    }

    #endregion

    #region TableElement Tests

    [Fact]
    public void TableElement_DefaultValues_AreCorrect()
    {
        // Act
        var table = new TableElement();

        // Assert
        table.Rows.Should().Be(3);
        table.Columns.Should().Be(3);
        table.ShowHeaders.Should().BeTrue();
        table.Type.Should().Be(ElementType.Table);
    }

    [Fact]
    public void TableElement_CanSetRowsAndColumns()
    {
        // Act
        var table = new TableElement
        {
            Rows = 5,
            Columns = 4
        };

        // Assert
        table.Rows.Should().Be(5);
        table.Columns.Should().Be(4);
    }

    #endregion

    #region Serialization Tests

    [Fact]
    public void FormTemplate_CanSerializeAndDeserialize()
    {
        // Arrange
        var template = new FormTemplate
        {
            Name = "Test Form",
            PageSize = PageSize.A4
        };

        template.Elements.Add(new LabelElement { Text = "Test Label" });
        template.Elements.Add(new TextInputElement { Placeholder = "Test Input" });

        // Act
        var json = JsonSerializer.Serialize(template);
        var deserialized = JsonSerializer.Deserialize<FormTemplate>(json);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized!.Name.Should().Be("Test Form");
        deserialized.PageSize.Should().Be(PageSize.A4);
        deserialized.Elements.Should().HaveCount(2);
    }

    [Fact]
    public void FormElement_SerializationPreservesElementType()
    {
        // Arrange
        var template = new FormTemplate();
        template.Elements.Add(new LabelElement { Text = "Label" });
        template.Elements.Add(new TextInputElement { Placeholder = "Input" });
        template.Elements.Add(new CheckboxElement { Label = "Check" });

        // Act
        var json = JsonSerializer.Serialize(template);
        var deserialized = JsonSerializer.Deserialize<FormTemplate>(json);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized!.Elements[0].Should().BeOfType<LabelElement>();
        deserialized.Elements[1].Should().BeOfType<TextInputElement>();
        deserialized.Elements[2].Should().BeOfType<CheckboxElement>();
    }

    [Fact]
    public void ComplexElement_SerializationPreservesAllProperties()
    {
        // Arrange
        var dropdown = new DropdownElement
        {
            X = 100,
            Y = 200,
            Width = 300,
            Height = 40,
            Placeholder = "Select...",
            DefaultValue = "Option 2",
            AllowMultiple = true,
            IsRequired = true
        };

        dropdown.Options.Clear();
        dropdown.Options.Add("Option 1");
        dropdown.Options.Add("Option 2");
        dropdown.Options.Add("Option 3");

        dropdown.Properties.FontSize = 20;
        dropdown.Properties.Bold = true;
        dropdown.Properties.Color = "#FF0000";

        // Act
        var json = JsonSerializer.Serialize(dropdown);
        var deserialized = JsonSerializer.Deserialize<DropdownElement>(json);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized!.X.Should().Be(100);
        deserialized.Y.Should().Be(200);
        deserialized.Placeholder.Should().Be("Select...");
        deserialized.DefaultValue.Should().Be("Option 2");
        deserialized.AllowMultiple.Should().BeTrue();
        deserialized.IsRequired.Should().BeTrue();
        deserialized.Options.Should().HaveCount(3);
        deserialized.Properties.FontSize.Should().Be(20);
        deserialized.Properties.Bold.Should().BeTrue();
        deserialized.Properties.Color.Should().Be("#FF0000");
    }

    #endregion

    #region ElementProperties Tests

    [Fact]
    public void ElementProperties_CanModifyAllProperties()
    {
        // Arrange
        var element = new LabelElement();

        // Act
        element.Properties.FontSize = 24;
        element.Properties.Bold = true;
        element.Properties.Italic = true;
        element.Properties.Underline = true;
        element.Properties.Color = "#FF0000";
        element.Properties.BackgroundColor = "#FFFF00";
        element.Properties.Alignment = TextAlignment.Center;
        element.Properties.PaddingTop = 10;
        element.Properties.MarginLeft = 5;
        element.Properties.BorderWidth = 2;
        element.Properties.BorderColor = "#000000";
        element.Properties.BorderRadius = 4;
        element.Properties.ZIndex = 10;

        // Assert
        element.Properties.FontSize.Should().Be(24);
        element.Properties.Bold.Should().BeTrue();
        element.Properties.Italic.Should().BeTrue();
        element.Properties.Underline.Should().BeTrue();
        element.Properties.Color.Should().Be("#FF0000");
        element.Properties.BackgroundColor.Should().Be("#FFFF00");
        element.Properties.Alignment.Should().Be(TextAlignment.Center);
        element.Properties.PaddingTop.Should().Be(10);
        element.Properties.MarginLeft.Should().Be(5);
        element.Properties.BorderWidth.Should().Be(2);
        element.Properties.BorderColor.Should().Be("#000000");
        element.Properties.BorderRadius.Should().Be(4);
        element.Properties.ZIndex.Should().Be(10);
    }

    #endregion
}
