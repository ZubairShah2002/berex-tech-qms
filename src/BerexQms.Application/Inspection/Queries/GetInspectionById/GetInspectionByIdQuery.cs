using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.Inspection.DTOs;

namespace BerexQms.Application.Inspection.Queries.GetInspectionById;

public sealed record GetInspectionByIdQuery(Guid InspectionId) : IQuery<InspectionDetailDto>;
