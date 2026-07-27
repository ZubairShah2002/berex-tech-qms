using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Domain.NonConformance.Repositories;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.NonConformance.Commands.AssignInvestigator;

public sealed class AssignInvestigatorCommandHandler : ICommandHandler<AssignInvestigatorCommand>
{
    private readonly INonConformanceRepository _repository;

    public AssignInvestigatorCommandHandler(INonConformanceRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result> Handle(AssignInvestigatorCommand request, CancellationToken cancellationToken)
    {
        var record = await _repository.GetWithContainmentsAsync(request.NonConformanceId, cancellationToken);
        if (record is null)
            return Result.Failure(NonConformanceErrors.NotFound);

        record.AssignInvestigator(request.InvestigatorId);
        await _repository.UpdateAsync(record, cancellationToken);

        return Result.Success();
    }
}
