using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.Calibration.DTOs;

namespace BerexQms.Application.Calibration.Queries.GetImpactAssessment;

public sealed record GetImpactAssessmentQuery(Guid AssessmentId) : IQuery<ImpactAssessmentDto>;
