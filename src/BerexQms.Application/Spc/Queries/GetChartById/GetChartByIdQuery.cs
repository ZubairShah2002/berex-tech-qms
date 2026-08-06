using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.Spc.DTOs;

namespace BerexQms.Application.Spc.Queries.GetChartById;

public sealed record GetChartByIdQuery(Guid Id) : IQuery<ControlChartDetailDto>;
