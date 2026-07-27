using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.Interfaces;
using BerexQms.Domain.NonConformance.Repositories;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.NonConformance.Commands.ReopenNonConformance;

public sealed class ReopenNonConformanceCommandHandler : ICommandHandler<ReopenNonConformanceCommand>
{
    private readonly INonConformanceRepository _repository;
    private readonly ICurrentUserService _currentUserService;

    public ReopenNonConformanceCommandHandler(
        INonConformanceRepository repository,
        ICurrentUserService currentUserService)
    {
        _repository = repository;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(ReopenNonConformanceCommand request, CancellationToken cancellationToken)
    {
        var record = await _repository.GetByIdAsync(request.NonConformanceId, cancellationToken);
        if (record is null)
            return Result.Failure(NonConformanceErrors.NotFound);

        record.Reopen(_currentUserService.Email, request.Reason);
        await _repository.UpdateAsync(record, cancellationToken);

        return Result.Success();
    }
}
