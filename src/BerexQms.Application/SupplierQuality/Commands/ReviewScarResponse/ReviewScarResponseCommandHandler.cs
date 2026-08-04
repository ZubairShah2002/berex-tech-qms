using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Domain.SupplierQuality.Repositories;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.SupplierQuality.Commands.ReviewScarResponse;

internal sealed class ReviewScarResponseCommandHandler : ICommandHandler<ReviewScarResponseCommand>
{
    private readonly ISupplierRepository _repository;

    public ReviewScarResponseCommandHandler(ISupplierRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result> Handle(ReviewScarResponseCommand request, CancellationToken cancellationToken)
    {
        var supplier = await _repository.GetWithScarsAsync(request.SupplierId, cancellationToken);
        if (supplier is null)
            return Result.Failure(SupplierErrors.NotFound);

        switch (request.Decision.ToUpperInvariant())
        {
            case "ACCEPT":
                supplier.AcceptScarResponse(request.ScarId);
                break;
            case "REJECT":
                supplier.RejectScarResponse(request.ScarId);
                break;
            default:
                return Result.Failure(SupplierErrors.InvalidStatus);
        }

        await _repository.UpdateAsync(supplier, cancellationToken);
        return Result.Success();
    }
}
