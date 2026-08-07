using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.AiEngine.DTOs;
using BerexQms.Domain.AiEngine.Entities;
using BerexQms.Domain.AiEngine.Enums;
using BerexQms.Domain.AiEngine.Repositories;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.AiEngine.Queries.GetAiContext;

internal sealed class GetAiContextQueryHandler
    : IQueryHandler<GetAiContextQuery, IReadOnlyList<ContextDocumentDto>>
{
    private readonly IAiContextDocumentRepository _repository;

    public GetAiContextQueryHandler(IAiContextDocumentRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<IReadOnlyList<ContextDocumentDto>>> Handle(
        GetAiContextQuery request, CancellationToken cancellationToken)
    {
        IReadOnlyList<AiContextDocument> documents;

        if (!string.IsNullOrWhiteSpace(request.ContextType))
        {
            if (!Enum.TryParse<AiContextType>(request.ContextType, true, out _))
                return AiEngineErrors.InvalidContextType;

            documents = await _repository.GetByContextTypeAsync(
                request.ContextType, cancellationToken);

            // Further filter by module
            documents = documents
                .Where(d => d.SourceModule.Equals(
                    request.SourceModule, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }
        else
        {
            documents = await _repository.GetByModuleAsync(
                request.SourceModule, cancellationToken);
        }

        var dtos = documents.Select(MapToDto).ToList();

        return dtos;
    }

    internal static ContextDocumentDto MapToDto(AiContextDocument doc)
    {
        return new ContextDocumentDto(
            doc.Id,
            doc.SourceModule,
            doc.SourceEntityId,
            doc.ContextType,
            doc.Title,
            doc.Content,
            doc.MetadataJson,
            doc.EmbeddingStatus,
            doc.IndexedAt,
            doc.IndexError,
            doc.ContentVersion,
            doc.CreatedAt,
            doc.ModifiedAt);
    }
}
