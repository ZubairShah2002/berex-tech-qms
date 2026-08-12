using BerexQms.Domain.Capa.Entities;
using BerexQms.Domain.Capa.Enums;
using BerexQms.Domain.Capa.Events;
using BerexQms.Domain.Capa.ValueObjects;
using BerexQms.Domain.NonConformance.Entities;
using BerexQms.Domain.NonConformance.Enums;
using BerexQms.SharedKernel.ValueObjects;

namespace BerexQms.Application.Tests.CrossModule;

/// <summary>
/// Cross-module integration tests for the NCR to CAPA workflow:
/// NCR -> Root Cause -> CAPA -> Verification -> Closure
/// </summary>
public class NcrToCapaWorkflowTests
{
    private static readonly TenantId TestTenantId = TenantId.From(Guid.NewGuid());
    private static readonly Guid TestPartId = Guid.NewGuid();

    private static NonConformanceRecord CreateNCRReadyForCapa()
    {
        var ncr = NonConformanceRecord.Create(
            Guid.NewGuid(), TestTenantId,
            "NCR-2026-0010", NCSeverity.Major,
            NCSource.Inspection, DetectionPoint.InProcess,
            "Process deviation causing dimensional failures",
            TestPartId, partRevisionId: null,
            lotNumber: "LOT-P100", serialNumber: null,
            supplierId: null, supplierLotNumber: null,
            workOrderNumber: "WO-2026-050", customerId: null,
            sourceInspectionId: null,
            quantityAffected: 200, quantityDefective: 25);

        // Move through investigation
        ncr.AssignInvestigator("investigator-001");
        ncr.SubmitInvestigation("Fishbone", "Temperature excursion in oven 3", "Process parameter drift");

        return ncr;
    }

    [Fact]
    public void InitiateCAPA_WithSourceNonConformanceId_LinksToNCR()
    {
        var ncr = CreateNCRReadyForCapa();
        var ncrId = ncr.Id;

        // Initiate CAPA from the NCR
        var capa = CAPARecord.Initiate(
            Guid.NewGuid(), TestTenantId,
            "CAPA-2026-0010", "Address temperature excursion in oven 3",
            "Root cause analysis required for process parameter drift causing failures",
            CAPAPriority.High,
            new CAPASource(CAPASourceType.NonConformance, ncrId, null, null),
            ownerId: "process-engineer-001",
            sourceNonConformanceId: ncrId,
            targetClosureDate: DateTime.UtcNow.AddMonths(2));

        // Link CAPA back to NCR
        ncr.LinkCapa(capa.Id);

        Assert.Equal(CAPAStatus.Initiated, capa.Status);
        Assert.Equal(ncrId, capa.SourceNonConformanceId);
        Assert.Equal(capa.Id, ncr.CapaId);
    }

    [Fact]
    public void StartAndSubmitRCA_TransitionsCapaThroughStatuses()
    {
        var ncrId = Guid.NewGuid();
        var capa = CAPARecord.Initiate(
            Guid.NewGuid(), TestTenantId,
            "CAPA-2026-0011", "Temperature control improvement",
            "Investigate and correct oven temperature drift",
            CAPAPriority.High,
            new CAPASource(CAPASourceType.NonConformance, ncrId, null, null),
            ownerId: "engineer-001",
            sourceNonConformanceId: ncrId,
            targetClosureDate: DateTime.UtcNow.AddMonths(3));

        // Start RCA
        capa.StartRCA(RCAMethodology.Fishbone, "analyst-001");
        Assert.Equal(CAPAStatus.RCAInProgress, capa.Status);
        Assert.NotNull(capa.RootCauseAnalysis);

        // Submit RCA findings
        capa.SubmitRCA(
            "Thermocouple calibration drift beyond tolerance",
            "Fishbone analysis identified calibration as key factor",
            "Insufficient calibration frequency, no drift trending");

        Assert.Equal(CAPAStatus.ActionPlanning, capa.Status);
        Assert.NotNull(capa.RootCauseAnalysis!.CompletedAt);
    }

    [Fact]
    public void AddCorrectiveAndPreventiveActions_ToCAPA()
    {
        var capa = CAPARecord.Initiate(
            Guid.NewGuid(), TestTenantId,
            "CAPA-2026-0012", "Thermocouple calibration program",
            "Implement revised calibration schedule",
            CAPAPriority.High,
            new CAPASource(CAPASourceType.NonConformance, Guid.NewGuid(), null, null),
            ownerId: "engineer-001",
            sourceNonConformanceId: Guid.NewGuid(),
            targetClosureDate: DateTime.UtcNow.AddMonths(2));

        capa.StartRCA(RCAMethodology.FiveWhy, "analyst-001");
        capa.SubmitRCA("Thermocouple drift", null, null);

        // Add corrective action
        var corrective = capa.AddAction(
            ActionType.Corrective,
            "Replace all thermocouples in oven 3 and recalibrate",
            "maintenance-001",
            DateTime.UtcNow.AddDays(14),
            "Calibration certificates required");

        // Add preventive action
        var preventive = capa.AddAction(
            ActionType.Preventive,
            "Implement monthly thermocouple drift trending program",
            "qa-engineer-001",
            DateTime.UtcNow.AddDays(30),
            "Trending chart template and data collection SOP");

        Assert.Equal(2, capa.Actions.Count);
        Assert.Equal(ActionType.Corrective, corrective.ActionType);
        Assert.Equal(ActionType.Preventive, preventive.ActionType);
        Assert.Equal(CAPAStatus.Implementation, capa.Status);
    }

    [Fact]
    public void CompleteAllActions_TransitionsToPendingVerification()
    {
        var capa = CAPARecord.Initiate(
            Guid.NewGuid(), TestTenantId,
            "CAPA-2026-0013", "Fix oven temperature",
            "Complete corrective actions for oven 3",
            CAPAPriority.Medium,
            new CAPASource(CAPASourceType.NonConformance, Guid.NewGuid(), null, null),
            ownerId: "engineer-001",
            sourceNonConformanceId: Guid.NewGuid(),
            targetClosureDate: DateTime.UtcNow.AddMonths(2));

        capa.StartRCA(RCAMethodology.FiveWhy, "analyst-001");
        capa.SubmitRCA("Root cause found", null, null);

        var action = capa.AddAction(
            ActionType.Corrective, "Replace thermocouples",
            "maintenance-001", DateTime.UtcNow.AddDays(14), null);

        // Complete the action
        capa.CompleteAction(action.Id, "maintenance-001",
            "Thermocouples replaced and calibrated", "Cal cert #TC-2026-001");

        Assert.Equal(CAPAStatus.PendingVerification, capa.Status);
    }

    [Fact]
    public void RecordVerification_Effective_ClosesCAPA()
    {
        // Full end-to-end: NCR -> CAPA -> RCA -> Actions -> Verification -> Closure
        var ncrId = Guid.NewGuid();
        var capa = CAPARecord.Initiate(
            Guid.NewGuid(), TestTenantId,
            "CAPA-2026-0014", "Oven 3 temperature control",
            "End-to-end CAPA for temperature excursion",
            CAPAPriority.High,
            new CAPASource(CAPASourceType.NonConformance, ncrId, null, null),
            ownerId: "engineer-001",
            sourceNonConformanceId: ncrId,
            targetClosureDate: DateTime.UtcNow.AddMonths(3));

        // RCA phase
        capa.StartRCA(RCAMethodology.FiveWhy, "analyst-001");
        capa.SubmitRCA("Thermocouple drift", "Detailed analysis", "Contributing factors");

        // Implementation phase
        var action = capa.AddAction(
            ActionType.Corrective, "Replace all thermocouples",
            "maintenance-001", DateTime.UtcNow.AddDays(14), null);
        capa.CompleteAction(action.Id, "maintenance-001", "Done", "Evidence");

        // Verification phase
        var verification = capa.ScheduleVerification(
            DateTime.UtcNow.AddDays(60),
            "No temperature excursions for 60 days of production");

        capa.RecordVerification(verification.Id, "qa-director-001", true,
            "Zero excursions over 60-day monitoring. 1,250 lots processed.",
            "Control chart data and inspection reports");

        // Assert full closure
        Assert.Equal(CAPAStatus.ClosedEffective, capa.Status);
        Assert.NotNull(capa.ClosedAt);
        Assert.Equal("qa-director-001", capa.ClosedBy);

        var closedEvent = capa.DomainEvents.OfType<CAPAClosedEvent>().Single();
        Assert.Equal("ClosedEffective", closedEvent.ClosureStatus);
    }
}
