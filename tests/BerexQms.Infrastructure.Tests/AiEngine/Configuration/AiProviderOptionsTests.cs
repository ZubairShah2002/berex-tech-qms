using BerexQms.Infrastructure.AiEngine.Configuration;

namespace BerexQms.Infrastructure.Tests.AiEngine.Configuration;

/// <summary>
/// Tests for AI provider configuration defaults and validation — Sprint 17.
/// </summary>
public class AiProviderOptionsTests
{
    [Fact]
    public void LocalProviderOptions_Defaults_AreReasonable()
    {
        var opts = new LocalProviderOptions();

        Assert.False(opts.Enabled);
        Assert.Equal("http://localhost:11434", opts.BaseUrl);
        Assert.Equal("qwen3", opts.DefaultModel);
        Assert.Equal(120, opts.TimeoutSeconds);
        Assert.Equal(5, opts.HealthCheckTimeoutSeconds);
        Assert.Equal(4096, opts.MaxTokens);
        Assert.Equal(0.1m, opts.Temperature);
        Assert.Equal(1, opts.MaxRetries);
        Assert.Empty(opts.TaskModels);
    }

    [Fact]
    public void AiProviderOptions_HasLocalSection()
    {
        var opts = new AiProviderOptions();

        Assert.NotNull(opts.Local);
        Assert.NotNull(opts.Claude);
        Assert.NotNull(opts.OpenAi);
    }

    [Fact]
    public void AiProviderOptions_ProviderRouting_DefaultsPopulated()
    {
        var opts = new AiProviderOptions();

        Assert.NotEmpty(opts.ProviderRouting);
        Assert.True(opts.ProviderRouting.ContainsKey("DocumentAnalysis"));
        Assert.True(opts.ProviderRouting.ContainsKey("QualityAnalysis"));
        Assert.True(opts.ProviderRouting.ContainsKey("Summarization"));
    }

    [Fact]
    public void AiProviderOptions_ProviderRouting_IncludesLocal()
    {
        var opts = new AiProviderOptions();

        foreach (var (taskType, chain) in opts.ProviderRouting)
        {
            Assert.True(chain.Contains("Local"),
                $"Task '{taskType}' routing chain should include Local provider");
        }
    }

    [Fact]
    public void AiProviderOptions_MaxLocalContextSizeChars_SmallerThanCloud()
    {
        var opts = new AiProviderOptions();

        Assert.True(opts.MaxLocalContextSizeChars < opts.MaxContextSizeChars,
            "Local context limit should be smaller than cloud limit");
        Assert.Equal(12_000, opts.MaxLocalContextSizeChars);
        Assert.Equal(50_000, opts.MaxContextSizeChars);
    }

    [Fact]
    public void AiProviderOptions_MaxFallbackChain_DefaultIs3()
    {
        var opts = new AiProviderOptions();

        Assert.Equal(3, opts.MaxFallbackChain);
    }

    [Fact]
    public void AiProviderOptions_LegacyMappings_StillPresent()
    {
        var opts = new AiProviderOptions();

        Assert.NotEmpty(opts.TaskMappings);
        Assert.NotEmpty(opts.FallbackMappings);
    }

    [Fact]
    public void LocalProviderOptions_NoApiKeyProperty()
    {
        // Local provider should NOT have an API key property —
        // verifies no secret storage concern for local inference
        var props = typeof(LocalProviderOptions).GetProperties();
        Assert.DoesNotContain(props, p =>
            p.Name.Contains("ApiKey", StringComparison.OrdinalIgnoreCase) ||
            p.Name.Contains("Secret", StringComparison.OrdinalIgnoreCase));
    }
}
