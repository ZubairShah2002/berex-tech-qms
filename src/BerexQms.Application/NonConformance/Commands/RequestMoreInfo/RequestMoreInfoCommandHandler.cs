using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Domain.NonConformance.Repositories;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.NonConformance.Commands.RequestMoreInfo;

public sealed class RequestMoreInfoCommandHandler : ICommandHandler<RequestMoreInfoCommand>
{
    private readonly INonConformanceRepository _repository;

    public RequestMoreInfoCommandHandler(INonConformanceRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result> Handle(RequestMoreInfoCommand request, CancellationToken cancellationToken)
    {
        var record = await _repository.GetWithInvestigationsAsync(request.NonConformanceId, cancellationToken);
        if (record is null)
            return Result.Failure(NonConformanceErrors.NotFound);

        record.RequestMoreInfo(request.Reason);
        await _repository.UpdateAsync(record, cancellationToken);

        return Result.Success();
    }
}
