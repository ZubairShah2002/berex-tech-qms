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

    /// <summary>
    /// Task-to-provider mappings. Key = AiTaskType name,
    /// Value = provider name ("Claude" or "OpenAi").
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
    /// Task-to-fallback mappings. Key = AiTaskType name,
    /// Value = fallback provider name.
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
