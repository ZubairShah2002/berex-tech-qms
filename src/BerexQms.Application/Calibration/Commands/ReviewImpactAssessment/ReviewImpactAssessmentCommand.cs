using BerexQms.Application.Abstractions.Messaging;

namespace BerexQms.Application.Calibration.Commands.ReviewImpactAssessment;

public sealed record ReviewImpactAssessmentCommand(
    Guid AssessmentId,
    string Action,
    string? Notes) : ICommand;
