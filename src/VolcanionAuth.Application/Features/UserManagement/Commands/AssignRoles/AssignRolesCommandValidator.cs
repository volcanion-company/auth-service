using FluentValidation;

namespace VolcanionAuth.Application.Features.UserManagement.Commands.AssignRoles;

/// <summary>
/// Validates the AssignRolesCommand to ensure all required data is present and valid.
/// </summary>
public class AssignRolesCommandValidator : AbstractValidator<AssignRolesCommand>
{
    public AssignRolesCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.RoleIds)
            .NotNull().WithMessage("Role IDs list is required.")
            .Must(list => list != null && list.Count > 0)
            .WithMessage("At least one role ID must be provided.");

        RuleForEach(x => x.RoleIds)
            .NotEmpty().WithMessage("Role ID cannot be empty.");
    }
}
