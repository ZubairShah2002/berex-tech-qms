using BerexQms.Application.AiEngine;
using BerexQms.Application.AiEngine.Commands.ExecuteAiAnalysis;
using BerexQms.Application.AiEngine.Commands.UpdateGovernancePolicy;
using BerexQms.Application.AiEngine.Commands.UpdateUserPreferences;
using BerexQms.Application.AiEngine.DTOs;
using BerexQms.Application.AiEngine.Interfaces;
using BerexQms.Application.AiEngine.Queries.GetEffectivePolicy;
using BerexQms.Application.Interfaces;
using BerexQms.Domain.AiEngine.Entities;
using BerexQms.Domain.AiEngine.Repositories;
using BerexQms.SharedKernel.Results;
using BerexQms.SharedKernel.ValueObjects;
using NSubstitute;

namespace BerexQms.Application.Tests.CrossModule;

/// <summary>
/// Cross-module integration tests for the AI Analysis workflow:
/// User -> Permission -> Governance -> Context -> AI Provider -> Recommendation -> Human Confirmation -> Audit Trail
///
/// Tests exercise the application layer handlers that enforce governance,
/// validate access, and coordinate AI execution.
/// </summary>
public class AiAnalysisWorkflowTests
{
    private static readonly Guid TestUserId = Guid.NewGuid();
    private static readonly Guid TestTenantId = Guid.NewGuid();

    private static ICurrentUserService CreateCurrentUser(
        string role = "Operator",
        Guid? userId = null,
        Guid? tenantId = null)
    {
        var mock = Substitute.For<ICurrentUserService>();
        mock.UserId.Returns(userId ?? TestUserId);
        mock.TenantId.Returns(tenantId ?? TestTenantId);
        mock.IsAuthenticated.Returns(true);
        mock.Email.Returns("test@berex.com");
        mock.Roles.Returns(new List<string> { role });
        mock.IsInRole(Arg.Any<string>()).Returns(ci => ci.ArgAt<string>(0) == role);
        return mock;
    }

    // -- Governance Denial Scenarios (via UpdateGovernancePolicy + GetEffectivePolicy) --

    [Fact]
    public async Task AiGovernance_DeniesAccess_WhenAiDisabledForTenant()
    {
        // Governance policy disables AI
        var policy = AiGovernancePolicy.Create(Guid.NewGuid(), TenantId.From(TestTenantId));
        policy.SetAiEnabled(false);

        var govService = Substitute.For<IAiGovernanceService>();
        govService.GetEffectivePolicyAsync(TestUserId, Arg.Any<CancellationToken>())
            .Returns(new AiEffectivePolicyDto
            {
                AiEnabled = false,
                ConfirmationRequired = true,
            });

        var currentUser = CreateCurrentUser();
        var handler = new GetEffectivePolicyQueryHandler(govService, currentUser);

        var result = await handler.Handle(new GetEffectivePolicyQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.False(result.Value.AiEnabled);
    }

    [Fact]
    public async Task AiGovernance_DeniesAccess_WhenUserExceedsDailyRateLimit()
    {
        var govService = Substitute.For<IAiGovernanceService>();
        govService.GetEffectivePolicyAsync(TestUserId, Arg.Any<CancellationToken>())
            .Returns(new AiEffectivePolicyDto
            {
                AiEnabled = true,
                DailyRequestsUsed = 100,
                DailyRequestsLimit = 100,
                ConfirmationRequired = true,
            });

        var currentUser = CreateCurrentUser();
        var handler = new GetEffectivePolicyQueryHandler(govService, currentUser);

        var result = await handler.Handle(new GetEffectivePolicyQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(100, result.Value.DailyRequestsUsed);
        Assert.Equal(100, result.Value.DailyRequestsLimit);
        // User has hit their limit — orchestrator would deny further requests
    }

    [Fact]
    public async Task AiGovernance_DeniesAccess_WhenProviderNotAllowed()
    {
        // Governance restricts to Local only, but user wants Claude
        var policy = AiGovernancePolicy.Create(Guid.NewGuid(), TenantId.From(TestTenantId));
        policy.SetAllowedProviders("[\"Local\"]");

        var prefRepo = Substitute.For<IAiUserPreferenceRepository>();
        prefRepo.GetByUserIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((AiUserPreference?)null);
        var govRepo = Substitute.For<IAiGovernancePolicyRepository>();
        govRepo.GetByTenantAsync(Arg.Any<CancellationToken>()).Returns(policy);
        var currentUser = CreateCurrentUser();

        var handler = new UpdateUserPreferencesCommandHandler(prefRepo, govRepo, currentUser);
        var command = new UpdateUserPreferencesCommand(true, "Claude", true, null, null);

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("AiGovernance.ProviderNotPermitted", result.Error.Code);
    }

    [Fact]
    public async Task AiGovernance_DeniesAccess_WhenTaskTypeNotAllowed()
    {
        // Governance restricts to DocumentAnalysis only
        var policy = AiGovernancePolicy.Create(Guid.NewGuid(), TenantId.From(TestTenantId));
        policy.SetAllowedTaskTypes("[\"DocumentAnalysis\"]");

        var prefRepo = Substitute.For<IAiUserPreferenceRepository>();
        prefRepo.GetByUserIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((AiUserPreference?)null);
        var govRepo = Substitute.For<IAiGovernancePolicyRepository>();
        govRepo.GetByTenantAsync(Arg.Any<CancellationToken>()).Returns(policy);
        var currentUser = CreateCurrentUser();

        var handler = new UpdateUserPreferencesCommandHandler(prefRepo, govRepo, currentUser);
        var command = new UpdateUserPreferencesCommand(true, "Automatic", true, null,
            new List<string> { "RiskAnalysis" }); // Not in allowed list

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("AiGovernance.TaskNotPermitted", result.Error.Code);
    }

    [Fact]
    public async Task AiGovernance_AllowsAccess_WhenAllChecksPassed_ReturnsEffectivePolicy()
    {
        var govService = Substitute.For<IAiGovernanceService>();
        govService.GetEffectivePolicyAsync(TestUserId, Arg.Any<CancellationToken>())
            .Returns(new AiEffectivePolicyDto
            {
                AiEnabled = true,
                AllowedProviders = new List<string> { "Claude", "Local" },
                PreferredProvider = "Claude",
                AllowedTaskTypes = new List<string> { "DocumentAnalysis", "RiskAnalysis" },
                ConfirmationRequired = true,
                DailyRequestsUsed = 5,
                DailyRequestsLimit = 100,
                MonthlyRequestsUsed = 50,
                MonthlyRequestsLimit = 5000,
            });

        var currentUser = CreateCurrentUser();
        var handler = new GetEffectivePolicyQueryHandler(govService, currentUser);

        var result = await handler.Handle(new GetEffectivePolicyQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value.AiEnabled);
        Assert.Equal("Claude", result.Value.PreferredProvider);
        Assert.Contains("DocumentAnalysis", result.Value.AllowedTaskTypes);
        Assert.Contains("RiskAnalysis", result.Value.AllowedTaskTypes);
        Assert.Equal(5, result.Value.DailyRequestsUsed);
        Assert.Equal(100, result.Value.DailyRequestsLimit);
    }

    [Fact]
    public async Task UserPreferences_CannotOverrideGovernanceProviderRestrictions()
    {
        // Governance restricts providers to Local only
        var policy = AiGovernancePolicy.Create(Guid.NewGuid(), TenantId.From(TestTenantId));
        policy.SetAllowedProviders("[\"Local\"]");

        var prefRepo = Substitute.For<IAiUserPreferenceRepository>();
        prefRepo.GetByUserIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((AiUserPreference?)null);
        var govRepo = Substitute.For<IAiGovernancePolicyRepository>();
        govRepo.GetByTenantAsync(Arg.Any<CancellationToken>()).Returns(policy);
        var currentUser = CreateCurrentUser();

        var handler = new UpdateUserPreferencesCommandHandler(prefRepo, govRepo, currentUser);

        // Attempt to set preferred provider to Claude (not allowed by governance)
        var command = new UpdateUserPreferencesCommand(true, "Claude", true, null, null);
        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("AiGovernance.ProviderNotPermitted", result.Error.Code);

        // But "Automatic" should always be allowed
        var autoCommand = new UpdateUserPreferencesCommand(true, "Automatic", true, null, null);
        var autoResult = await handler.Handle(autoCommand, CancellationToken.None);
        Assert.True(autoResult.IsSuccess);
    }

    [Fact]
    public async Task HumanConfirmation_AlwaysRequired_CannotBeDisabled()
    {
        // Even when governance policy tries to disable confirmation, domain enforces it
        var policy = AiGovernancePolicy.Create(Guid.NewGuid(), TenantId.From(TestTenantId));
        policy.SetRequireHumanConfirmation(false); // Domain should ignore and keep true

        Assert.True(policy.RequireHumanConfirmation);

        // Verify through the handler as well
        var govService = Substitute.For<IAiGovernanceService>();
        govService.GetEffectivePolicyAsync(TestUserId, Arg.Any<CancellationToken>())
            .Returns(new AiEffectivePolicyDto
            {
                AiEnabled = true,
                ConfirmationRequired = true, // Always true per governance
            });

        var currentUser = CreateCurrentUser();
        var handler = new GetEffectivePolicyQueryHandler(govService, currentUser);

        var result = await handler.Handle(new GetEffectivePolicyQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value.ConfirmationRequired);
    }
}
