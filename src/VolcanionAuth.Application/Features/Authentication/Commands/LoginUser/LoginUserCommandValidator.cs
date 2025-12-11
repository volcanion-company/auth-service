using FluentValidation;

namespace VolcanionAuth.Application.Features.Authentication.Commands.LoginUser;

/// <summary>
/// Provides validation rules for the <see cref="LoginUserCommand"/> to ensure that user login requests contain a valid
/// email address and a non-empty password.
/// </summary>
/// <remarks>This validator enforces that the email field is not empty and conforms to a valid email format, and
/// that the password field is not empty. Use this class to validate login requests before processing authentication.
/// This class is typically used with FluentValidation in command handling scenarios.</remarks>
public class LoginUserCommandValidator : AbstractValidator<LoginUserCommand>
{
    /// <summary>
    /// Initializes a new instance of the LoginUserCommandValidator class, which defines validation rules for user login
    /// commands.
    /// </summary>
    /// <remarks>This validator enforces that the email and password fields are not empty and that the email
    /// is in a valid format. Use this class to ensure that login requests meet the required criteria before
    /// processing.</remarks>
    public LoginUserCommandValidator()
    {
        // Define validation rules for Email and Password
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email format is invalid.");
        // Define validation rule for Password
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.");
    }
}
