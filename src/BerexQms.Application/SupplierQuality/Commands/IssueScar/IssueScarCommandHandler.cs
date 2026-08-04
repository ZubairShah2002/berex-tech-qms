using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.SupplierQuality.DTOs;
using BerexQms.Domain.SupplierQuality.Repositories;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.SupplierQuality.Commands.IssueScar;

internal sealed class IssueScarCommandHandler : ICommandHandler<IssueScarCommand, SCARRecordDto>
{
    private readonly ISupplierRepository _repository;

    public IssueScarCommandHandler(ISupplierRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<SCARRecordDto>> Handle(
        IssueScarCommand request, CancellationToken cancellationToken)
    {
        var supplier = await _repository.GetWithScarsAsync(request.SupplierId, cancellationToken);
        if (supplier is null)
            return SupplierErrors.NotFound;

        var scar = supplier.IssueScar(
            request.ScarNumber,
            request.NonConformanceId,
            request.DefectDescription,
            request.Severity,
            request.ResponseDays);

        await _repository.UpdateAsync(supplier, cancellationToken);

        return new SCARRecordDto(
            scar.Id,
            scar.ScarNumber,
            scar.NonConformanceId,
            scar.DefectDescription,
            scar.Severity,
            scar.IssuedDate,
            scar.ResponseDeadline,
            scar.Status,
            null, null, null, null);
    }
}
