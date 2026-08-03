using BerexQms.Application.Abstractions.Messaging;

namespace BerexQms.Application.DocumentControl.Commands.MakeObsolete;

public sealed record MakeObsoleteCommand(Guid DocumentId) : ICommand;
