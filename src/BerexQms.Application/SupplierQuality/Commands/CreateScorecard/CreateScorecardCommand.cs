using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.SupplierQuality.DTOs;

namespace BerexQms.Application.SupplierQuality.Commands.CreateScorecard;

public sealed record CreateScorecardCommand(
    Guid SupplierId,
    DateTime PeriodStart,
    DateTime PeriodEnd,
    decimal QualityScore,
    decimal DeliveryScore,
    decimal ResponsivenessScore,
    decimal CostScore) : ICommand<SupplierScorecardDto>;
