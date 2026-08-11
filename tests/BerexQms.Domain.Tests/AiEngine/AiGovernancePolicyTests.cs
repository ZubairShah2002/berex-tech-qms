using BerexQms.Domain.AiEngine.Entities;
using BerexQms.Domain.AiEngine.Events;
using BerexQms.SharedKernel.Exceptions;
using BerexQms.SharedKernel.ValueObjects;

namespace BerexQms.Domain.Tests.AiEngine;

public class AiGovernancePolicyTests
{
    private static readonly TenantId TestTenantId = TenantId.From(Guid.NewGuid());

    [Fact]
    public void Create_DefaultValues_AiEnabledConfirmationRequired()
    {
        var id = Guid.NewGuid();
        var policy = AiGovernancePolicy.Create(id, TestTenantId);

        Assert.Equal(id, policy.Id);
        Assert.Equal(TestTenantId, policy.TenantId);
        Assert.True(policy.IsAiEnabled);
        Assert.Null(policy.AllowedProvidersJson);
        Assert.Null(policy.DefaultProvider);
        Assert.Null(policy.AllowedTaskTypesJson);
        Assert.Null(policy.MaxDailyRequestsPerUser);
        Assert.Null(policy.MaxMonthlyRequestsPerTenant);
        Assert.True(policy.RequireHumanConfirmation);
    }

    [Fact]
    public void Create_RaisesDomainEvent()
    {
        var policy = AiGovernancePolicy.Create(Guid.NewGuid(), TestTenantId);

        Assert.Single(policy.DomainEvents);
        Assert.IsType<AiGovernancePolicyChangedEvent>(policy.DomainEvents.First());
    }

    [Fact]
    public void SetAiEnabled_False_DisablesAi()
    {
        var policy = AiGovernancePolicy.Create(Guid.NewGuid(), TestTenantId);
        policy.ClearDomainEvents();

        policy.SetAiEnabled(false);

        Assert.False(policy.IsAiEnabled);
        Assert.Single(policy.DomainEvents);
    }

    [Fact]
    public void SetAiEnabled_SameValue_NoEvent()
    {
        var policy = AiGovernancePolicy.Create(Guid.NewGuid(), TestTenantId);
        policy.ClearDomainEvents();

        policy.SetAiEnabled(true); // Same as default

        Assert.Empty(policy.DomainEvents);
    }

    [Fact]
    public void SetAllowedProviders_SetsJson()
    {
        var policy = AiGovernancePolicy.Create(Guid.NewGuid(), TestTenantId);
        policy.ClearDomainEvents();

        policy.SetAllowedProviders("[\"Local\",\"Claude\"]");

        Assert.Equal("[\"Local\",\"Claude\"]", policy.AllowedProvidersJson);
        Assert.Single(policy.DomainEvents);
    }

    [Fact]
    public void SetAllowedProviders_Null_AllowsAll()
    {
        var policy = AiGovernancePolicy.Create(Guid.NewGuid(), TestTenantId);
        policy.SetAllowedProviders("[\"Local\"]");
        policy.ClearDomainEvents();

        policy.SetAllowedProviders(null);

        Assert.Null(policy.AllowedProvidersJson);
    }

    [Fact]
    public void SetDefaultProvider_SetsValue()
    {
        var policy = AiGovernancePolicy.Create(Guid.NewGuid(), TestTenantId);

        policy.SetDefaultProvider("Local");

        Assert.Equal("Local", policy.DefaultProvider);
    }

    [Fact]
    public void SetDefaultProvider_WhitespaceTrimsToNull()
    {
        var policy = AiGovernancePolicy.Create(Guid.NewGuid(), TestTenantId);

        policy.SetDefaultProvider("   ");

        Assert.Null(policy.DefaultProvider);
    }

    [Fact]
    public void SetAllowedTaskTypes_SetsJsonAndRaisesEvent()
    {
        var policy = AiGovernancePolicy.Create(Guid.NewGuid(), TestTenantId);
        policy.ClearDomainEvents();

        policy.SetAllowedTaskTypes("[\"DocumentAnalysis\"]");

        Assert.Equal("[\"DocumentAnalysis\"]", policy.AllowedTaskTypesJson);
        Assert.Single(policy.DomainEvents);
    }

    [Fact]
    public void SetMaxDailyRequestsPerUser_PositiveValue_Succeeds()
    {
        var policy = AiGovernancePolicy.Create(Guid.NewGuid(), TestTenantId);

        policy.SetMaxDailyRequestsPerUser(100);

        Assert.Equal(100, policy.MaxDailyRequestsPerUser);
    }

    [Fact]
    public void SetMaxDailyRequestsPerUser_Null_SetsUnlimited()
    {
        var policy = AiGovernancePolicy.Create(Guid.NewGuid(), TestTenantId);
        policy.SetMaxDailyRequestsPerUser(50);

        policy.SetMaxDailyRequestsPerUser(null);

        Assert.Null(policy.MaxDailyRequestsPerUser);
    }

    [Fact]
    public void SetMaxDailyRequestsPerUser_Zero_ThrowsDomainException()
    {
        var policy = AiGovernancePolicy.Create(Guid.NewGuid(), TestTenantId);

        Assert.Throws<DomainException>(() => policy.SetMaxDailyRequestsPerUser(0));
    }

    [Fact]
    public void SetMaxDailyRequestsPerUser_Negative_ThrowsDomainException()
    {
        var policy = AiGovernancePolicy.Create(Guid.NewGuid(), TestTenantId);

        Assert.Throws<DomainException>(() => policy.SetMaxDailyRequestsPerUser(-5));
    }

    [Fact]
    public void SetMaxMonthlyRequestsPerTenant_PositiveValue_Succeeds()
    {
        var policy = AiGovernancePolicy.Create(Guid.NewGuid(), TestTenantId);

        policy.SetMaxMonthlyRequestsPerTenant(5000);

        Assert.Equal(5000, policy.MaxMonthlyRequestsPerTenant);
    }

    [Fact]
    public void SetMaxMonthlyRequestsPerTenant_Zero_ThrowsDomainException()
    {
        var policy = AiGovernancePolicy.Create(Guid.NewGuid(), TestTenantId);

        Assert.Throws<DomainException>(() => policy.SetMaxMonthlyRequestsPerTenant(0));
    }

    [Fact]
    public void SetRequireHumanConfirmation_False_StillEnforcesTrue()
    {
        var policy = AiGovernancePolicy.Create(Guid.NewGuid(), TestTenantId);

        policy.SetRequireHumanConfirmation(false);

        Assert.True(policy.RequireHumanConfirmation);
    }

    [Fact]
    public void SetRequireHumanConfirmation_True_RemainsTrue()
    {
        var policy = AiGovernancePolicy.Create(Guid.NewGuid(), TestTenantId);

        policy.SetRequireHumanConfirmation(true);

        Assert.True(policy.RequireHumanConfirmation);
    }
}
