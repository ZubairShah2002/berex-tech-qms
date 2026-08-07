using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.AiEngine.DTOs;

namespace BerexQms.Application.AiEngine.Commands.RequestPrediction;

public sealed record RequestPredictionCommand(
    string Capability,
    string? InputContext,
    Guid? RelatedRecordId,
    string? RelatedRecordType) : ICommand<AiSuggestionDto>;
