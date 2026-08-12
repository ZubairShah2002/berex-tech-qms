using BerexQms.Domain.Inspection.Entities;
using BerexQms.Domain.Inspection.Enums;
using BerexQms.Domain.NonConformance.Entities;
using BerexQms.Domain.NonConformance.Enums;
using BerexQms.SharedKernel.ValueObjects;

namespace BerexQms.Application.Tests.CrossModule;

/// <summary>
/// Cross-module integration tests for the Incoming Rejection workflow:
/// Inspection -> Defect -> Reject -> NCR
/// </summary>
public class IncomingRejectionWorkflowTests
{
    private static readonly TenantId TestTenantId = TenantId.From(Guid.NewGuid());
    private static readonly Guid TestPartId = Guid.NewGuid();

    private static InspectionRecord CreateAndStartInspection()
    {
        var inspection = InspectionRecord.Create(
            Guid.NewGuid(), TestTenantId,
            "INS-2026-0001", InspectionType.IQC,
            TestPartId, partRevisionId: null,
            lotNumber: "LOT-A100", lotSize: 500,
            sampleSize: 20, supplierId: Guid.NewGuid(),
            samplingPlanId: null, inspectorId: "inspector-001");
        inspection.StartInspection();
        return inspection;
    }

    [Fact]
    public void Inspection_Create_Start_RecordFail_Complete_StatusIsFailed()
    {
        // Arrange: create and start inspection
        var inspection = CreateAndStartInspection();

        // Act: record a failing measurement and complete
        inspection.AddMeasurement(
            null, "Outer Diameter", 12.7m, null, "mm",
            MeasurementResult.Fail, null, "operator-001");
        inspection.Complete("qa-inspector-001");

        // Assert
        Assert.Equal(InspectionStatus.PendingApproval, inspection.Status);
        Assert.Equal(InspectionResult.Fail, inspection.Result);
        Assert.NotNull(inspection.CompletedAt);
    }

    [Fact]
    public void NCR_CreatedWithSourceInspectionId_LinksToFailedInspection()
    {
        // Arrange: complete a failed inspection
        var inspection = CreateAndStartInspection();
        inspection.AddMeasurement(
            null, "Diameter", 12.7m, null, "mm",
            MeasurementResult.Fail, null, null);
        inspection.Complete("qa-inspector-001");
        var inspectionId = inspection.Id;

        // Act: create NCR linked to the failed inspection
        var ncr = NonConformanceRecord.Create(
            Guid.NewGuid(), TestTenantId,
            "NCR-2026-0001", NCSeverity.Critical,
            NCSource.Inspection, DetectionPoint.IncomingInspection,
            "OD out of tolerance per inspection INS-2026-0001",
            TestPartId, partRevisionId: null,
            lotNumber: "LOT-A100", serialNumber: null,
            supplierId: null, supplierLotNumber: null,
            workOrderNumber: null, customerId: null,
            sourceInspectionId: inspectionId,
            quantityAffected: 500, quantityDefective: 20);

        // Assert
        Assert.Equal(NCStatus.Open, ncr.Status);
        Assert.Equal(inspectionId, ncr.SourceInspectionId);
        Assert.Equal(NCSeverity.Critical, ncr.Severity);
    }

    [Fact]
    public void CriticalNCR_RequiresContainment_BeforeInvestigation()
    {
        // Arrange: create a Critical severity NCR (from failed inspection)
        var inspectionId = Guid.NewGuid();
        var ncr = NonConformanceRecord.Create(
            Guid.NewGuid(), TestTenantId,
            "NCR-2026-0002", NCSeverity.Critical,
            NCSource.Inspection, DetectionPoint.IncomingInspection,
            "Critical dimensional failure",
            TestPartId, partRevisionId: null,
            lotNumber: "LOT-B200", serialNumber: null,
            supplierId: null, supplierLotNumber: null,
            workOrderNumber: null, customerId: null,
            sourceInspectionId: inspectionId,
            quantityAffected: 100, quantityDefective: 15);

        // Act/Assert: cannot assign investigator without verified containment
        Assert.Throws<SharedKernel.Exceptions.DomainException>(() =>
            ncr.AssignInvestigator("investigator-001"));

        // Add containment but do NOT verify
        ncr.AddContainmentAction("Quarantine affected lot", "operator-001");
        Assert.Throws<SharedKernel.Exceptions.DomainException>(() =>
            ncr.AssignInvestigator("investigator-001"));
    }

    [Fact]
    public void ContainmentAction_CanBeVerified()
    {
        var ncr = NonConformanceRecord.Create(
            Guid.NewGuid(), TestTenantId,
            "NCR-2026-0003", NCSeverity.Critical,
            NCSource.Inspection, DetectionPoint.IncomingInspection,
            "Dimensional out of tolerance",
            TestPartId, partRevisionId: null,
            lotNumber: "LOT-C300", serialNumber: null,
            supplierId: null, supplierLotNumber: null,
            workOrderNumber: null, customerId: null,
            sourceInspectionId: Guid.NewGuid(),
            quantityAffected: 50, quantityDefective: 8);

        var action = ncr.AddContainmentAction(
            "Quarantine all affected parts in cage B12", "operator-001");

        // Verify the containment
        ncr.VerifyContainment(action.Id, "qa-supervisor-001");

        Assert.True(ncr.HasVerifiedContainment());
        var verified = ncr.ContainmentActions.First();
        Assert.True(verified.IsVerified);
        Assert.Equal("qa-supervisor-001", verified.VerifiedBy);
        Assert.NotNull(verified.VerifiedAt);
    }

    [Fact]
    public void Investigation_CanBeSubmitted_AfterContainment()
    {
        // Arrange: create Critical NCR with verified containment
        var ncr = NonConformanceRecord.Create(
            Guid.NewGuid(), TestTenantId,
            "NCR-2026-0004", NCSeverity.Critical,
            NCSource.Inspection, DetectionPoint.IncomingInspection,
            "Critical incoming quality failure",
            TestPartId, partRevisionId: null,
            lotNumber: "LOT-D400", serialNumber: null,
            supplierId: null, supplierLotNumber: null,
            workOrderNumber: null, customerId: null,
            sourceInspectionId: Guid.NewGuid(),
            quantityAffected: 200, quantityDefective: 30);

        var containmentAction = ncr.AddContainmentAction(
            "Quarantine all incoming material from this lot", "receiving-001");
        ncr.VerifyContainment(containmentAction.Id, "qa-manager-001");

        // Act: assign investigator (allowed now because containment is verified)
        ncr.AssignInvestigator("investigator-001");
        Assert.Equal(NCStatus.UnderInvestigation, ncr.Status);

        // Submit investigation
        ncr.SubmitInvestigation(
            "Five Why Analysis",
            "Supplier tooling worn beyond maintenance interval",
            "Root cause: Preventive maintenance not performed per schedule");

        // Assert
        Assert.Equal(NCStatus.PendingDisposition, ncr.Status);
        Assert.Single(ncr.Investigations);
    }
}
