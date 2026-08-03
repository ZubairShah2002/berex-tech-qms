using BerexQms.Application.Abstractions;
using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.DocumentControl.DTOs;

namespace BerexQms.Application.DocumentControl.Queries.ListDocuments;

public sealed record ListDocumentsQuery(
    string? SearchTerm,
    string? DocumentType,
    string? Status,
    bool? IsActive,
    int Page,
    int PageSize) : IQuery<PagedResult<DocumentDto>>;
