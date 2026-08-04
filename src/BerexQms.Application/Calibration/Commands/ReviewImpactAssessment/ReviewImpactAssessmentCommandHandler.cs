using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.Interfaces;
using BerexQms.Domain.Calibration.Repositories;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.Calibration.Commands.ReviewImpactAssessment;

internal sealed class ReviewImpactAssessmentCommandHandler
    : ICommandHandler<ReviewImpactAssessmentCommand>
{
    private readonly IEquipmentRepository _repository;
    private readonly ICurrentUserService _currentUserService;

    public ReviewImpactAssessmentCommandHandler(
        IEquipmentRepository repository,
        ICurrentUserService currentUserService)
    {
        _repository = repository;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(
        ReviewImpactAssessmentCommand request, CancellationToken cancellationToken)
    {
        var assessment = await _repository.GetImpactAssessmentByIdAsync(request.AssessmentId, cancellationToken);
        if (assessment is null)
            return Result.Failure(CalibrationErrors.AssessmentNotFound);

        switch (request.Action.ToUpperInvariant())
        {
            case "REVIEW":
                assessment.StartReview(_currentUserService.UserId);
                break;
            case "CLOSE":
                assessment.Close(request.Notes);
                break;
            default:
                return Result.Failure(CalibrationErrors.InvalidStatus);
        }

        return Result.Success();
    }
}
