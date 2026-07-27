using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.Interfaces;
using BerexQms.Domain.NonConformance.Repositories;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.NonConformance.Commands.VerifyContainment;

public sealed class VerifyContainmentCommandHandler : ICommandHandler<VerifyContainmentCommand>
{
    private readonly INonConformanceRepository _repository;
    private readonly ICurrentUserService _currentUserService;

    public VerifyContainmentCommandHandler(
        INonConformanceRepository repository,
        ICurrentUserService currentUserService)
    {
        _repository = repository;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(VerifyContainmentCommand request, CancellationToken cancellationToken)
    {
        var record = await _repository.GetWithContainmentsAsync(request.NonConformanceId, cancellationToken);
        if (record is null)
            return Result.Failure(NonConformanceErrors.NotFound);

        record.VerifyContainment(request.ContainmentActionId, _currentUserService.Email);
        await _repository.UpdateAsync(record, cancellationToken);

        return Result.Success();
    }
}
