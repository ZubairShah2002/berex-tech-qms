using BerexQms.Application.AiEngine.DTOs;
using BerexQms.Application.AiEngine.Interfaces;
using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.Interfaces;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.AiEngine.Queries.GetEffectivePolicy;

internal sealed class GetEffectivePolicyQueryHandler
    : IQueryHandler<GetEffectivePolicyQuery, AiEffectivePolicyDto>
{
    private readonly IAiGovernanceService _governanceService;
    private readonly ICurrentUserService _currentUser;

    public GetEffectivePolicyQueryHandler(
        IAiGovernanceService governanceService,
        ICurrentUserService currentUser)
    {
        _governanceService = governanceService;
        _currentUser = currentUser;
    }

    public async Task<Result<AiEffectivePolicyDto>> Handle(
        GetEffectivePolicyQuery request, CancellationToken ct)
    {
        var effectivePolicy = await _governanceService.GetEffectivePolicyAsync(
            _currentUser.UserId, ct);

        return Result<AiEffectivePolicyDto>.Success(effectivePolicy);
    }
}
