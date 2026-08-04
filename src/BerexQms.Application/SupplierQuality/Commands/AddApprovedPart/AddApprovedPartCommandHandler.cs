using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.SupplierQuality.DTOs;
using BerexQms.Domain.SupplierQuality.Repositories;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.SupplierQuality.Commands.AddApprovedPart;

internal sealed class AddApprovedPartCommandHandler : ICommandHandler<AddApprovedPartCommand, ApprovedPartDto>
{
    private readonly ISupplierRepository _repository;

    public AddApprovedPartCommandHandler(ISupplierRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<ApprovedPartDto>> Handle(
        AddApprovedPartCommand request, CancellationToken cancellationToken)
    {
        var supplier = await _repository.GetFullDetailAsync(request.SupplierId, cancellationToken);
        if (supplier is null)
            return SupplierErrors.NotFound;

        var part = supplier.AddApprovedPart(request.PartId, request.RevisionScope, request.ApprovalDate);
        await _repository.UpdateAsync(supplier, cancellationToken);

        return new ApprovedPartDto(
            part.Id,
            part.PartId,
            part.RevisionScope,
            part.ApprovalDate,
            part.IsActive);
    }
}
