using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.DocumentControl.DTOs;
using BerexQms.Application.Interfaces;
using BerexQms.Domain.DocumentControl.Repositories;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.DocumentControl.Commands.CreateVersion;

internal sealed class CreateVersionCommandHandler : ICommandHandler<CreateVersionCommand, DocumentVersionDto>
{
    private readonly IDocumentRepository _repository;
    private readonly ICurrentUserService _currentUserService;

    public CreateVersionCommandHandler(
        IDocumentRepository repository,
        ICurrentUserService currentUserService)
    {
        _repository = repository;
        _currentUserService = currentUserService;
    }

    public async Task<Result<DocumentVersionDto>> Handle(CreateVersionCommand request, CancellationToken cancellationToken)
    {
        var document = await _repository.GetWithVersionsAsync(request.DocumentId, cancellationToken);
        if (document is null)
            return DocumentErrors.NotFound;

        var version = document.CreateVersion(
            request.VersionNumber,
            request.Content,
            _currentUserService.UserId.ToString(),
            request.ChangeDescription);

        return new DocumentVersionDto(
            version.Id,
            version.VersionNumber,
            version.Status.ToString(),
            version.Content,
            version.ChangeDescription,
            version.AuthorId,
            version.EffectiveDate,
            null,
            version.CreatedAt,
            version.ReleasedAt,
            version.ReleasedBy);
    }
}
