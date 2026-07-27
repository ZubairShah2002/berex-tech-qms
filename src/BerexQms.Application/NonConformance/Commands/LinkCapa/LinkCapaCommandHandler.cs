using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Domain.NonConformance.Repositories;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.NonConformance.Commands.LinkCapa;

public sealed class LinkCapaCommandHandler : ICommandHandler<LinkCapaCommand>
{
    private readonly INonConformanceRepository _repository;

    public LinkCapaCommandHandler(INonConformanceRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result> Handle(LinkCapaCommand request, CancellationToken cancellationToken)
    {
        var record = await _repository.GetByIdAsync(request.NonConformanceId, cancellationToken);
        if (record is null)
            return Result.Failure(NonConformanceErrors.NotFound);

        record.LinkCapa(request.CapaId);
        await _repository.UpdateAsync(record, cancellationToken);

        return Result.Success();
    }
}
