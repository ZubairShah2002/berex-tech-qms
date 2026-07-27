using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Domain.NonConformance.Repositories;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.NonConformance.Commands.SubmitInvestigation;

public sealed class SubmitInvestigationCommandHandler : ICommandHandler<SubmitInvestigationCommand>
{
    private readonly INonConformanceRepository _repository;

    public SubmitInvestigationCommandHandler(INonConformanceRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result> Handle(SubmitInvestigationCommand request, CancellationToken cancellationToken)
    {
        var record = await _repository.GetWithInvestigationsAsync(request.NonConformanceId, cancellationToken);
        if (record is null)
            return Result.Failure(NonConformanceErrors.NotFound);

        record.SubmitInvestigation(request.Methodology, request.RootCause, request.Findings);
        await _repository.UpdateAsync(record, cancellationToken);

        return Result.Success();
    }
}
