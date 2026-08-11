using System.Text.Json;
using BerexQms.Application.AiEngine.DTOs;
using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.Interfaces;
using BerexQms.Domain.AiEngine.Repositories;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.AiEngine.Queries.GetGovernancePolicy;

internal sealed class GetGovernancePolicyQueryHandler
    : IQueryHandler<GetGovernancePolicyQuery, AiGovernancePolicyDto>
{
    private readonly IAiGovernancePolicyRepository _policyRepo;
    private readonly ICurrentUserService _currentUser;

    public GetGovernancePolicyQueryHandler(
        IAiGovernancePolicyRepository policyRepo,
        ICurrentUserService currentUser)
    {
        _policyRepo = policyRepo;
        _currentUser = currentUser;
    }

    public async Task<Result<AiGovernancePolicyDto>> Handle(
        GetGovernancePolicyQuery request, CancellationToken ct)
    {
        // Only administrators can view governance policy
        if (!_currentUser.IsInRole("Administrator")
            && !_currentUser.IsInRole("SuperAdministrator"))
        {
            return Result<AiGovernancePolicyDto>.Failure(
                AiGovernanceErrors.GovernanceAccessDenied);
        }

        var policy = await _policyRepo.GetByTenantAsync(ct);

        if (policy == null)
        {
            // Return defaults
            return Result<AiGovernancePolicyDto>.Success(new AiGovernancePolicyDto
            {
                AiEnabled = true,
                AllowedProviders = [],
                DefaultProvider = null,
                AllowedTaskTypes = [],
                MaxDailyRequestsPerUser = null,
                MaxMonthlyRequestsPerTenant = null,
                RequireHumanConfirmation = true,
            });
        }

        return Result<AiGovernancePolicyDto>.Success(new AiGovernancePolicyDto
        {
            AiEnabled = policy.IsAiEnabled,
            AllowedProviders = ParseJsonList(policy.AllowedProvidersJson),
            DefaultProvider = policy.DefaultProvider,
            AllowedTaskTypes = ParseJsonList(policy.AllowedTaskTypesJson),
            MaxDailyRequestsPerUser = policy.MaxDailyRequestsPerUser,
            MaxMonthlyRequestsPerTenant = policy.MaxMonthlyRequestsPerTenant,
            RequireHumanConfirmation = policy.RequireHumanConfirmation,
        });
    }

    private static List<string> ParseJsonList(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return [];
        try { return JsonSerializer.Deserialize<List<string>>(json) ?? []; }
        catch { return []; }
    }
}
