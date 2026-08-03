using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Domain.Capa.Repositories;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.Capa.Commands.AssignCapa;

public sealed class AssignCapaCommandHandler : ICommandHandler<AssignCapaCommand>
{
    private readonly ICAPARepository _repository;

    public AssignCapaCommandHandler(ICAPARepository repository)
    {
        _repository = repository;
    }

    public async Task<Result> Handle(AssignCapaCommand request, CancellationToken cancellationToken)
    {
        var record = await _repository.GetByIdAsync(request.CapaId, cancellationToken);
        if (record is null)
            return Result.Failure(CAPAErrors.NotFound);

        record.AssignTo(request.AssigneeId);
        await _repository.UpdateAsync(record, cancellationToken);

        return Result.Success();
    }
}
