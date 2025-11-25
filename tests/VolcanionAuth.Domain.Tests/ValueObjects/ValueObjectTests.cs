using FluentAssertions;
using VolcanionAuth.Domain.ValueObjects;
using Xunit;

namespace VolcanionAuth.Domain.Tests.ValueObjects;

public class EmailTests
{
    [Theory]
    [InlineData("test@example.com")]
    [InlineData("user.name@domain.co.uk")]
    [InlineData("test123@test-domain.com")]
    public void Create_WithValidEmail_ShouldReturnSuccess(string email)
    {
        // Act
        var result = Email.Create(email);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(email.ToLowerInvariant());
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("invalid-email")]
    [InlineData("@example.com")]
    [InlineData("test@")]
    public void Create_WithInvalidEmail_ShouldReturnFailure(string email)
    {
        // Act
        var result = Email.Create(email);

        // Assert
        result.IsFailure.Should().BeTrue();
    }
}

public class PasswordTests
{
    [Fact]
    public void Create_WithValidPassword_ShouldReturnSuccess()
    {
        // Arrange
        var password = "Test@123456";

        // Act
        var result = Password.Create(password);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Theory]
    [InlineData("short")] // Too short
    [InlineData("alllowercase123!")] // No uppercase
    [InlineData("ALLUPPERCASE123!")] // No lowercase
    [InlineData("NoNumbers!")] // No digits
    [InlineData("NoSpecial123")] // No special characters
    public void Create_WithInvalidPassword_ShouldReturnFailure(string password)
    {
        // Act
        var result = Password.Create(password);

        // Assert
        result.IsFailure.Should().BeTrue();
    }
}
