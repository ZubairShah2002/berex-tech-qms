using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.Interfaces;
using BerexQms.Domain.Inspection.Enums;
using BerexQms.Domain.Inspection.Repositories;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.Inspection.Commands.SetDisposition;

public sealed class SetDispositionCommandHandler : ICommandHandler<SetDispositionCommand>
{
    private readonly IInspectionRepository _inspectionRepository;
    private readonly ICurrentUserService _currentUserService;

    public SetDispositionCommandHandler(
        IInspectionRepository inspectionRepository,
        ICurrentUserService currentUserService)
    {
        _inspectionRepository = inspectionRepository;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(SetDispositionCommand request, CancellationToken cancellationToken)
    {
        var record = await _inspectionRepository.GetByIdAsync(request.InspectionId, cancellationToken);
        if (record is null)
            return Result.Failure(InspectionErrors.NotFound);

        if (!Enum.TryParse<DispositionType>(request.Type, true, out var dispositionType))
            return Result.Failure(InspectionErrors.InvalidDispositionType);

        record.SetDisposition(dispositionType, request.Justification, _currentUserService.Email);
        await _inspectionRepository.UpdateAsync(record, cancellationToken);

        return Result.Success();
    }
}
