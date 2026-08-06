using BerexQms.Application.Abstractions.Messaging;

namespace BerexQms.Application.Spc.Commands.AddDataPoint;

public sealed record AddDataPointCommand(
    Guid ChartId,
    decimal Value,
    string? SubgroupValues,
    int SampleSize,
    DateTime Timestamp,
    Guid? InspectionId) : ICommand<Guid>;
