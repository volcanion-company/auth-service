using VolcanionAuth.Application.Common.Interfaces;
using VolcanionAuth.Domain.Entities;
using VolcanionAuth.Domain.ValueObjects;

namespace VolcanionAuth.Application.Features.Authentication.Commands.RegisterUser;

/// <summary>
/// Handles the registration of a new user by processing a RegisterUserCommand and returning the result of the
/// operation.
/// </summary>
/// <remarks>This handler validates user input, ensures the email address is not already registered, securely
/// hashes the password, and persists the new user. It returns a result indicating success or failure, including error
/// details if registration cannot be completed. The operation is performed asynchronously and is suitable for use in
/// CQRS or MediatR-based architectures.</remarks>
/// <param name="userRepository">The repository used to add new User entities to persistent storage.</param>
/// <param name="readRepository">The read-only repository used to query existing User entities, such as checking for duplicate email addresses.</param>
/// <param name="passwordHasher">The service used to securely hash user passwords before storing them.</param>
/// <param name="unitOfWork">The unit of work used to commit changes to the data store as part of the registration process.</param>
public class RegisterUserCommandHandler(
    IRepository<User> userRepository,
    IReadRepository<User> readRepository,
    IPasswordHasher passwordHasher,
    IUnitOfWork unitOfWork) : IRequestHandler<RegisterUserCommand, Result<RegisterUserResponse>>
{
    /// <summary>
    /// Handles the registration of a new user by validating input, checking for existing accounts, and creating the
    /// user record.
    /// </summary>
    /// <remarks>Returns a failure result if the email is already registered or if any input validation fails.
    /// The operation is performed asynchronously and persists the new user to the database upon success.</remarks>
    /// <param name="request">The command containing user registration details, including email, password, first name, and last name.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the registration operation.</param>
    /// <returns>A result containing the registration response if successful; otherwise, a failure result with an error message.</returns>
    public async Task<Result<RegisterUserResponse>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        // Check if email already exists
        var existingUser = await readRepository.GetUserByEmailAsync(request.Email, cancellationToken);
        if (existingUser != null)
        {
            return Result.Failure<RegisterUserResponse>("Email already registered.");
        }

        // Create value objects
        var emailResult = Email.Create(request.Email);
        if (emailResult.IsFailure)
        {
            // Invalid email format
            return Result.Failure<RegisterUserResponse>(emailResult.Error);
        }

        // Validate password
        var passwordResult = Password.Create(request.Password);
        if (passwordResult.IsFailure)
        {
            return Result.Failure<RegisterUserResponse>(passwordResult.Error);
        }

        var fullNameResult = FullName.Create(request.FirstName, request.LastName);
        if (fullNameResult.IsFailure)
        {
            return Result.Failure<RegisterUserResponse>(fullNameResult.Error);
        }

        // Hash password
        var hashedPassword = passwordHasher.HashPassword(request.Password);
        var password = Password.CreateFromHash(hashedPassword);

        // Create user
        var userResult = User.Create(emailResult.Value, password, fullNameResult.Value);
        if (userResult.IsFailure)
        {
            return Result.Failure<RegisterUserResponse>(userResult.Error);
        }

        // Save to database
        await userRepository.AddAsync(userResult.Value, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var response = new RegisterUserResponse(
            userResult.Value.Id,
            userResult.Value.Email,
            userResult.Value.FullName.GetFullName()
        );

        return Result.Success(response);
    }
}
