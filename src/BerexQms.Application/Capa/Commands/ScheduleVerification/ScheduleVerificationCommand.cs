using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.Capa.DTOs;

namespace BerexQms.Application.Capa.Commands.ScheduleVerification;

public sealed record ScheduleVerificationCommand(
    Guid CapaId,
    DateTime ScheduledDate,
    string VerificationCriteria) : ICommand<EffectivenessVerificationDto>;
