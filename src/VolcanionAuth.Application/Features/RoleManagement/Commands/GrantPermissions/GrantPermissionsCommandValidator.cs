using FluentValidation;

namespace VolcanionAuth.Application.Features.RoleManagement.Commands.GrantPermissions;

/// <summary>
/// Validates the GrantPermissionsCommand to ensure all required data is present and valid.
/// </summary>
public class GrantPermissionsCommandValidator : AbstractValidator<GrantPermissionsCommand>
{
    public GrantPermissionsCommandValidator()
    {
        RuleFor(x => x.RoleId)
            .NotEmpty().WithMessage("Role ID is required.");

        RuleFor(x => x.PermissionIds)
            .NotNull().WithMessage("Permission IDs list is required.")
            .Must(list => list != null && list.Count > 0)
            .WithMessage("At least one permission ID must be provided.");

        RuleForEach(x => x.PermissionIds)
            .NotEmpty().WithMessage("Permission ID cannot be empty.");
    }
}
