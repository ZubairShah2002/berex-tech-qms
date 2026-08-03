using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.Interfaces;
using BerexQms.Domain.Capa.Repositories;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.Capa.Commands.CompleteAction;

public sealed class CompleteActionCommandHandler : ICommandHandler<CompleteActionCommand>
{
    private readonly ICAPARepository _repository;
    private readonly ICurrentUserService _currentUserService;

    public CompleteActionCommandHandler(
        ICAPARepository repository,
        ICurrentUserService currentUserService)
    {
        _repository = repository;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(CompleteActionCommand request, CancellationToken cancellationToken)
    {
        var record = await _repository.GetWithActionsAsync(request.CapaId, cancellationToken);
        if (record is null)
            return Result.Failure(CAPAErrors.NotFound);

        record.CompleteAction(
            request.ActionId, _currentUserService.Email,
            request.CompletionNotes, request.EvidenceProvided);

        await _repository.UpdateAsync(record, cancellationToken);

        return Result.Success();
    }
}
