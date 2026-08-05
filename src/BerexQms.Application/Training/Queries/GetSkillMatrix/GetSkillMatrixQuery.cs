using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.Training.DTOs;

namespace BerexQms.Application.Training.Queries.GetSkillMatrix;

public sealed record GetSkillMatrixQuery(
    string? Department,
    string? ProductFamily) : IQuery<IReadOnlyList<SkillMatrixEntryDto>>;
