using FluentValidation;

namespace VolcanionAuth.Application.Features.Authentication.Commands.RegisterUser;

/// <summary>
/// Provides validation rules for the RegisterUserCommand, ensuring that user registration data meets required criteria.
/// </summary>
/// <remarks>This validator enforces constraints on email, password, first name, and last name fields, such as
/// required formats and length limits. Use this class to validate user input before processing registration requests.
/// The validation rules are designed to help prevent common input errors and improve data integrity.</remarks>
public class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    /// <summary>
    /// Initializes a new instance of the RegisterUserCommandValidator class with validation rules for user registration
    /// fields.
    /// </summary>
    /// <remarks>The validator enforces requirements for email, password, first name, and last name fields to
    /// ensure that user registration data meets expected formats and constraints. This includes checks for non-empty
    /// values, valid email format, password complexity, and maximum lengths for names.</remarks>
    public RegisterUserCommandValidator()
    {
        // Email validation
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email format is invalid.");
        // Password validation
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters.")
            .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter.")
            .Matches(@"[0-9]").WithMessage("Password must contain at least one digit.")
            .Matches(@"[!@#$%^&*()_+\-=\[\]{}|;:,.<>?]").WithMessage("Password must contain at least one special character.");
        // First name validation
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .MaximumLength(50).WithMessage("First name cannot exceed 50 characters.");
        // Last name validation
        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required.")
            .MaximumLength(50).WithMessage("Last name cannot exceed 50 characters.");
    }
}
