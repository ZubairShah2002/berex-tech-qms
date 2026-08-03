using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.Interfaces;
using BerexQms.Domain.Capa.Repositories;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.Capa.Commands.RecordVerification;

public sealed class RecordVerificationCommandHandler : ICommandHandler<RecordVerificationCommand>
{
    private readonly ICAPARepository _repository;
    private readonly ICurrentUserService _currentUserService;

    public RecordVerificationCommandHandler(
        ICAPARepository repository,
        ICurrentUserService currentUserService)
    {
        _repository = repository;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(RecordVerificationCommand request, CancellationToken cancellationToken)
    {
        var record = await _repository.GetWithVerificationsAsync(request.CapaId, cancellationToken);
        if (record is null)
            return Result.Failure(CAPAErrors.NotFound);

        record.RecordVerification(
            request.VerificationId, _currentUserService.Email,
            request.IsEffective, request.Result, request.Evidence);

        await _repository.UpdateAsync(record, cancellationToken);

        return Result.Success();
    }
}
