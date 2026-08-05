using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.Training.DTOs;

namespace BerexQms.Application.Training.Queries.GetEmployeeCompetencies;

public sealed record GetEmployeeCompetenciesQuery(Guid EmployeeId) : IQuery<IReadOnlyList<CompetencyRecordDto>>;
