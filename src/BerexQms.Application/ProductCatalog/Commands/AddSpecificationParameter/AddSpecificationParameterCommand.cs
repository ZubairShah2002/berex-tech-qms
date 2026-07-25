using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.ProductCatalog.DTOs;

namespace BerexQms.Application.ProductCatalog.Commands.AddSpecificationParameter;

public sealed record AddSpecificationParameterCommand(
    Guid PartId,
    Guid RevisionId,
    string Name,
    string Type,
    string? Unit,
    decimal? NominalValue,
    decimal? UpperTolerance,
    decimal? LowerTolerance,
    string? TextValue,
    bool IsCritical) : ICommand<SpecificationParameterDto>;
