using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Domain.Inspection.Repositories;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.Inspection.Commands.CancelInspection;

public sealed class CancelInspectionCommandHandler : ICommandHandler<CancelInspectionCommand>
{
    private readonly IInspectionRepository _inspectionRepository;

    public CancelInspectionCommandHandler(IInspectionRepository inspectionRepository)
    {
        _inspectionRepository = inspectionRepository;
    }

    public async Task<Result> Handle(CancelInspectionCommand request, CancellationToken cancellationToken)
    {
        var record = await _inspectionRepository.GetByIdAsync(request.InspectionId, cancellationToken);
        if (record is null)
            return Result.Failure(InspectionErrors.NotFound);

        record.Cancel();
        await _inspectionRepository.UpdateAsync(record, cancellationToken);

        return Result.Success();
    }
}
