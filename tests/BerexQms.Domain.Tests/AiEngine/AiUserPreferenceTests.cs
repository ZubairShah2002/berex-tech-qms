using BerexQms.Domain.AiEngine.Entities;
using BerexQms.Domain.AiEngine.Events;
using BerexQms.SharedKernel.ValueObjects;

namespace BerexQms.Domain.Tests.AiEngine;

public class AiUserPreferenceTests
{
    private static readonly TenantId TestTenantId = TenantId.From(Guid.NewGuid());

    [Fact]
    public void Create_DefaultValues_AiEnabledConfirmationRequired()
    {
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var preference = AiUserPreference.Create(id, TestTenantId, userId);

        Assert.Equal(id, preference.Id);
        Assert.Equal(TestTenantId, preference.TenantId);
        Assert.Equal(userId, preference.UserId);
        Assert.True(preference.IsAiEnabled);
        Assert.Null(preference.PreferredProvider);
        Assert.True(preference.RequireConfirmationForRecommendations);
        Assert.Null(preference.PreferredLanguage);
        Assert.Null(preference.EnabledTaskTypesJson);
    }

    [Fact]
    public void Create_RaisesDomainEvent()
    {
        var preference = AiUserPreference.Create(Guid.NewGuid(), TestTenantId, Guid.NewGuid());

        Assert.Single(preference.DomainEvents);
        Assert.IsType<AiUserPreferenceChangedEvent>(preference.DomainEvents.First());
    }

    [Fact]
    public void SetAiEnabled_False_DisablesAiAndRaisesEvent()
    {
        var preference = AiUserPreference.Create(Guid.NewGuid(), TestTenantId, Guid.NewGuid());
        preference.ClearDomainEvents();

        preference.SetAiEnabled(false);

        Assert.False(preference.IsAiEnabled);
        Assert.Single(preference.DomainEvents);
    }

    [Fact]
    public void SetAiEnabled_SameValue_NoEvent()
    {
        var preference = AiUserPreference.Create(Guid.NewGuid(), TestTenantId, Guid.NewGuid());
        preference.ClearDomainEvents();

        preference.SetAiEnabled(true); // Same as default

        Assert.Empty(preference.DomainEvents);
    }

    [Fact]
    public void SetPreferredProvider_ValidName_SetsProvider()
    {
        var preference = AiUserPreference.Create(Guid.NewGuid(), TestTenantId, Guid.NewGuid());
        preference.ClearDomainEvents();

        preference.SetPreferredProvider("Claude");

        Assert.Equal("Claude", preference.PreferredProvider);
        Assert.Single(preference.DomainEvents);
    }

    [Fact]
    public void SetPreferredProvider_Null_ResetsToAutomatic()
    {
        var preference = AiUserPreference.Create(Guid.NewGuid(), TestTenantId, Guid.NewGuid());
        preference.SetPreferredProvider("Claude");
        preference.ClearDomainEvents();

        preference.SetPreferredProvider(null);

        Assert.Null(preference.PreferredProvider);
        Assert.Single(preference.DomainEvents);
    }

    [Fact]
    public void SetPreferredProvider_EmptyString_ResetsToNull()
    {
        var preference = AiUserPreference.Create(Guid.NewGuid(), TestTenantId, Guid.NewGuid());
        preference.SetPreferredProvider("Claude");
        preference.ClearDomainEvents();

        preference.SetPreferredProvider("  ");

        Assert.Null(preference.PreferredProvider);
    }

    [Fact]
    public void SetPreferredProvider_SameValue_NoEvent()
    {
        var preference = AiUserPreference.Create(Guid.NewGuid(), TestTenantId, Guid.NewGuid());
        preference.SetPreferredProvider("Claude");
        preference.ClearDomainEvents();

        preference.SetPreferredProvider("Claude");

        Assert.Empty(preference.DomainEvents);
    }

    [Fact]
    public void SetConfirmationPreference_False_DisablesConfirmation()
    {
        var preference = AiUserPreference.Create(Guid.NewGuid(), TestTenantId, Guid.NewGuid());

        preference.SetConfirmationPreference(false);

        Assert.False(preference.RequireConfirmationForRecommendations);
    }

    [Fact]
    public void SetPreferredLanguage_SetsValue()
    {
        var preference = AiUserPreference.Create(Guid.NewGuid(), TestTenantId, Guid.NewGuid());

        preference.SetPreferredLanguage("en");

        Assert.Equal("en", preference.PreferredLanguage);
    }

    [Fact]
    public void SetPreferredLanguage_Whitespace_SetsNull()
    {
        var preference = AiUserPreference.Create(Guid.NewGuid(), TestTenantId, Guid.NewGuid());
        preference.SetPreferredLanguage("en");

        preference.SetPreferredLanguage("  ");

        Assert.Null(preference.PreferredLanguage);
    }

    [Fact]
    public void SetEnabledTaskTypes_SetsJsonValue()
    {
        var preference = AiUserPreference.Create(Guid.NewGuid(), TestTenantId, Guid.NewGuid());

        preference.SetEnabledTaskTypes("[\"DocumentAnalysis\",\"RiskAnalysis\"]");

        Assert.Equal("[\"DocumentAnalysis\",\"RiskAnalysis\"]", preference.EnabledTaskTypesJson);
    }

    [Fact]
    public void SetEnabledTaskTypes_Null_EnablesAll()
    {
        var preference = AiUserPreference.Create(Guid.NewGuid(), TestTenantId, Guid.NewGuid());
        preference.SetEnabledTaskTypes("[\"DocumentAnalysis\"]");

        preference.SetEnabledTaskTypes(null);

        Assert.Null(preference.EnabledTaskTypesJson);
    }
}
