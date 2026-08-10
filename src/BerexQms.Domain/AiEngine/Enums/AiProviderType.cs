namespace BerexQms.Domain.AiEngine.Enums;

/// <summary>
/// Supported AI provider types. Provider selection is determined by
/// the orchestrator based on task type and configuration — never hardcoded
/// in business logic.
/// </summary>
public enum AiProviderType
{
    Claude = 1,
    OpenAi = 2,
    Local = 3,
}
