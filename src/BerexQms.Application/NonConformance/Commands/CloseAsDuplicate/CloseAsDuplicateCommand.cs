using BerexQms.Application.Abstractions.Messaging;

namespace BerexQms.Application.NonConformance.Commands.CloseAsDuplicate;

public sealed record CloseAsDuplicateCommand(
    Guid NonConformanceId,
    string Notes) : ICommand;
