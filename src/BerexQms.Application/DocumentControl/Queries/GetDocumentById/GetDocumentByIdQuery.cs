using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.DocumentControl.DTOs;

namespace BerexQms.Application.DocumentControl.Queries.GetDocumentById;

public sealed record GetDocumentByIdQuery(Guid DocumentId) : IQuery<DocumentDetailDto>;
