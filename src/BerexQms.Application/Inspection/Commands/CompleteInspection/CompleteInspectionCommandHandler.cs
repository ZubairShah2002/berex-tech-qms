using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.Interfaces;
using BerexQms.Domain.Inspection.Repositories;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.Inspection.Commands.CompleteInspection;

public sealed class CompleteInspectionCommandHandler : ICommandHandler<CompleteInspectionCommand>
{
    private readonly IInspectionRepository _inspectionRepository;
    private readonly ICurrentUserService _currentUserService;

    public CompleteInspectionCommandHandler(
        IInspectionRepository inspectionRepository,
        ICurrentUserService currentUserService)
    {
        _inspectionRepository = inspectionRepository;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(CompleteInspectionCommand request, CancellationToken cancellationToken)
    {
        var record = await _inspectionRepository.GetWithMeasurementsAsync(
            request.InspectionId, cancellationToken);
        if (record is null)
            return Result.Failure(InspectionErrors.NotFound);

        record.Complete(_currentUserService.Email);
        await _inspectionRepository.UpdateAsync(record, cancellationToken);

        return Result.Success();
    }
}
