using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.Spc.DTOs;

namespace BerexQms.Application.Spc.Queries.GetChartsByPart;

public sealed record GetChartsByPartQuery(Guid PartId) : IQuery<IReadOnlyList<ControlChartDto>>;
