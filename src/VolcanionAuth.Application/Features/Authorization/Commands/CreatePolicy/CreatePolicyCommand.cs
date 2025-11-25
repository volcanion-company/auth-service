using MediatR;
using VolcanionAuth.Domain.Common;

namespace VolcanionAuth.Application.Features.Authorization.Commands.CreatePolicy;

public record CreatePolicyCommand(
    string Name,
    string Resource,
    string Action,
    string Effect,
    string Conditions,
    int Priority = 0,
    string? Description = null
) : IRequest<Result<CreatePolicyResponse>>;

public record CreatePolicyResponse(
    Guid PolicyId,
    string Name
);
