using MediatR;
using VolcanionAuth.Application.Common.Interfaces;
using VolcanionAuth.Domain.Common;
using VolcanionAuth.Domain.Entities;

namespace VolcanionAuth.Application.Features.Authorization.Commands.CreatePolicy;

public class CreatePolicyCommandHandler : IRequestHandler<CreatePolicyCommand, Result<CreatePolicyResponse>>
{
    private readonly IRepository<Policy> _policyRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreatePolicyCommandHandler(IRepository<Policy> policyRepository, IUnitOfWork unitOfWork)
    {
        _policyRepository = policyRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<CreatePolicyResponse>> Handle(CreatePolicyCommand request, CancellationToken cancellationToken)
    {
        var policyResult = Policy.Create(
            request.Name,
            request.Resource,
            request.Action,
            request.Effect,
            request.Conditions,
            request.Priority,
            request.Description);

        if (policyResult.IsFailure)
            return Result.Failure<CreatePolicyResponse>(policyResult.Error);

        await _policyRepository.AddAsync(policyResult.Value, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new CreatePolicyResponse(policyResult.Value.Id, policyResult.Value.Name));
    }
}
