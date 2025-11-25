using MediatR;
using VolcanionAuth.Application.Common.Interfaces;
using VolcanionAuth.Domain.Common;
using VolcanionAuth.Domain.Entities;
using VolcanionAuth.Domain.ValueObjects;

namespace VolcanionAuth.Application.Features.Authentication.Commands.RegisterUser;

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, Result<RegisterUserResponse>>
{
    private readonly IRepository<User> _userRepository;
    private readonly IReadRepository<User> _readRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUnitOfWork _unitOfWork;

    public RegisterUserCommandHandler(
        IRepository<User> userRepository,
        IReadRepository<User> readRepository,
        IPasswordHasher passwordHasher,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _readRepository = readRepository;
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<RegisterUserResponse>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        // Check if email already exists
        var existingUser = await _readRepository.GetUserByEmailAsync(request.Email, cancellationToken);
        if (existingUser != null)
            return Result.Failure<RegisterUserResponse>("Email already registered.");

        // Create value objects
        var emailResult = Email.Create(request.Email);
        if (emailResult.IsFailure)
            return Result.Failure<RegisterUserResponse>(emailResult.Error);

        var passwordResult = Password.Create(request.Password);
        if (passwordResult.IsFailure)
            return Result.Failure<RegisterUserResponse>(passwordResult.Error);

        var fullNameResult = FullName.Create(request.FirstName, request.LastName);
        if (fullNameResult.IsFailure)
            return Result.Failure<RegisterUserResponse>(fullNameResult.Error);

        // Hash password
        var hashedPassword = _passwordHasher.HashPassword(request.Password);
        var password = Password.CreateFromHash(hashedPassword);

        // Create user
        var userResult = User.Create(emailResult.Value, password, fullNameResult.Value);
        if (userResult.IsFailure)
            return Result.Failure<RegisterUserResponse>(userResult.Error);

        // Save to database
        await _userRepository.AddAsync(userResult.Value, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var response = new RegisterUserResponse(
            userResult.Value.Id,
            userResult.Value.Email,
            userResult.Value.FullName.GetFullName()
        );

        return Result.Success(response);
    }
}
