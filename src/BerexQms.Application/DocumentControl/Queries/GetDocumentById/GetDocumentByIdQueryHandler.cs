using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.DocumentControl.DTOs;
using BerexQms.Domain.DocumentControl.Repositories;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.DocumentControl.Queries.GetDocumentById;

internal sealed class GetDocumentByIdQueryHandler
    : IQueryHandler<GetDocumentByIdQuery, DocumentDetailDto>
{
    private readonly IDocumentRepository _repository;

    public GetDocumentByIdQueryHandler(IDocumentRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<DocumentDetailDto>> Handle(
        GetDocumentByIdQuery request, CancellationToken cancellationToken)
    {
        var document = await _repository.GetFullDetailAsync(request.DocumentId, cancellationToken);
        if (document is null)
            return DocumentErrors.NotFound;

        var versionDtos = document.Versions
            .OrderByDescending(v => v.CreatedAt)
            .Select(v => new DocumentVersionDto(
                v.Id,
                v.VersionNumber,
                v.Status.ToString(),
                v.Content,
                v.ChangeDescription,
                v.AuthorId,
                v.EffectiveDate,
                v.Attachment is not null
                    ? new DocumentAttachmentDto(
                        v.Attachment.FileName,
                        v.Attachment.ContentType,
                        v.Attachment.SizeBytes,
                        v.Attachment.StoragePath)
                    : null,
                v.CreatedAt,
                v.ReleasedAt,
                v.ReleasedBy))
            .ToList();

        return new DocumentDetailDto(
            document.Id,
            document.DocumentNumber,
            document.Title,
            document.Description,
            document.DocumentType.ToString(),
            document.OwnerId,
            document.Department,
            document.IsActive,
            versionDtos,
            document.CreatedAt,
            document.CreatedBy,
            document.ModifiedAt);
    }
}
