using BerexQms.Domain.NonConformance.Entities;
using BerexQms.Domain.NonConformance.Enums;
using BerexQms.Domain.SupplierQuality.Entities;
using BerexQms.Domain.SupplierQuality.Enums;
using BerexQms.Domain.SupplierQuality.Events;
using BerexQms.SharedKernel.Exceptions;
using BerexQms.SharedKernel.ValueObjects;

namespace BerexQms.Application.Tests.CrossModule;

/// <summary>
/// Cross-module integration tests for the Supplier Issue workflow:
/// Supplier -> Defect -> NCR/SCAR -> CAPA -> Supplier Performance
/// </summary>
public class SupplierIssueWorkflowTests
{
    private static readonly TenantId TestTenantId = TenantId.From(Guid.NewGuid());

    private static Supplier CreateApprovedSupplier()
    {
        var supplier = Supplier.Create(
            Guid.NewGuid(), TestTenantId,
            "SUP-001", "Precision Machining Corp",
            tier: "Tier 1",
            contactName: "John Smith",
            contactRole: "Quality Manager",
            contactEmail: "john.smith@precisionmach.com",
            contactPhone: "+1-555-0100");
        supplier.Approve(DateTime.UtcNow.AddYears(-2));
        return supplier;
    }

    [Fact]
    public void CreateSupplier_WithValidData_StartsAsProspective()
    {
        var supplier = Supplier.Create(
            Guid.NewGuid(), TestTenantId,
            "SUP-002", "Advanced Materials Inc",
            tier: "Tier 2",
            contactName: "Jane Doe",
            contactRole: "Sales Rep",
            contactEmail: "jane@advmaterials.com",
            contactPhone: null);

        Assert.Equal("SUP-002", supplier.Code);
        Assert.Equal("Advanced Materials Inc", supplier.Name);
        Assert.Equal(SupplierStatus.Prospective.ToString(), supplier.Status);
        Assert.NotNull(supplier.PrimaryContact);
    }

    [Fact]
    public void IssueSCAR_WithNonConformanceId_LinksSCARToNCR()
    {
        var supplier = CreateApprovedSupplier();
        var ncrId = Guid.NewGuid();

        var scar = supplier.IssueScar(
            "SCAR-2026-0001", ncrId,
            "Incoming material failed hardness specification",
            "Critical", responseDays: 10);

        Assert.Single(supplier.Scars);
        Assert.Equal(ncrId, scar.NonConformanceId);
        Assert.Equal(ScarStatus.AwaitingResponse.ToString(), scar.Status);
        Assert.Equal("SCAR-2026-0001", scar.ScarNumber);
    }

    [Fact]
    public void SupplierRespondsToSCAR_TransitionsToUnderReview()
    {
        var supplier = CreateApprovedSupplier();
        var scar = supplier.IssueScar(
            "SCAR-2026-0002", Guid.NewGuid(),
            "Material hardness out of spec", "Major");

        supplier.RespondToScar(scar.Id,
            "Root cause: Heat treatment furnace temperature was 15 degrees above setpoint due to faulty thermocouple",
            "1. Replace thermocouple. 2. Add redundant temperature monitoring. 3. Re-inspect all material from affected lot.",
            "Cal cert #TC-001, Inspection report #IR-2026-050");

        var updatedScar = supplier.Scars.First(s => s.Id == scar.Id);
        Assert.Equal(ScarStatus.UnderReview.ToString(), updatedScar.Status);
        Assert.NotNull(updatedScar.Response);
    }

    [Fact]
    public void ReviewSCARResponse_Accept_TransitionsToAccepted()
    {
        var supplier = CreateApprovedSupplier();
        var scar = supplier.IssueScar(
            "SCAR-2026-0003", Guid.NewGuid(),
            "Plating thickness below minimum", "Major");

        supplier.RespondToScar(scar.Id,
            "Root cause: Bath concentration depleted",
            "Corrective: Replenish bath chemistry. Preventive: Daily bath analysis.",
            null);

        // Accept the response
        supplier.AcceptScarResponse(scar.Id);

        var updatedScar = supplier.Scars.First(s => s.Id == scar.Id);
        Assert.Equal(ScarStatus.Accepted.ToString(), updatedScar.Status);
    }

    [Fact]
    public void ReviewSCARResponse_Reject_TransitionsToRejected()
    {
        var supplier = CreateApprovedSupplier();
        var scar = supplier.IssueScar(
            "SCAR-2026-0004", Guid.NewGuid(),
            "Surface finish out of spec", "Minor");

        supplier.RespondToScar(scar.Id,
            "Root cause: Operator error",
            "Corrective: Retrain operator",
            null);

        supplier.RejectScarResponse(scar.Id);

        var updatedScar = supplier.Scars.First(s => s.Id == scar.Id);
        Assert.Equal(ScarStatus.Rejected.ToString(), updatedScar.Status);
    }

    [Fact]
    public void VerifySCARClosure_FromAccepted_ClosesSuccessfully()
    {
        var supplier = CreateApprovedSupplier();
        var scar = supplier.IssueScar(
            "SCAR-2026-0005", Guid.NewGuid(),
            "Contamination found in raw material", "Critical");

        supplier.RespondToScar(scar.Id,
            "Root cause: Cross-contamination in storage",
            "Corrective: Segregated storage. Preventive: Incoming inspection added.",
            "Lab analysis report #LA-2026-100");

        supplier.AcceptScarResponse(scar.Id);
        supplier.CloseScar(scar.Id);

        var updatedScar = supplier.Scars.First(s => s.Id == scar.Id);
        Assert.Equal(ScarStatus.Closed.ToString(), updatedScar.Status);
    }

    [Fact]
    public void CreateScorecard_ForSupplier_CalculatesOverallScore()
    {
        var supplier = CreateApprovedSupplier();

        var scorecard = supplier.CreateScorecard(
            periodStart: new DateTime(2026, 1, 1),
            periodEnd: new DateTime(2026, 6, 30),
            qualityScore: 85.0m,
            deliveryScore: 90.0m,
            responsivenessScore: 80.0m,
            costScore: 75.0m);

        Assert.Single(supplier.Scorecards);

        // Expected: 85*0.40 + 90*0.25 + 80*0.20 + 75*0.15
        //         = 34.0 + 22.5 + 16.0 + 11.25 = 83.75
        Assert.Equal(83.75m, scorecard.OverallScore);
        Assert.Equal(ScorecardStatus.Draft.ToString(), scorecard.Status);

        // Verify domain event
        Assert.Single(supplier.DomainEvents.OfType<SupplierScoreUpdatedEvent>());
    }

    [Fact]
    public void FullSCARWorkflow_IssueThroughClosure()
    {
        // Full workflow: Issue -> Response -> Review -> Follow-up -> Closure
        var supplier = CreateApprovedSupplier();
        var ncrId = Guid.NewGuid();

        // 1. Issue SCAR
        var scar = supplier.IssueScar(
            "SCAR-2026-0006", ncrId,
            "Thread damage on incoming fasteners", "Major");
        Assert.Equal(ScarStatus.AwaitingResponse.ToString(),
            supplier.Scars.First(s => s.Id == scar.Id).Status);

        // 2. Supplier responds
        supplier.RespondToScar(scar.Id,
            "Root cause: Packaging damage during transit",
            "Corrective: Improved packaging. Preventive: Transit testing.",
            "Packaging spec revision #PS-R2");

        // 3. Accept response
        supplier.AcceptScarResponse(scar.Id);

        // 4. Require follow-up verification
        supplier.RequireFollowUpOnScar(scar.Id);
        Assert.Equal(ScarStatus.FollowUp.ToString(),
            supplier.Scars.First(s => s.Id == scar.Id).Status);

        // 5. Close after verification
        supplier.CloseScar(scar.Id);
        Assert.Equal(ScarStatus.Closed.ToString(),
            supplier.Scars.First(s => s.Id == scar.Id).Status);
    }
}
