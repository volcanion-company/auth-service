using FluentAssertions;
using VolcanionAuth.Domain.Entities;
using VolcanionAuth.Domain.ValueObjects;
using Xunit;

namespace VolcanionAuth.Domain.Tests.Entities;

public class UserTests
{
    [Fact]
    public void Create_WithValidData_ShouldReturnSuccessResult()
    {
        // Arrange
        var email = Email.Create("test@example.com").Value;
        var password = Password.Create("Test@123456").Value;
        var fullName = FullName.Create("John", "Doe").Value;

        // Act
        var result = User.Create(email, password, fullName);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Email.Should().Be(email);
        result.Value.FullName.Should().Be(fullName);
        result.Value.IsActive.Should().BeTrue();
        result.Value.IsEmailVerified.Should().BeFalse();
    }

    [Fact]
    public void RecordSuccessfulLogin_WhenAccountActive_ShouldResetFailedAttempts()
    {
        // Arrange
        var user = CreateTestUser();
        
        // Act
        var result = user.RecordSuccessfulLogin("192.168.1.1", "Mozilla/5.0");

        // Assert
        result.IsSuccess.Should().BeTrue();
        user.FailedLoginAttempts.Should().Be(0);
        user.IsLocked.Should().BeFalse();
        user.LastLoginAt.Should().NotBeNull();
    }

    [Fact]
    public void RecordFailedLogin_AfterFiveAttempts_ShouldLockAccount()
    {
        // Arrange
        var user = CreateTestUser();

        // Act
        for (int i = 0; i < 5; i++)
        {
            user.RecordFailedLogin("192.168.1.1", "Mozilla/5.0");
        }

        // Assert
        user.IsLocked.Should().BeTrue();
        user.LockedUntil.Should().NotBeNull();
        user.FailedLoginAttempts.Should().Be(5);
    }

    [Fact]
    public void AddRole_WhenRoleNotExists_ShouldAddRole()
    {
        // Arrange
        var user = CreateTestUser();
        var roleId = Guid.NewGuid();

        // Act
        user.AddRole(roleId);

        // Assert
        user.UserRoles.Should().HaveCount(1);
        user.UserRoles.First().RoleId.Should().Be(roleId);
    }

    [Fact]
    public void VerifyEmail_WhenNotVerified_ShouldSetVerifiedTrue()
    {
        // Arrange
        var user = CreateTestUser();

        // Act
        var result = user.VerifyEmail();

        // Assert
        result.IsSuccess.Should().BeTrue();
        user.IsEmailVerified.Should().BeTrue();
    }

    private static User CreateTestUser()
    {
        var email = Email.Create("test@example.com").Value;
        var password = Password.Create("Test@123456").Value;
        var fullName = FullName.Create("John", "Doe").Value;
        return User.Create(email, password, fullName).Value;
    }
}
