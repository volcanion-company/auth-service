using VolcanionAuth.Application.Common.Interfaces;
using VolcanionAuth.Domain.Entities;
using VolcanionAuth.Domain.ValueObjects;

namespace VolcanionAuth.Application.Features.UserManagement.Commands.CreateUser;

/// <summary>
/// Handles the creation of a new user by processing the CreateUserCommand and persisting the user to the data store.
/// </summary>
/// <remarks>This handler validates user input, ensures email uniqueness, hashes the password, assigns roles if
/// specified, and saves the new user atomically. It returns a result indicating success or failure, with error details
/// if applicable.</remarks>
/// <param name="userRepository">The repository used to add new User entities to the data store.</param>
/// <param name="readUserRepository">The read-only repository used to query existing User entities, such as checking for duplicate emails.</param>
/// <param name="readRoleRepository">The read-only repository used to retrieve Role entities for assigning roles to the new user.</param>
/// <param name="passwordHasher">The service used to securely hash user passwords before storage.</param>
/// <param name="unitOfWork">The unit of work used to commit changes to the data store as part of the user creation process.</param>
public class CreateUserCommandHandler(
    IRepository<User> userRepository,
    IReadRepository<User> readUserRepository,
    IReadRepository<Role> readRoleRepository,
    IPasswordHasher passwordHasher,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateUserCommand, Result<CreateUserResponse>>
{
    /// <summary>
    /// Handles the creation of a new user account based on the specified command request.
    /// </summary>
    /// <remarks>This method validates the provided user information, checks for existing users with the same
    /// email, assigns roles if specified, and persists the new user to the database. If any validation or business rule
    /// fails, the result will indicate failure with an appropriate error message.</remarks>
    /// <param name="request">The command containing user details, such as email, password, first name, last name, and optional role
    /// identifiers, to be used for creating the user.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A result containing the response data for the newly created user if successful; otherwise, a failure result with
    /// an error message.</returns>
    public async Task<Result<CreateUserResponse>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        // Create Email value object
        var emailResult = Email.Create(request.Email);
        if (emailResult.IsFailure)
        {
            return Result.Failure<CreateUserResponse>(emailResult.Error);
        }

        // Check if user already exists
        var existingUser = await readUserRepository.GetUserByEmailAsync(request.Email, cancellationToken);
        if (existingUser != null)
        {
            return Result.Failure<CreateUserResponse>("A user with this email already exists");
        }

        // Create FullName value object
        var fullNameResult = FullName.Create(request.FirstName, request.LastName);
        if (fullNameResult.IsFailure)
        {
            return Result.Failure<CreateUserResponse>(fullNameResult.Error);
        }

        // Hash the password
        var hashedPassword = passwordHasher.HashPassword(request.Password);
        var passwordResult = Password.CreateFromHash(hashedPassword);

        // Create the user
        var userResult = User.Create(
            emailResult.Value,
            passwordResult,
            fullNameResult.Value
        );

        if (userResult.IsFailure)
        {
            return Result.Failure<CreateUserResponse>(userResult.Error);
        }

        var user = userResult.Value;

        // Assign roles if provided
        if (request.RoleIds != null && request.RoleIds.Count != 0)
        {
            var roles = await readRoleRepository.GetAllAsync(cancellationToken);
            var validRoleIds = roles.Where(r => request.RoleIds.Contains(r.Id)).Select(r => r.Id).ToList();

            if (validRoleIds.Count != request.RoleIds.Count)
            {
                return Result.Failure<CreateUserResponse>("One or more specified roles were not found");
            }

            foreach (var roleId in validRoleIds)
            {
                user.AddRole(roleId);
            }
        }

        // Save to database
        await userRepository.AddAsync(user, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new CreateUserResponse(
            user.Id,
            user.Email.Value,
            user.FullName.FirstName,
            user.FullName.LastName
        ));
    }
}
