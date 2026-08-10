namespace BerexQms.Infrastructure.AiEngine.Configuration;

/// <summary>
/// Root AI configuration section. API keys must come from environment
/// variables or secret storage — NEVER committed to source control.
/// </summary>
public sealed class AiProviderOptions
{
    public const string SectionName = "AiProviders";

    public ClaudeProviderOptions Claude { get; set; } = new();
    public OpenAiProviderOptions OpenAi { get; set; } = new();
    public LocalProviderOptions Local { get; set; } = new();

    /// <summary>
    /// Task-to-provider routing. Key = AiTaskType name,
    /// Value = ordered list of provider names (first = primary, remainder = fallback chain).
    /// When only a single provider name is present it behaves identically to the Sprint 16
    /// two-provider TaskMappings/FallbackMappings scheme.
    /// </summary>
    public Dictionary<string, List<string>> ProviderRouting { get; set; } = new()
    {
        ["DocumentAnalysis"] = ["Local", "Claude", "OpenAi"],
        ["QualityAnalysis"] = ["Local", "OpenAi", "Claude"],
        ["RecommendationGeneration"] = ["Claude", "Local", "OpenAi"],
        ["Summarization"] = ["Local", "OpenAi", "Claude"],
        ["RiskAnalysis"] = ["Local", "Claude", "OpenAi"],
        ["CAPAAnalysis"] = ["Claude", "Local", "OpenAi"],
        ["SupplierAnalysis"] = ["Local", "Claude", "OpenAi"],
        ["AuditAnalysis"] = ["Local", "Claude", "OpenAi"],
        ["DefectTrendAnalysis"] = ["Local", "OpenAi", "Claude"],
        ["StructuredDataExtraction"] = ["Local", "OpenAi", "Claude"],
    };

    /// <summary>
    /// Legacy Task-to-provider mappings — primary provider per task.
    /// Retained for backward compatibility; <see cref="ProviderRouting"/>
    /// takes precedence when populated.
    /// </summary>
    public Dictionary<string, string> TaskMappings { get; set; } = new()
    {
        ["DocumentAnalysis"] = "Claude",
        ["QualityAnalysis"] = "OpenAi",
        ["RecommendationGeneration"] = "OpenAi",
        ["Summarization"] = "OpenAi",
        ["RiskAnalysis"] = "OpenAi",
        ["CAPAAnalysis"] = "Claude",
        ["SupplierAnalysis"] = "Claude",
        ["AuditAnalysis"] = "Claude",
        ["DefectTrendAnalysis"] = "OpenAi",
        ["StructuredDataExtraction"] = "OpenAi",
    };

    /// <summary>
    /// Legacy task-to-fallback mappings.
    /// Retained for backward compatibility; <see cref="ProviderRouting"/>
    /// takes precedence when populated.
    /// </summary>
    public Dictionary<string, string> FallbackMappings { get; set; } = new()
    {
        ["DocumentAnalysis"] = "OpenAi",
        ["QualityAnalysis"] = "Claude",
        ["RecommendationGeneration"] = "Claude",
        ["Summarization"] = "Claude",
        ["RiskAnalysis"] = "Claude",
        ["CAPAAnalysis"] = "OpenAi",
        ["SupplierAnalysis"] = "OpenAi",
        ["AuditAnalysis"] = "OpenAi",
        ["DefectTrendAnalysis"] = "Claude",
        ["StructuredDataExtraction"] = "Claude",
    };

    /// <summary>Maximum total context characters sent to any provider.</summary>
    public int MaxContextSizeChars { get; set; } = 50_000;

    /// <summary>Maximum context characters for the Local provider (smaller context window).</summary>
    public int MaxLocalContextSizeChars { get; set; } = 12_000;

    /// <summary>Maximum number of providers to attempt in the fallback chain per request.</summary>
    public int MaxFallbackChain { get; set; } = 3;
}

public sealed class ClaudeProviderOptions
{
    public bool Enabled { get; set; }
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "claude-sonnet-4-20250514";
    public string BaseUrl { get; set; } = "https://api.anthropic.com";
    public int TimeoutSeconds { get; set; } = 60;
    public int MaxTokens { get; set; } = 4096;
    public int MaxRetries { get; set; } = 3;
    public decimal InputTokenCostPer1K { get; set; } = 0.003m;
    public decimal OutputTokenCostPer1K { get; set; } = 0.015m;
}

public sealed class OpenAiProviderOptions
{
    public bool Enabled { get; set; }
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "gpt-4o";
    public string BaseUrl { get; set; } = "https://api.openai.com";
    public int TimeoutSeconds { get; set; } = 60;
    public int MaxTokens { get; set; } = 4096;
    public decimal Temperature { get; set; } = 0.1m;
    public int MaxRetries { get; set; } = 3;
    public decimal InputTokenCostPer1K { get; set; } = 0.005m;
    public decimal OutputTokenCostPer1K { get; set; } = 0.015m;
}

/// <summary>
/// Configuration for the Local AI provider (Ollama-compatible).
/// No API key is required by default. The BaseUrl must point to a running
/// Ollama instance. Model availability is validated at runtime via health checks.
/// </summary>
public sealed class LocalProviderOptions
{
    public bool Enabled { get; set; }
    public string BaseUrl { get; set; } = "http://localhost:11434";
    public string DefaultModel { get; set; } = "qwen3";
    public int TimeoutSeconds { get; set; } = 120;
    public int HealthCheckTimeoutSeconds { get; set; } = 5;
    public int MaxTokens { get; set; } = 4096;
    public decimal Temperature { get; set; } = 0.1m;
    public int MaxRetries { get; set; } = 1;

    /// <summary>
    /// Optional per-task model overrides. Key = AiTaskType name,
    /// Value = Ollama model name. Falls back to <see cref="DefaultModel"/>
    /// when no override is configured.
    /// </summary>
    public Dictionary<string, string> TaskModels { get; set; } = new();
}
