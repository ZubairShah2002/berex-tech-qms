using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.Training.DTOs;

namespace BerexQms.Application.Training.Queries.GetAssignment;

public sealed record GetAssignmentQuery(Guid AssignmentId) : IQuery<TrainingAssignmentDto>;
