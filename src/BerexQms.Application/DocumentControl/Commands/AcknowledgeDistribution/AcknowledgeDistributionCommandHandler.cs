using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Domain.DocumentControl.Repositories;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.DocumentControl.Commands.AcknowledgeDistribution;

internal sealed class AcknowledgeDistributionCommandHandler : ICommandHandler<AcknowledgeDistributionCommand>
{
    private readonly IDocumentRepository _repository;

    public AcknowledgeDistributionCommandHandler(IDocumentRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result> Handle(AcknowledgeDistributionCommand request, CancellationToken cancellationToken)
    {
        var distribution = await _repository.GetDistributionAsync(request.DistributionId, cancellationToken);
        if (distribution is null)
            return Result.Failure(DocumentErrors.DistributionNotFound);

        distribution.Acknowledge();
        return Result.Success();
    }
}
