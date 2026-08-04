using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.SupplierQuality.DTOs;
using BerexQms.Domain.SupplierQuality.Repositories;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.SupplierQuality.Commands.AddApproval;

internal sealed class AddApprovalCommandHandler : ICommandHandler<AddApprovalCommand, SupplierApprovalDto>
{
    private readonly ISupplierRepository _repository;

    public AddApprovalCommandHandler(ISupplierRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<SupplierApprovalDto>> Handle(
        AddApprovalCommand request, CancellationToken cancellationToken)
    {
        var supplier = await _repository.GetWithApprovalsAsync(request.SupplierId, cancellationToken);
        if (supplier is null)
            return SupplierErrors.NotFound;

        var approval = supplier.AddApproval(
            request.ScopeDescription,
            request.ApprovedDate,
            request.ExpiryDate,
            request.Conditions);

        supplier.Approve(request.ApprovedDate);
        await _repository.UpdateAsync(supplier, cancellationToken);

        return new SupplierApprovalDto(
            approval.Id,
            approval.ScopeDescription,
            approval.ApprovedDate,
            approval.ExpiryDate,
            approval.Conditions,
            approval.IsActive);
    }
}
