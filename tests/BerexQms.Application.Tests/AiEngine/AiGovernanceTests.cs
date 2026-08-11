using BerexQms.Application.AiEngine;
using BerexQms.Application.AiEngine.Commands.UpdateGovernancePolicy;
using BerexQms.Application.AiEngine.Commands.UpdateUserPreferences;
using BerexQms.Application.AiEngine.DTOs;
using BerexQms.Application.AiEngine.Queries.GetGovernancePolicy;
using BerexQms.Application.AiEngine.Queries.GetUserPreferences;
using BerexQms.Application.AiEngine.Queries.GetEffectivePolicy;
using BerexQms.Application.AiEngine.Interfaces;
using BerexQms.Application.Interfaces;
using BerexQms.Domain.AiEngine.Entities;
using BerexQms.Domain.AiEngine.Repositories;
using BerexQms.SharedKernel.ValueObjects;
using NSubstitute;

namespace BerexQms.Application.Tests.AiEngine;

public class AiGovernanceTests
{
    private static readonly Guid TestUserId = Guid.NewGuid();
    private static readonly Guid TestTenantId = Guid.NewGuid();

    private static ICurrentUserService CreateMockCurrentUser(
        string role = "Operator",
        Guid? userId = null,
        Guid? tenantId = null)
    {
        var mock = Substitute.For<ICurrentUserService>();
        mock.UserId.Returns(userId ?? TestUserId);
        mock.TenantId.Returns(tenantId ?? TestTenantId);
        mock.IsAuthenticated.Returns(true);
        mock.Email.Returns("test@example.com");
        mock.Roles.Returns(new List<string> { role });
        mock.IsInRole(Arg.Any<string>()).Returns(ci => ci.ArgAt<string>(0) == role);
        return mock;
    }

    // ---- GetUserPreferencesQueryHandler Tests ----

    [Fact]
    public async Task GetUserPreferences_NoPreferenceExists_ReturnsDefaults()
    {
        var repo = Substitute.For<IAiUserPreferenceRepository>();
        repo.GetByUserIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((AiUserPreference?)null);
        var currentUser = CreateMockCurrentUser();

        var handler = new GetUserPreferencesQueryHandler(repo, currentUser);
        var result = await handler.Handle(new GetUserPreferencesQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value.AiEnabled);
        Assert.Equal("Automatic", result.Value.PreferredProvider);
        Assert.True(result.Value.ConfirmationRequired);
        Assert.Null(result.Value.PreferredLanguage);
        Assert.Empty(result.Value.EnabledTaskTypes);
    }

    [Fact]
    public async Task GetUserPreferences_PreferenceExists_ReturnsMappedDto()
    {
        var pref = AiUserPreference.Create(Guid.NewGuid(), TenantId.From(TestTenantId), TestUserId);
        pref.SetAiEnabled(false);
        pref.SetPreferredProvider("Claude");
        pref.SetPreferredLanguage("de");

        var repo = Substitute.For<IAiUserPreferenceRepository>();
        repo.GetByUserIdAsync(TestUserId, Arg.Any<CancellationToken>()).Returns(pref);
        var currentUser = CreateMockCurrentUser();

        var handler = new GetUserPreferencesQueryHandler(repo, currentUser);
        var result = await handler.Handle(new GetUserPreferencesQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.False(result.Value.AiEnabled);
        Assert.Equal("Claude", result.Value.PreferredProvider);
        Assert.Equal("de", result.Value.PreferredLanguage);
    }

    // ---- GetGovernancePolicyQueryHandler Tests ----

    [Fact]
    public async Task GetGovernancePolicy_NonAdmin_ReturnsForbidden()
    {
        var repo = Substitute.For<IAiGovernancePolicyRepository>();
        var currentUser = CreateMockCurrentUser("Operator");

        var handler = new GetGovernancePolicyQueryHandler(repo, currentUser);
        var result = await handler.Handle(new GetGovernancePolicyQuery(), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("AiGovernance.AccessDenied", result.Error.Code);
    }

    [Fact]
    public async Task GetGovernancePolicy_Administrator_ReturnsPolicy()
    {
        var repo = Substitute.For<IAiGovernancePolicyRepository>();
        repo.GetByTenantAsync(Arg.Any<CancellationToken>()).Returns((AiGovernancePolicy?)null);
        var currentUser = CreateMockCurrentUser("Administrator");

        var handler = new GetGovernancePolicyQueryHandler(repo, currentUser);
        var result = await handler.Handle(new GetGovernancePolicyQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value.AiEnabled);
        Assert.True(result.Value.RequireHumanConfirmation);
    }

    [Fact]
    public async Task GetGovernancePolicy_SuperAdministrator_ReturnsPolicy()
    {
        var repo = Substitute.For<IAiGovernancePolicyRepository>();
        repo.GetByTenantAsync(Arg.Any<CancellationToken>()).Returns((AiGovernancePolicy?)null);
        var currentUser = CreateMockCurrentUser("SuperAdministrator");

        var handler = new GetGovernancePolicyQueryHandler(repo, currentUser);
        var result = await handler.Handle(new GetGovernancePolicyQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task GetGovernancePolicy_ExistingPolicy_ReturnsMappedDto()
    {
        var policy = AiGovernancePolicy.Create(Guid.NewGuid(), TenantId.From(TestTenantId));
        policy.SetAiEnabled(false);
        policy.SetMaxDailyRequestsPerUser(50);
        policy.SetMaxMonthlyRequestsPerTenant(1000);

        var repo = Substitute.For<IAiGovernancePolicyRepository>();
        repo.GetByTenantAsync(Arg.Any<CancellationToken>()).Returns(policy);
        var currentUser = CreateMockCurrentUser("Administrator");

        var handler = new GetGovernancePolicyQueryHandler(repo, currentUser);
        var result = await handler.Handle(new GetGovernancePolicyQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.False(result.Value.AiEnabled);
        Assert.Equal(50, result.Value.MaxDailyRequestsPerUser);
        Assert.Equal(1000, result.Value.MaxMonthlyRequestsPerTenant);
    }

    // ---- GetEffectivePolicyQueryHandler Tests ----

    [Fact]
    public async Task GetEffectivePolicy_DelegatesToGovernanceService()
    {
        var expectedPolicy = new AiEffectivePolicyDto
        {
            AiEnabled = true,
            AllowedProviders = new List<string> { "Claude" },
            PreferredProvider = "Claude",
            AllowedTaskTypes = [],
            ConfirmationRequired = true,
            DailyRequestsUsed = 5,
            DailyRequestsLimit = 100,
        };

        var governanceService = Substitute.For<IAiGovernanceService>();
        governanceService.GetEffectivePolicyAsync(TestUserId, Arg.Any<CancellationToken>()).Returns(expectedPolicy);
        var currentUser = CreateMockCurrentUser();

        var handler = new GetEffectivePolicyQueryHandler(governanceService, currentUser);
        var result = await handler.Handle(new GetEffectivePolicyQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Claude", result.Value.PreferredProvider);
        Assert.Equal(5, result.Value.DailyRequestsUsed);
        Assert.Equal(100, result.Value.DailyRequestsLimit);
    }

    // ---- UpdateUserPreferencesCommandHandler Tests ----

    [Fact]
    public async Task UpdateUserPreferences_NoExistingPreference_CreatesNew()
    {
        var prefRepo = Substitute.For<IAiUserPreferenceRepository>();
        prefRepo.GetByUserIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((AiUserPreference?)null);
        var govRepo = Substitute.For<IAiGovernancePolicyRepository>();
        govRepo.GetByTenantAsync(Arg.Any<CancellationToken>()).Returns((AiGovernancePolicy?)null);
        var currentUser = CreateMockCurrentUser();

        var handler = new UpdateUserPreferencesCommandHandler(prefRepo, govRepo, currentUser);
        var command = new UpdateUserPreferencesCommand(true, "Claude", true, "en", null);

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Claude", result.Value.PreferredProvider);
        await prefRepo.Received(1).AddAsync(Arg.Any<AiUserPreference>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateUserPreferences_ExistingPreference_Updates()
    {
        var pref = AiUserPreference.Create(Guid.NewGuid(), TenantId.From(TestTenantId), TestUserId);
        var prefRepo = Substitute.For<IAiUserPreferenceRepository>();
        prefRepo.GetByUserIdAsync(TestUserId, Arg.Any<CancellationToken>()).Returns(pref);
        var govRepo = Substitute.For<IAiGovernancePolicyRepository>();
        govRepo.GetByTenantAsync(Arg.Any<CancellationToken>()).Returns((AiGovernancePolicy?)null);
        var currentUser = CreateMockCurrentUser();

        var handler = new UpdateUserPreferencesCommandHandler(prefRepo, govRepo, currentUser);
        var command = new UpdateUserPreferencesCommand(false, "Local", false, "fr", new List<string> { "RiskAnalysis" });

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.False(result.Value.AiEnabled);
        Assert.Equal("Local", result.Value.PreferredProvider);
        await prefRepo.Received(0).AddAsync(Arg.Any<AiUserPreference>(), Arg.Any<CancellationToken>());
        await prefRepo.Received(1).UpdateAsync(Arg.Any<AiUserPreference>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateUserPreferences_ProviderBlockedByGovernance_ReturnsForbidden()
    {
        var policy = AiGovernancePolicy.Create(Guid.NewGuid(), TenantId.From(TestTenantId));
        policy.SetAllowedProviders("[\"Local\"]"); // Only Local allowed

        var prefRepo = Substitute.For<IAiUserPreferenceRepository>();
        prefRepo.GetByUserIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((AiUserPreference?)null);
        var govRepo = Substitute.For<IAiGovernancePolicyRepository>();
        govRepo.GetByTenantAsync(Arg.Any<CancellationToken>()).Returns(policy);
        var currentUser = CreateMockCurrentUser();

        var handler = new UpdateUserPreferencesCommandHandler(prefRepo, govRepo, currentUser);
        var command = new UpdateUserPreferencesCommand(true, "Claude", true, null, null); // Claude NOT allowed

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("AiGovernance.ProviderNotPermitted", result.Error.Code);
    }

    [Fact]
    public async Task UpdateUserPreferences_TaskBlockedByGovernance_ReturnsForbidden()
    {
        var policy = AiGovernancePolicy.Create(Guid.NewGuid(), TenantId.From(TestTenantId));
        policy.SetAllowedTaskTypes("[\"DocumentAnalysis\"]"); // Only DocumentAnalysis allowed

        var prefRepo = Substitute.For<IAiUserPreferenceRepository>();
        prefRepo.GetByUserIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((AiUserPreference?)null);
        var govRepo = Substitute.For<IAiGovernancePolicyRepository>();
        govRepo.GetByTenantAsync(Arg.Any<CancellationToken>()).Returns(policy);
        var currentUser = CreateMockCurrentUser();

        var handler = new UpdateUserPreferencesCommandHandler(prefRepo, govRepo, currentUser);
        var command = new UpdateUserPreferencesCommand(true, "Automatic", true, null, new List<string> { "RiskAnalysis" });

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("AiGovernance.TaskNotPermitted", result.Error.Code);
    }

    [Fact]
    public async Task UpdateUserPreferences_AutomaticProvider_AlwaysAllowed()
    {
        var policy = AiGovernancePolicy.Create(Guid.NewGuid(), TenantId.From(TestTenantId));
        policy.SetAllowedProviders("[\"Local\"]");

        var prefRepo = Substitute.For<IAiUserPreferenceRepository>();
        prefRepo.GetByUserIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((AiUserPreference?)null);
        var govRepo = Substitute.For<IAiGovernancePolicyRepository>();
        govRepo.GetByTenantAsync(Arg.Any<CancellationToken>()).Returns(policy);
        var currentUser = CreateMockCurrentUser();

        var handler = new UpdateUserPreferencesCommandHandler(prefRepo, govRepo, currentUser);
        var command = new UpdateUserPreferencesCommand(true, "Automatic", true, null, null);

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
    }

    // ---- UpdateGovernancePolicyCommandHandler Tests ----

    [Fact]
    public async Task UpdateGovernancePolicy_NonAdmin_ReturnsForbidden()
    {
        var repo = Substitute.For<IAiGovernancePolicyRepository>();
        var currentUser = CreateMockCurrentUser("Operator");

        var handler = new UpdateGovernancePolicyCommandHandler(repo, currentUser);
        var command = new UpdateGovernancePolicyCommand(true, null, null, null, null, null, true);

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("AiGovernance.AccessDenied", result.Error.Code);
    }

    [Fact]
    public async Task UpdateGovernancePolicy_Administrator_CreatesPolicy()
    {
        var repo = Substitute.For<IAiGovernancePolicyRepository>();
        repo.GetByTenantAsync(Arg.Any<CancellationToken>()).Returns((AiGovernancePolicy?)null);
        var currentUser = CreateMockCurrentUser("Administrator");

        var handler = new UpdateGovernancePolicyCommandHandler(repo, currentUser);
        var command = new UpdateGovernancePolicyCommand(
            true,
            new List<string> { "Local", "Claude" },
            "Local",
            null,
            100,
            5000,
            true);

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value.AiEnabled);
        Assert.Equal(100, result.Value.MaxDailyRequestsPerUser);
        Assert.Equal(5000, result.Value.MaxMonthlyRequestsPerTenant);
        await repo.Received(1).AddAsync(Arg.Any<AiGovernancePolicy>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateGovernancePolicy_ExistingPolicy_Updates()
    {
        var policy = AiGovernancePolicy.Create(Guid.NewGuid(), TenantId.From(TestTenantId));
        var repo = Substitute.For<IAiGovernancePolicyRepository>();
        repo.GetByTenantAsync(Arg.Any<CancellationToken>()).Returns(policy);
        var currentUser = CreateMockCurrentUser("Administrator");

        var handler = new UpdateGovernancePolicyCommandHandler(repo, currentUser);
        var command = new UpdateGovernancePolicyCommand(false, null, null, null, null, null, true);

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.False(result.Value.AiEnabled);
        await repo.Received(0).AddAsync(Arg.Any<AiGovernancePolicy>(), Arg.Any<CancellationToken>());
        await repo.Received(1).UpdateAsync(Arg.Any<AiGovernancePolicy>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateGovernancePolicy_ConfirmationFalse_StillEnforcesTrue()
    {
        var repo = Substitute.For<IAiGovernancePolicyRepository>();
        repo.GetByTenantAsync(Arg.Any<CancellationToken>()).Returns((AiGovernancePolicy?)null);
        var currentUser = CreateMockCurrentUser("SuperAdministrator");

        var handler = new UpdateGovernancePolicyCommandHandler(repo, currentUser);
        var command = new UpdateGovernancePolicyCommand(true, null, null, null, null, null, false);

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value.RequireHumanConfirmation);
    }

    // ---- Validator Tests ----

    [Fact]
    public void UserPreferencesValidator_ValidCommand_Passes()
    {
        var validator = new UpdateUserPreferencesCommandValidator();
        var command = new UpdateUserPreferencesCommand(true, "Claude", true, "en", new List<string> { "DocumentAnalysis" });

        var result = validator.Validate(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void UserPreferencesValidator_InvalidProvider_Fails()
    {
        var validator = new UpdateUserPreferencesCommandValidator();
        var command = new UpdateUserPreferencesCommand(true, "InvalidProvider", true, null, null);

        var result = validator.Validate(command);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void UserPreferencesValidator_InvalidTaskType_Fails()
    {
        var validator = new UpdateUserPreferencesCommandValidator();
        var command = new UpdateUserPreferencesCommand(true, null, true, null, new List<string> { "InvalidTask" });

        var result = validator.Validate(command);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void UserPreferencesValidator_LanguageTooLong_Fails()
    {
        var validator = new UpdateUserPreferencesCommandValidator();
        var command = new UpdateUserPreferencesCommand(true, null, true, "ThisLanguageIsTooLong", null);

        var result = validator.Validate(command);

        Assert.False(result.IsValid);
    }

    [Theory]
    [InlineData("Automatic")]
    [InlineData("Local")]
    [InlineData("Claude")]
    [InlineData("OpenAi")]
    [InlineData(null)]
    [InlineData("")]
    public void UserPreferencesValidator_AllValidProviders_Pass(string? provider)
    {
        var validator = new UpdateUserPreferencesCommandValidator();
        var command = new UpdateUserPreferencesCommand(true, provider, true, null, null);

        var result = validator.Validate(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void GovernancePolicyValidator_ValidCommand_Passes()
    {
        var validator = new UpdateGovernancePolicyCommandValidator();
        var command = new UpdateGovernancePolicyCommand(
            true, new List<string> { "Local", "Claude" }, "Local",
            new List<string> { "DocumentAnalysis" }, 100, 5000, true);

        var result = validator.Validate(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void GovernancePolicyValidator_InvalidProvider_Fails()
    {
        var validator = new UpdateGovernancePolicyCommandValidator();
        var command = new UpdateGovernancePolicyCommand(
            true, new List<string> { "InvalidProvider" }, null, null, null, null, true);

        var result = validator.Validate(command);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void GovernancePolicyValidator_InvalidDefaultProvider_Fails()
    {
        var validator = new UpdateGovernancePolicyCommandValidator();
        var command = new UpdateGovernancePolicyCommand(
            true, null, "InvalidProvider", null, null, null, true);

        var result = validator.Validate(command);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void GovernancePolicyValidator_ZeroDailyLimit_Fails()
    {
        var validator = new UpdateGovernancePolicyCommandValidator();
        var command = new UpdateGovernancePolicyCommand(
            true, null, null, null, 0, null, true);

        var result = validator.Validate(command);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void GovernancePolicyValidator_NegativeMonthlyLimit_Fails()
    {
        var validator = new UpdateGovernancePolicyCommandValidator();
        var command = new UpdateGovernancePolicyCommand(
            true, null, null, null, null, -100, true);

        var result = validator.Validate(command);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void GovernancePolicyValidator_NullLimits_Passes()
    {
        var validator = new UpdateGovernancePolicyCommandValidator();
        var command = new UpdateGovernancePolicyCommand(
            true, null, null, null, null, null, true);

        var result = validator.Validate(command);

        Assert.True(result.IsValid);
    }
}
