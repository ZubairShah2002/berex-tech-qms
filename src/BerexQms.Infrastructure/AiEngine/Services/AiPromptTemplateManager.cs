using BerexQms.Domain.AiEngine.Repositories;

namespace BerexQms.Infrastructure.AiEngine.Services;

/// <summary>
/// Centralized prompt management. Provides system and user prompts
/// for each task type from the database, with hardened defaults
/// that instruct the model to:
/// - Use provided evidence only
/// - Not invent facts
/// - Identify uncertainty
/// - Provide supporting evidence
/// - Provide confidence levels
/// - Recommend rather than decide
/// - Never directly execute actions
/// </summary>
internal sealed class AiPromptTemplateManager
{
    private readonly IAiPromptTemplateRepository _repository;

    public AiPromptTemplateManager(IAiPromptTemplateRepository repository)
    {
        _repository = repository;
    }

    public async Task<(string SystemPrompt, string UserPrompt, string? OutputSchema)> GetPromptsAsync(
        string taskType, string? module, string? content, CancellationToken ct)
    {
        var template = await _repository.GetActiveByTaskTypeAsync(taskType, ct);

        if (template != null)
        {
            var userPrompt = template.UserPromptTemplate
                .Replace("{{MODULE}}", module ?? "All Modules")
                .Replace("{{CONTENT}}", content ?? string.Empty);

            return (template.SystemPrompt, userPrompt, template.OutputSchema);
        }

        // Default hardened prompts
        return (GetDefaultSystemPrompt(), GetDefaultUserPrompt(taskType, module, content), GetDefaultOutputSchema());
    }

    private static string GetDefaultSystemPrompt() =>
        """
        You are a Quality Management System (QMS) analyst for a discrete manufacturing enterprise.
        You operate within strict governance rules:

        RULES:
        1. Base all analysis ONLY on the provided QMS context data.
        2. DO NOT invent facts or data points not present in the context.
        3. Clearly distinguish between:
           - FACT: Directly verifiable from provided data
           - INFERENCE: Derived from data but not directly confirmed
           - RECOMMENDATION: Suggested action based on analysis
           - UNKNOWN: Cannot be determined from available data
        4. Always provide a confidence score (0.0 to 1.0) for each finding.
        5. Always cite supporting evidence from the provided context.
        6. RECOMMEND actions — never DECIDE or EXECUTE.
        7. Identify areas of uncertainty explicitly.
        8. Never suggest bypassing quality procedures, audits, or approvals.
        9. Respond in structured JSON when a schema is provided.
        10. If insufficient data is available, state this clearly rather than guessing.
        """;

    private static string GetDefaultUserPrompt(string taskType, string? module, string? content)
    {
        var moduleClause = module != null ? $" for the {module} module" : string.Empty;
        var contentClause = !string.IsNullOrWhiteSpace(content) ? $"\n\nAdditional context:\n{content}" : string.Empty;

        return taskType switch
        {
            "DocumentAnalysis" =>
                $"Analyse the provided QMS documents{moduleClause}. Identify compliance gaps, missing procedures, outdated references, and areas requiring attention.{contentClause}",
            "QualityAnalysis" =>
                $"Analyse the quality data{moduleClause}. Identify trends, patterns, anomalies, and areas of concern. Provide data-driven quality insights.{contentClause}",
            "RecommendationGeneration" =>
                $"Based on the QMS context{moduleClause}, generate prioritised quality recommendations. Each recommendation must include: type, title, severity, reason, supporting evidence, confidence score, and recommended action.{contentClause}",
            "Summarization" =>
                $"Summarise the key quality information{moduleClause}. Focus on: current status, critical issues, trends, and required actions.{contentClause}",
            "RiskAnalysis" =>
                $"Perform a risk assessment{moduleClause}. Identify risks, rate severity, assess likelihood, and recommend mitigation actions.{contentClause}",
            "CAPAAnalysis" =>
                $"Analyse corrective and preventive action data{moduleClause}. Evaluate CAPA effectiveness, identify recurring issues, and suggest improvements.{contentClause}",
            "SupplierAnalysis" =>
                $"Analyse supplier quality data{moduleClause}. Evaluate supplier performance, identify risk suppliers, and recommend corrective actions.{contentClause}",
            "AuditAnalysis" =>
                $"Analyse audit findings and results{moduleClause}. Identify compliance gaps, recurring findings, and recommend audit focus areas.{contentClause}",
            "DefectTrendAnalysis" =>
                $"Analyse defect trends{moduleClause}. Identify patterns, increasing/decreasing trends, correlations, and recommend preventive measures.{contentClause}",
            "StructuredDataExtraction" =>
                $"Extract structured quality data from the provided context{moduleClause}. Return data in the specified format.{contentClause}",
            _ =>
                $"Analyse the provided QMS data{moduleClause} and provide quality insights with supporting evidence.{contentClause}",
        };
    }

    private static string GetDefaultOutputSchema() =>
        """
        [
          {
            "recommendationType": "string (DefectTrend|SupplierRisk|ProcessRisk|DocumentGap|AuditRisk|CAPARecommendation)",
            "title": "string",
            "severity": "string (Low|Medium|High|Critical)",
            "reason": "string",
            "supportingEvidence": "string",
            "confidenceScore": 0.0,
            "recommendedAction": "string",
            "requiresHumanReview": true,
            "confidenceCategory": "string (Fact|Inference|Recommendation|Unknown)"
          }
        ]
        """;
}
