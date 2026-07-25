using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.Interfaces;
using BerexQms.Domain.Inspection.Repositories;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.Inspection.Commands.RejectInspection;

public sealed class RejectInspectionCommandHandler : ICommandHandler<RejectInspectionCommand>
{
    private readonly IInspectionRepository _inspectionRepository;
    private readonly ICurrentUserService _currentUserService;

    public RejectInspectionCommandHandler(
        IInspectionRepository inspectionRepository,
        ICurrentUserService currentUserService)
    {
        _inspectionRepository = inspectionRepository;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(RejectInspectionCommand request, CancellationToken cancellationToken)
    {
        var record = await _inspectionRepository.GetByIdAsync(request.InspectionId, cancellationToken);
        if (record is null)
            return Result.Failure(InspectionErrors.NotFound);

        record.Reject(_currentUserService.Email, request.Notes);
        await _inspectionRepository.UpdateAsync(record, cancellationToken);

        return Result.Success();
    }
}
