using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.Interfaces;
using BerexQms.Domain.AiEngine.Entities;
using BerexQms.Domain.AiEngine.Enums;
using BerexQms.Domain.AiEngine.Repositories;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.AiEngine.Commands.CreateContextDocument;

internal sealed class CreateContextDocumentCommandHandler
    : ICommandHandler<CreateContextDocumentCommand, Guid>
{
    private readonly IAiContextDocumentRepository _repository;
    private readonly ITenantContext _tenantContext;

    public CreateContextDocumentCommandHandler(
        IAiContextDocumentRepository repository,
        ITenantContext tenantContext)
    {
        _repository = repository;
        _tenantContext = tenantContext;
    }

    public async Task<Result<Guid>> Handle(
        CreateContextDocumentCommand request, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<AiContextType>(request.ContextType, true, out var contextType))
            return AiEngineErrors.InvalidContextType;

        // Prevent duplicate context documents for the same source entity
        if (!string.IsNullOrWhiteSpace(request.SourceEntityId))
        {
            var existing = await _repository.GetBySourceEntityAsync(
                request.SourceModule, request.SourceEntityId, cancellationToken);

            if (existing is not null)
                return AiEngineErrors.ContextDocumentAlreadyExists;
        }

        var document = AiContextDocument.Create(
            Guid.NewGuid(),
            _tenantContext.CurrentTenantId,
            request.SourceModule,
            request.SourceEntityId,
            contextType,
            request.Title,
            request.Content,
            request.MetadataJson);

        await _repository.AddAsync(document, cancellationToken);

        return document.Id;
    }
}
