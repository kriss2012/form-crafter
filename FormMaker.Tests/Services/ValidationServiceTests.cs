using FluentAssertions;
using FormMaker.Client.Services;
using FormMaker.Shared.Models;
using FormMaker.Shared.Models.Elements;
using Xunit;

namespace FormMaker.Tests.Services;

public class ValidationServiceTests
{
    private readonly ValidationService _service;

    public ValidationServiceTests()
    {
        _service = new ValidationService();
    }

    #region Required Field Validation

    [Fact]
    public void ValidateElement_RequiredField_WithValue_Passes()
    {
        // Arrange
        var element = new TextInputElement
        {
            IsRequired = true,
            Label = "Name"
        };

        // Act
        var result = _service.ValidateElement(element, "John Doe");

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void ValidateElement_RequiredField_WithEmptyValue_Fails()
    {
        // Arrange
        var element = new TextInputElement
        {
            IsRequired = true,
            Label = "Name"
        };

        // Act
        var result = _service.ValidateElement(element, "");

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors[0].Should().Contain("Name is required");
    }

    [Fact]
    public void ValidateElement_RequiredField_WithNull_Fails()
    {
        // Arrange
        var element = new TextInputElement
        {
            IsRequired = true,
            Label = "Email"
        };

        // Act
        var result = _service.ValidateElement(element, null);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    public void ValidateElement_RequiredField_WithCustomErrorMessage_UsesCustomMessage()
    {
        // Arrange
        var element = new TextInputElement
        {
            IsRequired = true,
            Label = "Password",
            RequiredErrorMessage = "Please provide a password"
        };

        // Act
        var result = _service.ValidateElement(element, null);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors[0].Should().Be("Please provide a password");
    }

    #endregion

    #region Email Validation

    [Fact]
    public void ValidateElement_EmailInput_WithValidEmail_Passes()
    {
        // Arrange
        var element = new TextInputElement
        {
            InputType = "email"
        };

        // Act
        var result = _service.ValidateElement(element, "test@example.com");

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void ValidateElement_EmailInput_WithInvalidEmail_Fails()
    {
        // Arrange
        var element = new TextInputElement
        {
            InputType = "email"
        };

        // Act
        var result = _service.ValidateElement(element, "invalid-email");

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("Please enter a valid email address");
    }

    [Fact]
    public void ValidateElement_EmailValidationRule_WithInvalidEmail_Fails()
    {
        // Arrange
        var element = new TextInputElement();
        element.ValidationRules.Add(new ValidationRule
        {
            Type = ValidationType.Email
        });

        // Act
        var result = _service.ValidateElement(element, "not@email");

        // Assert
        result.IsValid.Should().BeFalse();
    }

    #endregion

    #region Phone Validation

    [Fact]
    public void ValidateElement_PhoneValidationRule_WithValidPhone_Passes()
    {
        // Arrange
        var element = new TextInputElement();
        element.ValidationRules.Add(new ValidationRule
        {
            Type = ValidationType.Phone
        });

        // Act
        var result = _service.ValidateElement(element, "(555) 123-4567");

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void ValidateElement_PhoneInput_WithInvalidPhone_Fails()
    {
        // Arrange
        var element = new TextInputElement
        {
            InputType = "tel"
        };

        // Act
        var result = _service.ValidateElement(element, "abc-defg");

        // Assert
        result.IsValid.Should().BeFalse();
    }

    #endregion

    #region URL Validation

    [Fact]
    public void ValidateElement_UrlInput_WithValidUrl_Passes()
    {
        // Arrange
        var element = new TextInputElement
        {
            InputType = "url"
        };

        // Act
        var result = _service.ValidateElement(element, "https://www.example.com");

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void ValidateElement_UrlInput_WithInvalidUrl_Fails()
    {
        // Arrange
        var element = new TextInputElement
        {
            InputType = "url"
        };

        // Act
        var result = _service.ValidateElement(element, "not a url");

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("Please enter a valid URL");
    }

    #endregion

    #region Number Validation

    [Fact]
    public void ValidateElement_NumberInput_WithValidNumber_Passes()
    {
        // Arrange
        var element = new TextInputElement
        {
            InputType = "number"
        };

        // Act
        var result = _service.ValidateElement(element, "123.45");

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void ValidateElement_NumberInput_WithInvalidNumber_Fails()
    {
        // Arrange
        var element = new TextInputElement
        {
            InputType = "number"
        };

        // Act
        var result = _service.ValidateElement(element, "abc");

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("Please enter a valid number");
    }

    #endregion

    #region Length Validation

    [Fact]
    public void ValidateElement_MinLength_WithValidLength_Passes()
    {
        // Arrange
        var element = new TextInputElement();
        element.ValidationRules.Add(new ValidationRule
        {
            Type = ValidationType.MinLength,
            MinValue = 5,
            ErrorMessage = "Too short"
        });

        // Act
        var result = _service.ValidateElement(element, "HelloWorld");

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void ValidateElement_MinLength_WithInvalidLength_Fails()
    {
        // Arrange
        var element = new TextInputElement();
        element.ValidationRules.Add(new ValidationRule
        {
            Type = ValidationType.MinLength,
            MinValue = 10
        });

        // Act
        var result = _service.ValidateElement(element, "Short");

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("Minimum length is 10 characters");
    }

    [Fact]
    public void ValidateElement_MaxLength_WithValidLength_Passes()
    {
        // Arrange
        var element = new TextInputElement
        {
            MaxLength = 10
        };

        // Act
        var result = _service.ValidateElement(element, "Hello");

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void ValidateElement_MaxLength_WithInvalidLength_Fails()
    {
        // Arrange
        var element = new TextInputElement
        {
            MaxLength = 5
        };

        // Act
        var result = _service.ValidateElement(element, "TooLongText");

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("Maximum length is 5 characters");
    }

    #endregion

    #region Range Validation

    [Fact]
    public void ValidateElement_Range_WithValueInRange_Passes()
    {
        // Arrange
        var element = new TextInputElement();
        element.ValidationRules.Add(new ValidationRule
        {
            Type = ValidationType.Range,
            MinValue = 1,
            MaxValue = 100
        });

        // Act
        var result = _service.ValidateElement(element, "50");

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void ValidateElement_Range_WithValueBelowMin_Fails()
    {
        // Arrange
        var element = new TextInputElement();
        element.ValidationRules.Add(new ValidationRule
        {
            Type = ValidationType.Range,
            MinValue = 10
        });

        // Act
        var result = _service.ValidateElement(element, "5");

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("Value must be at least 10");
    }

    [Fact]
    public void ValidateElement_Range_WithValueAboveMax_Fails()
    {
        // Arrange
        var element = new TextInputElement();
        element.ValidationRules.Add(new ValidationRule
        {
            Type = ValidationType.Range,
            MaxValue = 100
        });

        // Act
        var result = _service.ValidateElement(element, "150");

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("Value must be at most 100");
    }

    #endregion

    #region Pattern Validation

    [Fact]
    public void ValidateElement_Pattern_WithMatchingValue_Passes()
    {
        // Arrange
        var element = new TextInputElement();
        element.ValidationRules.Add(new ValidationRule
        {
            Type = ValidationType.Pattern,
            Pattern = @"^\d{5}$", // 5-digit zip code
            ErrorMessage = "Invalid zip code"
        });

        // Act
        var result = _service.ValidateElement(element, "12345");

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void ValidateElement_Pattern_WithNonMatchingValue_Fails()
    {
        // Arrange
        var element = new TextInputElement();
        element.ValidationRules.Add(new ValidationRule
        {
            Type = ValidationType.Pattern,
            Pattern = @"^\d{5}$",
            ErrorMessage = "Invalid zip code"
        });

        // Act
        var result = _service.ValidateElement(element, "ABCDE");

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("Invalid zip code");
    }

    #endregion

    #region TextArea Validation

    [Fact]
    public void ValidateElement_TextArea_WithMaxLength_Passes()
    {
        // Arrange
        var element = new TextAreaElement
        {
            MaxLength = 100
        };

        // Act
        var result = _service.ValidateElement(element, "Short text");

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void ValidateElement_TextArea_ExceedsMaxLength_Fails()
    {
        // Arrange
        var element = new TextAreaElement
        {
            MaxLength = 10
        };

        // Act
        var result = _service.ValidateElement(element, "This text is way too long");

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("Maximum length is 10 characters");
    }

    #endregion

    #region DatePicker Validation

    [Fact]
    public void ValidateElement_DatePicker_WithinRange_Passes()
    {
        // Arrange
        var element = new DatePickerElement
        {
            MinDate = DateTime.Parse("2024-01-01"),
            MaxDate = DateTime.Parse("2024-12-31")
        };

        // Act
        var result = _service.ValidateElement(element, DateTime.Parse("2024-06-15"));

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void ValidateElement_DatePicker_BeforeMinDate_Fails()
    {
        // Arrange
        var element = new DatePickerElement
        {
            MinDate = DateTime.Parse("2024-01-01")
        };

        // Act
        var result = _service.ValidateElement(element, DateTime.Parse("2023-12-31"));

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("must be after"));
    }

    [Fact]
    public void ValidateElement_DatePicker_AfterMaxDate_Fails()
    {
        // Arrange
        var element = new DatePickerElement
        {
            MaxDate = DateTime.Parse("2024-12-31")
        };

        // Act
        var result = _service.ValidateElement(element, DateTime.Parse("2025-01-01"));

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("must be before"));
    }

    #endregion

    #region Form Validation

    [Fact]
    public void ValidateForm_AllFieldsValid_Passes()
    {
        // Arrange
        var template = new FormTemplate();
        template.Elements.Add(new TextInputElement
        {
            Id = Guid.NewGuid(),
            IsRequired = true,
            Label = "Name"
        });
        template.Elements.Add(new TextInputElement
        {
            Id = Guid.NewGuid(),
            InputType = "email",
            Label = "Email"
        });

        var formValues = new Dictionary<Guid, object?>
        {
            { template.Elements[0].Id, "John Doe" },
            { template.Elements[1].Id, "john@example.com" }
        };

        // Act
        var result = _service.ValidateForm(template, formValues);

        // Assert
        result.IsValid.Should().BeTrue();
        result.ErrorCount.Should().Be(0);
    }

    [Fact]
    public void ValidateForm_WithErrors_ReturnsAllErrors()
    {
        // Arrange
        var template = new FormTemplate();
        var nameElement = new TextInputElement
        {
            Id = Guid.NewGuid(),
            IsRequired = true,
            Label = "Name"
        };
        var emailElement = new TextInputElement
        {
            Id = Guid.NewGuid(),
            InputType = "email",
            IsRequired = true,
            Label = "Email"
        };

        template.Elements.Add(nameElement);
        template.Elements.Add(emailElement);

        var formValues = new Dictionary<Guid, object?>
        {
            { nameElement.Id, "" },
            { emailElement.Id, "invalid-email" }
        };

        // Act
        var result = _service.ValidateForm(template, formValues);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorCount.Should().BeGreaterThanOrEqualTo(2);
        result.GetErrorsForElement(nameElement.Id).Should().NotBeEmpty();
        result.GetErrorsForElement(emailElement.Id).Should().NotBeEmpty();
    }

    [Fact]
    public void ValidateForm_SkipsNonValidatableElements()
    {
        // Arrange
        var template = new FormTemplate();
        template.Elements.Add(new LabelElement { Id = Guid.NewGuid() });
        template.Elements.Add(new DividerElement { Id = Guid.NewGuid() });
        template.Elements.Add(new TextInputElement
        {
            Id = Guid.NewGuid(),
            IsRequired = true
        });

        var formValues = new Dictionary<Guid, object?>
        {
            { template.Elements[2].Id, "Valid value" }
        };

        // Act
        var result = _service.ValidateForm(template, formValues);

        // Assert
        result.IsValid.Should().BeTrue();
        result.ElementResults.Should().HaveCount(1); // Only TextInput
    }

    #endregion

    #region Multiple Validation Rules

    [Fact]
    public void ValidateElement_MultipleRules_AllPass_Passes()
    {
        // Arrange
        var element = new TextInputElement
        {
            IsRequired = true
        };
        element.ValidationRules.Add(new ValidationRule
        {
            Type = ValidationType.MinLength,
            MinValue = 5
        });
        element.ValidationRules.Add(new ValidationRule
        {
            Type = ValidationType.MaxLength,
            MaxValue = 20
        });

        // Act
        var result = _service.ValidateElement(element, "ValidValue");

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void ValidateElement_MultipleRules_SomeFail_ReturnsAllErrors()
    {
        // Arrange
        var element = new TextInputElement
        {
            IsRequired = true
        };
        element.ValidationRules.Add(new ValidationRule
        {
            Type = ValidationType.MinLength,
            MinValue = 10
        });
        element.ValidationRules.Add(new ValidationRule
        {
            Type = ValidationType.Email
        });

        // Act
        var result = _service.ValidateElement(element, "short");

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(2); // MinLength and Email
    }

    #endregion

    #region Optional Fields

    [Fact]
    public void ValidateElement_OptionalField_WithEmptyValue_Passes()
    {
        // Arrange
        var element = new TextInputElement
        {
            IsRequired = false,
            InputType = "email"
        };

        // Act
        var result = _service.ValidateElement(element, "");

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void ValidateElement_OptionalField_WithInvalidValue_Fails()
    {
        // Arrange
        var element = new TextInputElement
        {
            IsRequired = false,
            InputType = "email"
        };

        // Act
        var result = _service.ValidateElement(element, "invalid");

        // Assert
        result.IsValid.Should().BeFalse();
    }

    #endregion
}
