using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Domain.Inspection.Repositories;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.Inspection.Commands.StartInspection;

public sealed class StartInspectionCommandHandler : ICommandHandler<StartInspectionCommand>
{
    private readonly IInspectionRepository _inspectionRepository;

    public StartInspectionCommandHandler(IInspectionRepository inspectionRepository)
    {
        _inspectionRepository = inspectionRepository;
    }

    public async Task<Result> Handle(StartInspectionCommand request, CancellationToken cancellationToken)
    {
        var record = await _inspectionRepository.GetByIdAsync(request.InspectionId, cancellationToken);
        if (record is null)
            return Result.Failure(InspectionErrors.NotFound);

        record.StartInspection();
        await _inspectionRepository.UpdateAsync(record, cancellationToken);

        return Result.Success();
    }
}
