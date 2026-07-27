using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.Interfaces;
using BerexQms.Domain.NonConformance.Repositories;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.NonConformance.Commands.CloseAsDuplicate;

public sealed class CloseAsDuplicateCommandHandler : ICommandHandler<CloseAsDuplicateCommand>
{
    private readonly INonConformanceRepository _repository;
    private readonly ICurrentUserService _currentUserService;

    public CloseAsDuplicateCommandHandler(
        INonConformanceRepository repository,
        ICurrentUserService currentUserService)
    {
        _repository = repository;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(CloseAsDuplicateCommand request, CancellationToken cancellationToken)
    {
        var record = await _repository.GetByIdAsync(request.NonConformanceId, cancellationToken);
        if (record is null)
            return Result.Failure(NonConformanceErrors.NotFound);

        record.CloseAsDuplicate(_currentUserService.Email, request.Notes);
        await _repository.UpdateAsync(record, cancellationToken);

        return Result.Success();
    }
}
