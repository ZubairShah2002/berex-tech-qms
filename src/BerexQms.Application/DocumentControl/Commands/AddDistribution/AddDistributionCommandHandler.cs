using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.DocumentControl.DTOs;
using BerexQms.Domain.DocumentControl.Repositories;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.DocumentControl.Commands.AddDistribution;

internal sealed class AddDistributionCommandHandler : ICommandHandler<AddDistributionCommand, DistributionDto>
{
    private readonly IDocumentRepository _repository;

    public AddDistributionCommandHandler(IDocumentRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<DistributionDto>> Handle(AddDistributionCommand request, CancellationToken cancellationToken)
    {
        var document = await _repository.GetWithVersionsAsync(request.DocumentId, cancellationToken);
        if (document is null)
            return DocumentErrors.NotFound;

        var distribution = document.AddDistribution(
            request.VersionId, request.RecipientId, request.ComplianceDeadline);

        await _repository.AddDistributionAsync(distribution, cancellationToken);

        return new DistributionDto(
            distribution.Id,
            distribution.RecipientId,
            distribution.DistributedAt,
            distribution.AcknowledgedAt,
            distribution.ComplianceDeadline,
            distribution.IsOverdue);
    }
}
