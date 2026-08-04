using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.SupplierQuality.DTOs;

namespace BerexQms.Application.SupplierQuality.Commands.IssueScar;

public sealed record IssueScarCommand(
    Guid SupplierId,
    string ScarNumber,
    Guid? NonConformanceId,
    string DefectDescription,
    string Severity,
    int ResponseDays = 14) : ICommand<SCARRecordDto>;
