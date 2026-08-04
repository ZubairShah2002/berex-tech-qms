using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.SupplierQuality.DTOs;
using BerexQms.Domain.SupplierQuality.Repositories;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.SupplierQuality.Commands.CreateScorecard;

internal sealed class CreateScorecardCommandHandler : ICommandHandler<CreateScorecardCommand, SupplierScorecardDto>
{
    private readonly ISupplierRepository _repository;

    public CreateScorecardCommandHandler(ISupplierRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<SupplierScorecardDto>> Handle(
        CreateScorecardCommand request, CancellationToken cancellationToken)
    {
        var supplier = await _repository.GetByIdAsync(request.SupplierId, cancellationToken);
        if (supplier is null)
            return SupplierErrors.NotFound;

        var scorecard = supplier.CreateScorecard(
            request.PeriodStart,
            request.PeriodEnd,
            request.QualityScore,
            request.DeliveryScore,
            request.ResponsivenessScore,
            request.CostScore);

        await _repository.UpdateAsync(supplier, cancellationToken);

        return new SupplierScorecardDto(
            scorecard.Id,
            scorecard.PeriodStart,
            scorecard.PeriodEnd,
            scorecard.QualityScore,
            scorecard.DeliveryScore,
            scorecard.ResponsivenessScore,
            scorecard.CostScore,
            scorecard.OverallScore,
            scorecard.Status);
    }
}
