using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.Interfaces;
using BerexQms.Domain.NonConformance.Enums;
using BerexQms.Domain.NonConformance.Repositories;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.NonConformance.Commands.RecordDisposition;

public sealed class RecordDispositionCommandHandler : ICommandHandler<RecordDispositionCommand>
{
    private readonly INonConformanceRepository _repository;
    private readonly ICurrentUserService _currentUserService;

    public RecordDispositionCommandHandler(
        INonConformanceRepository repository,
        ICurrentUserService currentUserService)
    {
        _repository = repository;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(RecordDispositionCommand request, CancellationToken cancellationToken)
    {
        var record = await _repository.GetByIdAsync(request.NonConformanceId, cancellationToken);
        if (record is null)
            return Result.Failure(NonConformanceErrors.NotFound);

        if (!Enum.TryParse<NCDispositionType>(request.Type, true, out var dispositionType))
            return Result.Failure(NonConformanceErrors.InvalidDispositionType);

        record.RecordDisposition(dispositionType, request.Justification, _currentUserService.Email);
        await _repository.UpdateAsync(record, cancellationToken);

        return Result.Success();
    }
}
