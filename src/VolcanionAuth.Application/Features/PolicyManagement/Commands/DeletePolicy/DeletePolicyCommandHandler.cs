using VolcanionAuth.Application.Common.Interfaces;
using VolcanionAuth.Domain.Entities;

namespace VolcanionAuth.Application.Features.PolicyManagement.Commands.DeletePolicy;

/// <summary>
/// Handler for deleting a policy from the system.
/// </summary>
public class DeletePolicyCommandHandler : IRequestHandler<DeletePolicyCommand, Result>
{
    private readonly IRepository<Policy> _policyRepository;
    private readonly IReadRepository<Policy> _readPolicyRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeletePolicyCommandHandler(
        IRepository<Policy> policyRepository,
        IReadRepository<Policy> readPolicyRepository,
        IUnitOfWork unitOfWork)
    {
        _policyRepository = policyRepository;
        _readPolicyRepository = readPolicyRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DeletePolicyCommand request, CancellationToken cancellationToken)
    {
        // Find the policy
        var policy = await _readPolicyRepository.GetByIdAsync(request.PolicyId, cancellationToken);
        if (policy == null)
        {
            return Result.Failure($"Policy with ID '{request.PolicyId}' was not found");
        }

        // Delete the policy
        _policyRepository.Remove(policy);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
