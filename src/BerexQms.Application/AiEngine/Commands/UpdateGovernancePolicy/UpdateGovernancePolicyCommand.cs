using BerexQms.Application.AiEngine.DTOs;
using BerexQms.Application.Abstractions.Messaging;

namespace BerexQms.Application.AiEngine.Commands.UpdateGovernancePolicy;

/// <summary>
/// Update the tenant's AI governance policy.
/// Only accessible by Administrator or SuperAdministrator roles.
/// </summary>
public sealed record UpdateGovernancePolicyCommand(
    bool AiEnabled,
    List<string>? AllowedProviders,
    string? DefaultProvider,
    List<string>? AllowedTaskTypes,
    int? MaxDailyRequestsPerUser,
    int? MaxMonthlyRequestsPerTenant,
    bool RequireHumanConfirmation) : ICommand<AiGovernancePolicyDto>;
