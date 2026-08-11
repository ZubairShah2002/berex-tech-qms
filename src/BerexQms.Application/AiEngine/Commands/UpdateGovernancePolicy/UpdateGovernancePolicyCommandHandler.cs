using System.Text.Json;
using BerexQms.Application.AiEngine.DTOs;
using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.Exceptions;
using BerexQms.Application.Interfaces;
using BerexQms.Domain.AiEngine.Entities;
using BerexQms.Domain.AiEngine.Repositories;
using BerexQms.SharedKernel;
using BerexQms.SharedKernel.Results;
using BerexQms.SharedKernel.ValueObjects;

namespace BerexQms.Application.AiEngine.Commands.UpdateGovernancePolicy;

internal sealed class UpdateGovernancePolicyCommandHandler
    : ICommandHandler<UpdateGovernancePolicyCommand, AiGovernancePolicyDto>
{
    private readonly IAiGovernancePolicyRepository _policyRepo;
    private readonly ICurrentUserService _currentUser;

    public UpdateGovernancePolicyCommandHandler(
        IAiGovernancePolicyRepository policyRepo,
        ICurrentUserService currentUser)
    {
        _policyRepo = policyRepo;
        _currentUser = currentUser;
    }

    public async Task<Result<AiGovernancePolicyDto>> Handle(
        UpdateGovernancePolicyCommand request, CancellationToken ct)
    {
        // Only administrators can manage governance
        if (!_currentUser.IsInRole("Administrator")
            && !_currentUser.IsInRole("SuperAdministrator"))
        {
            return Result<AiGovernancePolicyDto>.Failure(
                AiGovernanceErrors.GovernanceAccessDenied);
        }

        var tenantId = TenantId.From(_currentUser.TenantId);

        // Get or create policy
        var policy = await _policyRepo.GetByTenantAsync(ct);

        if (policy == null)
        {
            policy = AiGovernancePolicy.Create(Guid.NewGuid(), tenantId);
            await _policyRepo.AddAsync(policy, ct);
        }

        // Apply governance changes
        policy.SetAiEnabled(request.AiEnabled);

        policy.SetAllowedProviders(
            request.AllowedProviders is { Count: > 0 }
                ? JsonSerializer.Serialize(request.AllowedProviders)
                : null);

        policy.SetDefaultProvider(request.DefaultProvider);

        policy.SetAllowedTaskTypes(
            request.AllowedTaskTypes is { Count: > 0 }
                ? JsonSerializer.Serialize(request.AllowedTaskTypes)
                : null);

        policy.SetMaxDailyRequestsPerUser(request.MaxDailyRequestsPerUser);
        policy.SetMaxMonthlyRequestsPerTenant(request.MaxMonthlyRequestsPerTenant);

        // Human confirmation is always enforced
        policy.SetRequireHumanConfirmation(request.RequireHumanConfirmation);

        await _policyRepo.UpdateAsync(policy, ct);

        return Result<AiGovernancePolicyDto>.Success(MapToDto(policy));
    }

    private static AiGovernancePolicyDto MapToDto(AiGovernancePolicy policy) => new()
    {
        AiEnabled = policy.IsAiEnabled,
        AllowedProviders = ParseJsonList(policy.AllowedProvidersJson),
        DefaultProvider = policy.DefaultProvider,
        AllowedTaskTypes = ParseJsonList(policy.AllowedTaskTypesJson),
        MaxDailyRequestsPerUser = policy.MaxDailyRequestsPerUser,
        MaxMonthlyRequestsPerTenant = policy.MaxMonthlyRequestsPerTenant,
        RequireHumanConfirmation = policy.RequireHumanConfirmation,
    };

    private static List<string> ParseJsonList(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return [];
        try { return JsonSerializer.Deserialize<List<string>>(json) ?? []; }
        catch { return []; }
    }
}
