using FluentAssertions;
using Moq;
using VolcanionAuth.Application.Common.Interfaces;
using VolcanionAuth.Application.Features.Authentication.Commands.RegisterUser;
using VolcanionAuth.Domain.Entities;
using VolcanionAuth.Domain.ValueObjects;
using Xunit;

namespace VolcanionAuth.Application.Tests.Features.Authentication;

public class RegisterUserCommandHandlerTests
{
    private readonly Mock<IRepository<User>> _userRepositoryMock;
    private readonly Mock<IReadRepository<User>> _readRepositoryMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly RegisterUserCommandHandler _handler;

    public RegisterUserCommandHandlerTests()
    {
        _userRepositoryMock = new Mock<IRepository<User>>();
        _readRepositoryMock = new Mock<IReadRepository<User>>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _handler = new RegisterUserCommandHandler(
            _userRepositoryMock.Object,
            _readRepositoryMock.Object,
            _passwordHasherMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldRegisterUser()
    {
        // Arrange
        var command = new RegisterUserCommand(
            "test@example.com",
            "Test@123456",
            "John",
            "Doe");

        _readRepositoryMock
            .Setup(r => r.GetUserByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        _passwordHasherMock
            .Setup(h => h.HashPassword(It.IsAny<string>()))
            .Returns("hashed_password");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Email.Should().Be("test@example.com");
        _userRepositoryMock.Verify(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenEmailExists_ShouldReturnFailure()
    {
        // Arrange
        var command = new RegisterUserCommand(
            "existing@example.com",
            "Test@123456",
            "John",
            "Doe");

        var existingUser = User.Create(
            Email.Create("existing@example.com").Value,
            Password.CreateFromHash("hash"),
            FullName.Create("Jane", "Doe").Value).Value;

        _readRepositoryMock
            .Setup(r => r.GetUserByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Email already registered.");
    }
}
