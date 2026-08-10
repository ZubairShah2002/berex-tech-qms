using BerexQms.Application.AiEngine.DTOs;
using BerexQms.Application.Abstractions.Messaging;

namespace BerexQms.Application.AiEngine.Commands.ExecuteAiAnalysis;

/// <summary>
/// Executes an AI analysis task through the orchestrator pipeline:
/// Permission Check → Context Retrieval → Provider Selection →
/// AI Call → Response Validation → Usage Recording.
/// </summary>
public sealed record ExecuteAiAnalysisCommand(
    string TaskType,
    string? Module,
    string? Content,
    string? EntityId,
    int MaxContextDocuments = 10) : ICommand<AiProviderResponseDto>;
