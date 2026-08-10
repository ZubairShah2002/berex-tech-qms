using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.Interfaces;
using BerexQms.Domain.AiEngine.Repositories;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.AiEngine.Commands.ReviewRecommendation;

internal sealed class ReviewRecommendationCommandHandler
    : ICommandHandler<ReviewRecommendationCommand>
{
    private readonly IAiRecommendationRepository _repository;
    private readonly ICurrentUserService _currentUserService;

    public ReviewRecommendationCommandHandler(
        IAiRecommendationRepository repository,
        ICurrentUserService currentUserService)
    {
        _repository = repository;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(
        ReviewRecommendationCommand request, CancellationToken cancellationToken)
    {
        var recommendation = await _repository.GetByIdAsync(request.RecommendationId, cancellationToken);
        if (recommendation is null)
            return Result.Failure(AiEngineErrors.RecommendationNotFound);

        var userId = _currentUserService.UserId.ToString();

        switch (request.Action.ToLowerInvariant())
        {
            case "accept":
                recommendation.Accept(userId, request.Notes);
                break;
            case "reject":
                recommendation.Reject(userId, request.Notes);
                break;
            case "review":
                recommendation.MarkReviewed(userId);
                break;
            default:
                return Result.Failure(AiEngineErrors.InvalidReviewAction);
        }

        await _repository.UpdateAsync(recommendation, cancellationToken);

        return Result.Success();
    }
}
