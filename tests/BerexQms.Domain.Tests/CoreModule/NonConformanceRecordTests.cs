using BerexQms.Domain.NonConformance.Entities;
using BerexQms.Domain.NonConformance.Enums;
using BerexQms.Domain.NonConformance.Events;
using BerexQms.SharedKernel.Exceptions;
using BerexQms.SharedKernel.ValueObjects;

namespace BerexQms.Domain.Tests.CoreModule;

public class NonConformanceRecordTests
{
    private static readonly TenantId TestTenantId = TenantId.From(Guid.NewGuid());

    private static NonConformanceRecord CreateNCR(
        NCSeverity severity = NCSeverity.Minor,
        Guid? sourceInspectionId = null)
    {
        return NonConformanceRecord.Create(
            Guid.NewGuid(),
            TestTenantId,
            "NCR-2026-0001",
            severity,
            NCSource.Inspection,
            DetectionPoint.IncomingInspection,
            "Dimensional out of tolerance",
            partId: Guid.NewGuid(),
            partRevisionId: null,
            lotNumber: "LOT-001",
            serialNumber: null,
            supplierId: null,
            supplierLotNumber: null,
            workOrderNumber: null,
            customerId: null,
            sourceInspectionId: sourceInspectionId,
            quantityAffected: 100,
            quantityDefective: 5);
    }

    [Fact]
    public void Create_WithValidData_RaisesNonConformanceRaisedEvent()
    {
        var ncr = CreateNCR();

        Assert.Single(ncr.DomainEvents);
        Assert.IsType<NonConformanceRaisedEvent>(ncr.DomainEvents.First());
    }

    [Fact]
    public void Create_StartsInOpenStatus()
    {
        var ncr = CreateNCR();

        Assert.Equal(NCStatus.Open, ncr.Status);
        Assert.Equal("NCR-2026-0001", ncr.NcrNumber);
    }

    [Fact]
    public void AddContainmentAction_OnOpenNCR_Succeeds()
    {
        var ncr = CreateNCR();

        var action = ncr.AddContainmentAction("Quarantine affected lot", "operator-001");

        Assert.Single(ncr.ContainmentActions);
        Assert.Equal("Quarantine affected lot", action.Description);
        Assert.False(action.IsVerified);
    }

    [Fact]
    public void VerifyContainment_SetsVerifiedFlag()
    {
        var ncr = CreateNCR();
        var action = ncr.AddContainmentAction("Quarantine lot", "operator-001");

        ncr.VerifyContainment(action.Id, "qa-manager-001");

        Assert.True(ncr.HasVerifiedContainment());
        var verifiedAction = ncr.ContainmentActions.First();
        Assert.True(verifiedAction.IsVerified);
        Assert.Equal("qa-manager-001", verifiedAction.VerifiedBy);
    }

    [Fact]
    public void SubmitInvestigation_TransitionsToUnderInvestigation_ThenPendingDisposition()
    {
        var ncr = CreateNCR();
        ncr.AddContainmentAction("Quarantine", "operator-001");

        // Assign investigator transitions to UnderInvestigation
        ncr.AssignInvestigator("investigator-001");
        Assert.Equal(NCStatus.UnderInvestigation, ncr.Status);

        // Submit investigation transitions to PendingDisposition
        ncr.SubmitInvestigation("5 Why Analysis", "Worn tooling", "Tool wear detected");
        Assert.Equal(NCStatus.PendingDisposition, ncr.Status);
    }

    [Fact]
    public void CriticalNCR_RequiresVerifiedContainment_BeforeInvestigation()
    {
        var ncr = CreateNCR(NCSeverity.Critical);

        // Add containment but do NOT verify it
        ncr.AddContainmentAction("Quarantine lot", "operator-001");

        Assert.Throws<DomainException>(() =>
            ncr.AssignInvestigator("investigator-001"));
    }

    [Fact]
    public void CriticalNCR_WithVerifiedContainment_AllowsInvestigation()
    {
        var ncr = CreateNCR(NCSeverity.Critical);
        var action = ncr.AddContainmentAction("Quarantine lot", "operator-001");
        ncr.VerifyContainment(action.Id, "qa-manager-001");

        ncr.AssignInvestigator("investigator-001");

        Assert.Equal(NCStatus.UnderInvestigation, ncr.Status);
    }

    [Fact]
    public void RecordDisposition_ClosesNCR()
    {
        var ncr = CreateNCR(NCSeverity.Minor);
        ncr.AssignInvestigator("investigator-001");
        ncr.SubmitInvestigation("5 Why", "Root cause found", "Detailed findings");

        ncr.RecordDisposition(NCDispositionType.Rework, "Rework to spec", "approver-001");

        Assert.Equal(NCStatus.Closed, ncr.Status);
        Assert.NotNull(ncr.ClosedAt);
        Assert.NotNull(ncr.Disposition);
    }

    [Fact]
    public void MajorNCR_RequiresCapaLink_BeforeClosure()
    {
        var ncr = CreateNCR(NCSeverity.Major);
        ncr.AssignInvestigator("investigator-001");
        ncr.SubmitInvestigation("5 Why", "Root cause", "Findings");

        // Major NCR without CAPA link should fail
        Assert.Throws<DomainException>(() =>
            ncr.RecordDisposition(NCDispositionType.Scrap, "Scrap defective parts", "approver-001"));
    }

    [Fact]
    public void MajorNCR_WithCapaLink_AllowsClosure()
    {
        var ncr = CreateNCR(NCSeverity.Major);
        ncr.AssignInvestigator("investigator-001");
        ncr.SubmitInvestigation("Fishbone", "Process deviation", "Temperature out of range");
        ncr.LinkCapa(Guid.NewGuid());

        ncr.RecordDisposition(NCDispositionType.Rework, "Rework to specification", "approver-001");

        Assert.Equal(NCStatus.Closed, ncr.Status);
    }

    [Fact]
    public void Reopen_ClosedNCR_TransitionsToReopened()
    {
        var ncr = CreateNCR(NCSeverity.Minor);
        ncr.AssignInvestigator("investigator-001");
        ncr.SubmitInvestigation("5 Why", "Root cause", "Findings");
        ncr.RecordDisposition(NCDispositionType.UseAsIs, "Acceptable deviation", "approver-001");

        ncr.Reopen("qa-manager-001", "Additional defects found");

        Assert.Equal(NCStatus.Reopened, ncr.Status);
        Assert.NotNull(ncr.ReopenedAt);
        Assert.Equal("Additional defects found", ncr.ReopenReason);
        Assert.Null(ncr.Disposition); // Disposition is cleared
    }

    [Fact]
    public void CloseAsDuplicate_OnOpenNCR_ClosesWithNotes()
    {
        var ncr = CreateNCR();

        ncr.CloseAsDuplicate("qa-manager-001", "Duplicate of NCR-2026-0002");

        Assert.Equal(NCStatus.Closed, ncr.Status);
        Assert.Equal("Duplicate of NCR-2026-0002", ncr.ClosureNotes);
        Assert.Single(ncr.DomainEvents.OfType<NonConformanceClosedEvent>());
    }

    [Fact]
    public void CloseAsDuplicate_OnNonOpenNCR_ThrowsDomainException()
    {
        var ncr = CreateNCR();
        ncr.AssignInvestigator("investigator-001");

        Assert.Throws<DomainException>(() =>
            ncr.CloseAsDuplicate("qa-manager-001", "Duplicate"));
    }
}
