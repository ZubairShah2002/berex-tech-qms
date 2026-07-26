using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.Interfaces;
using BerexQms.Domain.Inspection.Repositories;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.Inspection.Commands.ApproveInspection;

public sealed class ApproveInspectionCommandHandler : ICommandHandler<ApproveInspectionCommand>
{
    private readonly IInspectionRepository _inspectionRepository;
    private readonly ICurrentUserService _currentUserService;

    public ApproveInspectionCommandHandler(
        IInspectionRepository inspectionRepository,
        ICurrentUserService currentUserService)
    {
        _inspectionRepository = inspectionRepository;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(ApproveInspectionCommand request, CancellationToken cancellationToken)
    {
        var record = await _inspectionRepository.GetByIdAsync(request.InspectionId, cancellationToken);
        if (record is null)
            return Result.Failure(InspectionErrors.NotFound);

        record.Approve(_currentUserService.Email);
        await _inspectionRepository.UpdateAsync(record, cancellationToken);

        return Result.Success();
    }
}
