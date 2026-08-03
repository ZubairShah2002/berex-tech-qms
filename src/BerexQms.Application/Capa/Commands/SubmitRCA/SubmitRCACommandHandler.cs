using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Domain.Capa.Repositories;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.Capa.Commands.SubmitRCA;

public sealed class SubmitRCACommandHandler : ICommandHandler<SubmitRCACommand>
{
    private readonly ICAPARepository _repository;

    public SubmitRCACommandHandler(ICAPARepository repository)
    {
        _repository = repository;
    }

    public async Task<Result> Handle(SubmitRCACommand request, CancellationToken cancellationToken)
    {
        var record = await _repository.GetFullDetailAsync(request.CapaId, cancellationToken);
        if (record is null)
            return Result.Failure(CAPAErrors.NotFound);

        record.SubmitRCA(request.RootCause, request.AnalysisDetails, request.ContributingFactors);
        await _repository.UpdateAsync(record, cancellationToken);

        return Result.Success();
    }
}
