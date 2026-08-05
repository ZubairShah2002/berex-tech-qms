using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.Training.DTOs;

namespace BerexQms.Application.Training.Queries.GetQualification;

public sealed record GetQualificationQuery(Guid QualificationId) : IQuery<QualificationDto>;
