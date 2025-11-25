using MediatR;
using VolcanionAuth.Application.Common.Interfaces;
using VolcanionAuth.Domain.Common;
using VolcanionAuth.Domain.Entities;
using VolcanionAuth.Domain.ValueObjects;

namespace VolcanionAuth.Application.Features.UserManagement.Commands.CreateUser;

/// <summary>
/// Handler for creating a new user in the system.
/// </summary>
public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Result<CreateUserResponse>>
{
    private readonly IRepository<User> _userRepository;
    private readonly IReadRepository<User> _readUserRepository;
    private readonly IReadRepository<Role> _readRoleRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUnitOfWork _unitOfWork;

    public CreateUserCommandHandler(
        IRepository<User> userRepository,
        IReadRepository<User> readUserRepository,
        IReadRepository<Role> readRoleRepository,
        IPasswordHasher passwordHasher,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _readUserRepository = readUserRepository;
        _readRoleRepository = readRoleRepository;
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<CreateUserResponse>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        // Create Email value object
        var emailResult = Email.Create(request.Email);
        if (emailResult.IsFailure)
        {
            return Result.Failure<CreateUserResponse>(emailResult.Error);
        }

        // Check if user already exists
        var existingUser = await _readUserRepository.GetUserByEmailAsync(request.Email, cancellationToken);
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
        var hashedPassword = _passwordHasher.HashPassword(request.Password);
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
        if (request.RoleIds != null && request.RoleIds.Any())
        {
            var roles = await _readRoleRepository.GetAllAsync(cancellationToken);
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
        await _userRepository.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new CreateUserResponse(
            user.Id,
            user.Email.Value,
            user.FullName.FirstName,
            user.FullName.LastName
        ));
    }
}
