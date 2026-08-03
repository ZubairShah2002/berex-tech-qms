using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.Interfaces;
using BerexQms.Domain.DocumentControl.Entities;
using BerexQms.Domain.DocumentControl.Enums;
using BerexQms.Domain.DocumentControl.Repositories;
using BerexQms.SharedKernel.Results;
using BerexQms.SharedKernel.ValueObjects;

namespace BerexQms.Application.DocumentControl.Commands.CreateDocument;

internal sealed class CreateDocumentCommandHandler : ICommandHandler<CreateDocumentCommand, Guid>
{
    private readonly IDocumentRepository _repository;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUserService _currentUserService;

    public CreateDocumentCommandHandler(
        IDocumentRepository repository,
        ITenantContext tenantContext,
        ICurrentUserService currentUserService)
    {
        _repository = repository;
        _tenantContext = tenantContext;
        _currentUserService = currentUserService;
    }

    public async Task<Result<Guid>> Handle(CreateDocumentCommand request, CancellationToken cancellationToken)
    {
        if (await _repository.DocumentNumberExistsAsync(request.DocumentNumber, cancellationToken))
            return DocumentErrors.DocumentNumberExists;

        if (!Enum.TryParse<DocumentType>(request.DocumentType, ignoreCase: true, out var docType))
            return Error.Validation("Document.InvalidType", $"Invalid document type: {request.DocumentType}.");

        var document = DocumentMaster.Create(
            Guid.NewGuid(),
            _tenantContext.CurrentTenantId,
            request.DocumentNumber,
            request.Title,
            docType,
            _currentUserService.UserId.ToString(),
            request.Description,
            request.Department);

        await _repository.AddAsync(document, cancellationToken);
        return document.Id;
    }
}
