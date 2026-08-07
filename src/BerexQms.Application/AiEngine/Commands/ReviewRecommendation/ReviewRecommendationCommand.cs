using BerexQms.Application.Abstractions.Messaging;

namespace BerexQms.Application.AiEngine.Commands.ReviewRecommendation;

public sealed record ReviewRecommendationCommand(
    Guid RecommendationId,
    string Action,
    string? Notes) : ICommand;
