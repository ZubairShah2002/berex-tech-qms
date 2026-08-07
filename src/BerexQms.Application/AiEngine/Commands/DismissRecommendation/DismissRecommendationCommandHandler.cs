using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Domain.AiEngine.Repositories;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.AiEngine.Commands.DismissRecommendation;

internal sealed class DismissRecommendationCommandHandler
    : ICommandHandler<DismissRecommendationCommand>
{
    private readonly IAiRecommendationRepository _repository;

    public DismissRecommendationCommandHandler(IAiRecommendationRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result> Handle(
        DismissRecommendationCommand request, CancellationToken cancellationToken)
    {
        var recommendation = await _repository.GetByIdAsync(request.RecommendationId, cancellationToken);
        if (recommendation is null)
            return Result.Failure(AiEngineErrors.RecommendationNotFound);

        recommendation.MarkExpired();
        await _repository.UpdateAsync(recommendation, cancellationToken);

        return Result.Success();
    }
}
