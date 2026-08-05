using BerexQms.Application.Abstractions.Messaging;

namespace BerexQms.Application.Training.Commands.UpdateQualification;

public sealed record UpdateQualificationCommand(
    Guid QualificationId,
    string Name,
    string? Description,
    string? ScopeProductFamily,
    string? ScopeInspectionType,
    string? ScopeProcessArea,
    int ValidityMonths,
    int RenewalWindowDays) : ICommand;
