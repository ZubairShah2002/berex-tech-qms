using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.Capa.DTOs;

namespace BerexQms.Application.Capa.Queries.GetCapaById;

public sealed record GetCapaByIdQuery(Guid CapaId) : IQuery<CAPADetailDto>;
