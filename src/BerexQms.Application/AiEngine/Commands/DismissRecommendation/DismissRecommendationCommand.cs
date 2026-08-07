using BerexQms.Application.Abstractions.Messaging;

namespace BerexQms.Application.AiEngine.Commands.DismissRecommendation;

public sealed record DismissRecommendationCommand(Guid RecommendationId) : ICommand;
