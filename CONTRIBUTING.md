# Contributing to Volcanion Auth Service

First off, thank you for considering contributing to Volcanion Auth Service! It's people like you that make this project better for everyone.

## 📋 Table of Contents

- [Code of Conduct](#code-of-conduct)
- [Getting Started](#getting-started)
- [How Can I Contribute?](#how-can-i-contribute)
- [Development Workflow](#development-workflow)
- [Coding Standards](#coding-standards)
- [Commit Guidelines](#commit-guidelines)
- [Pull Request Process](#pull-request-process)
- [Testing Requirements](#testing-requirements)
- [Documentation Guidelines](#documentation-guidelines)

## 📜 Code of Conduct

### Our Pledge

We are committed to providing a welcoming and inclusive environment for all contributors, regardless of experience level, gender, gender identity and expression, sexual orientation, disability, personal appearance, body size, race, ethnicity, age, religion, or nationality.

### Our Standards

**Examples of behavior that contributes to a positive environment:**

- Using welcoming and inclusive language
- Being respectful of differing viewpoints and experiences
- Gracefully accepting constructive criticism
- Focusing on what is best for the community
- Showing empathy towards other community members

**Examples of unacceptable behavior:**

- Trolling, insulting/derogatory comments, and personal or political attacks
- Public or private harassment
- Publishing others' private information without explicit permission
- Other conduct which could reasonably be considered inappropriate in a professional setting

### Enforcement

Instances of abusive, harassing, or otherwise unacceptable behavior may be reported by contacting the project team at support@volcanion.company. All complaints will be reviewed and investigated promptly and fairly.

## 🚀 Getting Started

### Prerequisites

Before you begin, ensure you have the following installed:

- **.NET 9.0 SDK** or later
- **PostgreSQL 14+**
- **Redis 7+**
- **Git**
- **Docker Desktop** (optional, but recommended)
- **Visual Studio 2022** or **VS Code** with C# extension

### Initial Setup

1. **Fork the repository**

   Click the "Fork" button at the top right of the repository page.

2. **Clone your fork**

   ```bash
   git clone https://github.com/YOUR_USERNAME/auth-service.git
   cd auth-service
   ```

3. **Add upstream remote**

   ```bash
   git remote add upstream https://github.com/volcanion-company/auth-service.git
   ```

4. **Install dependencies**

   ```bash
   dotnet restore
   ```

5. **Set up the database**

   ```bash
   # Using Docker
   docker-compose up -d postgres-primary redis

   # Or manually configure PostgreSQL and Redis
   ```

6. **Run migrations**

   ```bash
   cd src/VolcanionAuth.API
   dotnet ef database update
   ```

7. **Run the application**

   ```bash
   dotnet run
   ```

8. **Verify setup**

   Navigate to `http://localhost:5000/swagger` to see the API documentation.

## 🤝 How Can I Contribute?

### Reporting Bugs

Before creating bug reports, please check the [issue tracker](https://github.com/volcanion-company/auth-service/issues) to avoid duplicates.

**When submitting a bug report, include:**

- **Clear descriptive title**
- **Detailed description** of the issue
- **Steps to reproduce** the behavior
- **Expected behavior**
- **Actual behavior**
- **Screenshots** (if applicable)
- **Environment details**:
  - OS: [e.g., Windows 11, macOS 14, Ubuntu 22.04]
  - .NET Version: [e.g., 9.0.0]
  - Database Version: [e.g., PostgreSQL 16.1]
  - Redis Version: [e.g., 7.2.3]

**Example Bug Report:**

```markdown
### Bug: User email verification fails silently

**Description:**
When a user attempts to verify their email, the API returns 200 OK but the user's `IsEmailVerified` field remains `false`.

**Steps to Reproduce:**
1. Register a new user via `POST /api/v1/auth/register`
2. Call `POST /api/v1/auth/verify-email` with the verification token
3. Check user in database

**Expected Behavior:**
`IsEmailVerified` should be set to `true`

**Actual Behavior:**
`IsEmailVerified` remains `false`

**Environment:**
- OS: Windows 11
- .NET: 9.0.0
- Database: PostgreSQL 16.1
```

### Suggesting Enhancements

Enhancement suggestions are tracked as GitHub issues. When creating an enhancement suggestion:

- **Use a clear and descriptive title**
- **Provide a detailed description** of the proposed feature
- **Explain why this enhancement would be useful**
- **List any alternatives** you've considered
- **Include mockups or examples** if applicable

### Your First Code Contribution

Unsure where to start? Look for issues labeled:

- `good first issue` - Simple issues suitable for beginners
- `help wanted` - Issues where we need community help
- `documentation` - Documentation improvements

## 🔄 Development Workflow

### Branch Strategy

We use **Git Flow** branching model:

- `master` - Production-ready code
- `develop` - Integration branch for features
- `feature/*` - New features
- `bugfix/*` - Bug fixes
- `hotfix/*` - Urgent production fixes
- `release/*` - Release preparation

### Creating a Feature Branch

```bash
# Update your local repository
git checkout develop
git pull upstream develop

# Create a feature branch
git checkout -b feature/your-feature-name

# Example
git checkout -b feature/add-two-factor-auth
```

### Making Changes

1. **Write code** following our [coding standards](#coding-standards)
2. **Add tests** for your changes
3. **Update documentation** if needed
4. **Run tests** to ensure nothing breaks
5. **Commit your changes** following our [commit guidelines](#commit-guidelines)

### Syncing with Upstream

```bash
# Fetch upstream changes
git fetch upstream

# Merge upstream develop into your branch
git checkout develop
git merge upstream/develop

# Rebase your feature branch
git checkout feature/your-feature-name
git rebase develop
```

## 💻 Coding Standards

### C# Style Guide

We follow the [Microsoft C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions).

#### Naming Conventions

```csharp
// PascalCase for classes, methods, properties, constants
public class UserService { }
public void RegisterUser() { }
public string FirstName { get; set; }
public const int MaxLoginAttempts = 5;

// camelCase for local variables and parameters
var userId = Guid.NewGuid();
public void Login(string email, string password) { }

// Prefix interfaces with 'I'
public interface IUserRepository { }

// Suffix async methods with 'Async'
public async Task<User> GetUserAsync(Guid id) { }
```

#### Code Structure

```csharp
// ✅ Good: Single responsibility, clear intent
public class UserRegistrationService
{
    private readonly IUserRepository _userRepository;
    private readonly IEmailService _emailService;

    public UserRegistrationService(
        IUserRepository userRepository,
        IEmailService emailService)
    {
        _userRepository = userRepository;
        _emailService = emailService;
    }

    public async Task<Result<User>> RegisterAsync(
        string email, 
        string password)
    {
        // Validation
        if (string.IsNullOrWhiteSpace(email))
            return Result<User>.Failure("Email is required");

        // Business logic
        var user = User.Create(email, password);
        await _userRepository.AddAsync(user);
        await _emailService.SendVerificationEmailAsync(user);

        return Result<User>.Success(user);
    }
}

// ❌ Bad: Multiple responsibilities, unclear
public class UserService
{
    public void DoEverything(string data)
    {
        // Don't do this
    }
}
```

#### LINQ and Async

```csharp
// ✅ Good: Async all the way
public async Task<List<User>> GetActiveUsersAsync()
{
    return await _context.Users
        .Where(u => u.IsActive)
        .OrderBy(u => u.CreatedAt)
        .ToListAsync();
}

// ❌ Bad: Blocking async code
public List<User> GetActiveUsers()
{
    return _context.Users
        .Where(u => u.IsActive)
        .ToList(); // Synchronous
}
```

### Project Structure Standards

#### Feature Folder Structure

```
Features/
└── Authentication/
    ├── Commands/
    │   ├── RegisterUserCommand.cs
    │   ├── RegisterUserCommandHandler.cs
    │   └── RegisterUserCommandValidator.cs
    ├── Queries/
    │   ├── GetUserQuery.cs
    │   └── GetUserQueryHandler.cs
    └── DTOs/
        ├── UserDto.cs
        └── LoginResponseDto.cs
```

#### Clean Architecture Rules

1. **Domain Layer** should have NO dependencies on other layers
2. **Application Layer** can only reference Domain
3. **Infrastructure Layer** can reference Domain and Application
4. **API Layer** can reference all layers but contains no business logic

### Error Handling

```csharp
// ✅ Good: Return Result pattern
public async Task<Result<User>> GetUserAsync(Guid id)
{
    var user = await _repository.GetByIdAsync(id);
    
    if (user is null)
        return Result<User>.Failure("User not found");
    
    return Result<User>.Success(user);
}

// ✅ Good: Throw domain exceptions
public class User
{
    public void ChangeEmail(string newEmail)
    {
        if (string.IsNullOrWhiteSpace(newEmail))
            throw new DomainException("Email cannot be empty");
        
        Email = newEmail;
    }
}

// ❌ Bad: Returning null
public async Task<User> GetUserAsync(Guid id)
{
    return await _repository.GetByIdAsync(id); // Don't return null
}
```

### Dependency Injection

```csharp
// ✅ Good: Constructor injection
public class UserService
{
    private readonly IUserRepository _repository;
    private readonly ILogger<UserService> _logger;

    public UserService(
        IUserRepository repository,
        ILogger<UserService> logger)
    {
        _repository = repository;
        _logger = logger;
    }
}

// ❌ Bad: Service locator pattern
public class UserService
{
    public void DoSomething()
    {
        var repository = ServiceLocator.Get<IUserRepository>();
    }
}
```

## 📝 Commit Guidelines

We follow [Conventional Commits](https://www.conventionalcommits.org/) specification.

### Commit Message Format

```
<type>(<scope>): <subject>

<body>

<footer>
```

### Types

- `feat`: New feature
- `fix`: Bug fix
- `docs`: Documentation changes
- `style`: Code style changes (formatting, missing semicolons, etc.)
- `refactor`: Code refactoring
- `perf`: Performance improvements
- `test`: Adding or updating tests
- `chore`: Maintenance tasks
- `ci`: CI/CD changes
- `build`: Build system changes

### Examples

```bash
# Feature
feat(auth): add two-factor authentication support

Implement TOTP-based 2FA with QR code generation.
Users can now enable 2FA in their profile settings.

Closes #123

# Bug fix
fix(permissions): resolve null reference in permission checker

The permission checker was throwing NullReferenceException
when user had no roles assigned.

Fixes #456

# Documentation
docs(readme): update installation instructions

Added Docker setup section and troubleshooting guide.

# Refactoring
refactor(domain): extract password validation to value object

Moved password validation logic from User entity to
HashedPassword value object for better encapsulation.

# Performance
perf(cache): implement Redis pipeline for bulk operations

Reduced cache operations from N calls to 1 batched call,
improving performance by 70%.
```

### Commit Message Rules

1. **Limit subject line to 50 characters**
2. **Capitalize the subject line**
3. **Do not end subject with a period**
4. **Use imperative mood** ("add" not "added" or "adds")
5. **Wrap body at 72 characters**
6. **Use body to explain what and why, not how**

## 🔀 Pull Request Process

### Before Submitting

- [ ] Code follows our coding standards
- [ ] All tests pass (`dotnet test`)
- [ ] New tests added for new features
- [ ] Documentation updated (if applicable)
- [ ] No merge conflicts with `develop` branch
- [ ] Commit messages follow conventions
- [ ] Self-review completed

### PR Template

```markdown
## Description
Brief description of the changes

## Type of Change
- [ ] Bug fix (non-breaking change which fixes an issue)
- [ ] New feature (non-breaking change which adds functionality)
- [ ] Breaking change (fix or feature that would cause existing functionality to not work as expected)
- [ ] Documentation update

## How Has This Been Tested?
- [ ] Unit tests
- [ ] Integration tests
- [ ] Manual testing

## Checklist
- [ ] My code follows the style guidelines
- [ ] I have performed a self-review
- [ ] I have commented my code, particularly in hard-to-understand areas
- [ ] I have made corresponding changes to the documentation
- [ ] My changes generate no new warnings
- [ ] I have added tests that prove my fix is effective or that my feature works
- [ ] New and existing unit tests pass locally with my changes

## Screenshots (if applicable)

## Related Issues
Closes #123
```

### Review Process

1. **Submit PR** against `develop` branch
2. **Automated checks** run (build, tests, code quality)
3. **Code review** by maintainers (at least 1 approval required)
4. **Address feedback** if requested
5. **Merge** once approved

### PR Review Criteria

Reviewers will check for:

- **Functionality**: Does it work as intended?
- **Code Quality**: Is it clean, readable, and maintainable?
- **Tests**: Are there adequate tests?
- **Performance**: Any performance concerns?
- **Security**: Any security vulnerabilities?
- **Documentation**: Is documentation updated?

## 🧪 Testing Requirements

### Test Coverage

- **Minimum 75% code coverage** for new code
- **100% coverage** for critical business logic (authentication, authorization)

### Test Types

#### Unit Tests

```csharp
[Fact]
public async Task RegisterUser_WithValidData_ShouldSucceed()
{
    // Arrange
    var command = new RegisterUserCommand
    {
        Email = "test@example.com",
        Password = "SecureP@ss123",
        FirstName = "John",
        LastName = "Doe"
    };

    // Act
    var result = await _handler.Handle(command, CancellationToken.None);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Data.Email.Should().Be(command.Email);
}

[Fact]
public async Task RegisterUser_WithExistingEmail_ShouldFail()
{
    // Arrange
    var command = new RegisterUserCommand
    {
        Email = "existing@example.com",
        Password = "SecureP@ss123"
    };

    _mockRepository
        .Setup(x => x.ExistsAsync(It.IsAny<string>()))
        .ReturnsAsync(true);

    // Act
    var result = await _handler.Handle(command, CancellationToken.None);

    // Assert
    result.IsSuccess.Should().BeFalse();
    result.Error.Should().Contain("already exists");
}
```

#### Integration Tests

```csharp
public class AuthControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task Login_WithValidCredentials_ReturnsToken()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = "Password123!"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
        result.AccessToken.Should().NotBeNullOrEmpty();
    }
}
```

### Running Tests

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover

# Run specific test project
dotnet test tests/VolcanionAuth.Application.Tests

# Run tests with filter
dotnet test --filter "Category=Unit"
```

## 📚 Documentation Guidelines

### Code Documentation

```csharp
/// <summary>
/// Registers a new user in the system.
/// </summary>
/// <param name="command">The registration command containing user details.</param>
/// <param name="cancellationToken">Cancellation token.</param>
/// <returns>A result containing the created user or an error message.</returns>
/// <exception cref="DomainException">Thrown when email is already registered.</exception>
public async Task<Result<UserDto>> Handle(
    RegisterUserCommand command, 
    CancellationToken cancellationToken)
{
    // Implementation
}
```

### API Documentation

Update Swagger documentation for new endpoints:

```csharp
[HttpPost("register")]
[ProducesResponseType(typeof(UserDto), StatusCodes.Status201Created)]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
[SwaggerOperation(
    Summary = "Register a new user",
    Description = "Creates a new user account and sends verification email",
    Tags = new[] { "Authentication" }
)]
public async Task<IActionResult> Register([FromBody] RegisterUserCommand command)
{
    // Implementation
}
```

### README Updates

When adding features, update:

- Feature list
- API endpoints
- Configuration examples
- Installation steps (if needed)

## 🏆 Recognition

Contributors will be recognized in:

- **CONTRIBUTORS.md** file
- **GitHub Contributors** page
- **Release notes** for significant contributions

## 💬 Questions?

- **GitHub Discussions**: For general questions
- **GitHub Issues**: For bug reports and feature requests
- **Email**: support@volcanion.company

## 📄 License

By contributing, you agree that your contributions will be licensed under the MIT License.

---

**Thank you for contributing to Volcanion Auth Service! 🎉**
