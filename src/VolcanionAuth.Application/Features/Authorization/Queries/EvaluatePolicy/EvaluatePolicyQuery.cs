using MediatR;
using VolcanionAuth.Domain.Common;

namespace VolcanionAuth.Application.Features.Authorization.Queries.EvaluatePolicy;

public record EvaluatePolicyQuery(
    Guid UserId,
    string Resource,
    string Action,
    Dictionary<string, object> Context
) : IRequest<Result<EvaluatePolicyResponse>>;

public record EvaluatePolicyResponse(
    bool IsAllowed,
    string Reason
);
