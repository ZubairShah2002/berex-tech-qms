using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Domain.Capa.Enums;
using BerexQms.Domain.Capa.Repositories;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.Capa.Commands.StartRCA;

public sealed class StartRCACommandHandler : ICommandHandler<StartRCACommand>
{
    private readonly ICAPARepository _repository;

    public StartRCACommandHandler(ICAPARepository repository)
    {
        _repository = repository;
    }

    public async Task<Result> Handle(StartRCACommand request, CancellationToken cancellationToken)
    {
        var record = await _repository.GetByIdAsync(request.CapaId, cancellationToken);
        if (record is null)
            return Result.Failure(CAPAErrors.NotFound);

        if (!Enum.TryParse<RCAMethodology>(request.Methodology, true, out var methodology))
            return Result.Failure(Error.Validation("CAPA.InvalidMethodology", $"Invalid RCA methodology: {request.Methodology}."));

        record.StartRCA(methodology, request.AnalystId);
        await _repository.UpdateAsync(record, cancellationToken);

        return Result.Success();
    }
}
