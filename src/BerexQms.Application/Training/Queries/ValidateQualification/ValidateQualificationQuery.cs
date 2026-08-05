using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.Training.DTOs;

namespace BerexQms.Application.Training.Queries.ValidateQualification;

public sealed record ValidateQualificationQuery(
    Guid EmployeeId,
    Guid QualificationId) : IQuery<QualificationValidationDto>;
