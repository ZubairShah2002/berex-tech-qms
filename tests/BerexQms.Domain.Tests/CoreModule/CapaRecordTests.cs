using BerexQms.Domain.Capa.Entities;
using BerexQms.Domain.Capa.Enums;
using BerexQms.Domain.Capa.Events;
using BerexQms.Domain.Capa.ValueObjects;
using BerexQms.SharedKernel.Exceptions;
using BerexQms.SharedKernel.ValueObjects;

namespace BerexQms.Domain.Tests.CoreModule;

public class CapaRecordTests
{
    private static readonly TenantId TestTenantId = TenantId.From(Guid.NewGuid());

    private static CAPARecord CreateCAPA(Guid? sourceNcrId = null)
    {
        return CAPARecord.Initiate(
            Guid.NewGuid(),
            TestTenantId,
            "CAPA-2026-0001",
            "Correct root cause of dimensional failures",
            "Dimensional failures detected during incoming inspection",
            CAPAPriority.High,
            new CAPASource(CAPASourceType.NonConformance, sourceNcrId, null, null),
            ownerId: "owner-001",
            sourceNonConformanceId: sourceNcrId,
            targetClosureDate: DateTime.UtcNow.AddMonths(3));
    }

    private static CAPARecord CreateCAPAWithRCA()
    {
        var capa = CreateCAPA();
        capa.StartRCA(RCAMethodology.FiveWhy, "analyst-001");
        capa.SubmitRCA("Worn tooling not detected during PM", "Five Why analysis", "Insufficient PM schedule");
        return capa;
    }

    private static CAPARecord CreateCAPAWithCompletedActions()
    {
        var capa = CreateCAPAWithRCA();
        var action = capa.AddAction(
            ActionType.Corrective, "Replace worn tooling",
            "maintenance-001", DateTime.UtcNow.AddDays(30), null);
        capa.CompleteAction(action.Id, "maintenance-001", "Tooling replaced", "Photo evidence");
        return capa;
    }

    [Fact]
    public void Initiate_WithValidData_RaisesCAPAInitiatedEvent()
    {
        var capa = CreateCAPA();

        Assert.Single(capa.DomainEvents);
        Assert.IsType<CAPAInitiatedEvent>(capa.DomainEvents.First());
    }

    [Fact]
    public void Initiate_StartsInInitiatedStatus()
    {
        var capa = CreateCAPA();

        Assert.Equal(CAPAStatus.Initiated, capa.Status);
        Assert.Equal("CAPA-2026-0001", capa.CapaNumber);
    }

    [Fact]
    public void StartRCA_FromInitiated_TransitionsToRCAInProgress()
    {
        var capa = CreateCAPA();

        capa.StartRCA(RCAMethodology.Fishbone, "analyst-001");

        Assert.Equal(CAPAStatus.RCAInProgress, capa.Status);
        Assert.NotNull(capa.RootCauseAnalysis);
        Assert.Equal(RCAMethodology.Fishbone, capa.RootCauseAnalysis.Methodology);
    }

    [Fact]
    public void SubmitRCA_FromRCAInProgress_TransitionsToActionPlanning()
    {
        var capa = CreateCAPA();
        capa.StartRCA(RCAMethodology.FiveWhy, "analyst-001");

        capa.SubmitRCA("Worn tooling", "Five Why analysis details", "Contributing factors");

        Assert.Equal(CAPAStatus.ActionPlanning, capa.Status);
        Assert.NotNull(capa.RootCauseAnalysis!.CompletedAt);
        Assert.Equal("Worn tooling", capa.RootCauseAnalysis.RootCause);
    }

    [Fact]
    public void AddCorrectiveAction_FromActionPlanning_TransitionsToImplementation()
    {
        var capa = CreateCAPAWithRCA();

        var action = capa.AddAction(
            ActionType.Corrective,
            "Replace worn tooling and update PM schedule",
            "maintenance-001",
            DateTime.UtcNow.AddDays(30),
            "Before and after photos");

        Assert.Equal(CAPAStatus.Implementation, capa.Status);
        Assert.Single(capa.Actions);
        Assert.Equal(ActionType.Corrective, action.ActionType);
    }

    [Fact]
    public void AddPreventiveAction_FromActionPlanning_Succeeds()
    {
        var capa = CreateCAPAWithRCA();

        var action = capa.AddAction(
            ActionType.Preventive,
            "Add tooling wear check to PM checklist",
            "qa-manager-001",
            DateTime.UtcNow.AddDays(14),
            null);

        Assert.Equal(ActionType.Preventive, action.ActionType);
        Assert.Single(capa.Actions);
    }

    [Fact]
    public void CompleteAction_LastAction_TransitionsToPendingVerification()
    {
        var capa = CreateCAPAWithRCA();
        var action = capa.AddAction(
            ActionType.Corrective, "Fix it", "owner-002",
            DateTime.UtcNow.AddDays(10), null);

        capa.CompleteAction(action.Id, "owner-002", "Done", "Evidence");

        Assert.Equal(CAPAStatus.PendingVerification, capa.Status);
    }

    [Fact]
    public void CompleteAction_NotLastAction_StaysInImplementation()
    {
        var capa = CreateCAPAWithRCA();
        var action1 = capa.AddAction(
            ActionType.Corrective, "Fix it", "owner-002",
            DateTime.UtcNow.AddDays(10), null);
        capa.AddAction(
            ActionType.Preventive, "Prevent it", "owner-003",
            DateTime.UtcNow.AddDays(20), null);

        capa.CompleteAction(action1.Id, "owner-002", "Done", null);

        Assert.Equal(CAPAStatus.Implementation, capa.Status);
    }

    [Fact]
    public void ScheduleVerification_FromPendingVerification_Succeeds()
    {
        var capa = CreateCAPAWithCompletedActions();

        var verification = capa.ScheduleVerification(
            DateTime.UtcNow.AddDays(60),
            "Verify no recurrence of dimensional failures for 30 days");

        Assert.Single(capa.Verifications);
        Assert.Equal("Verify no recurrence of dimensional failures for 30 days",
            verification.VerificationCriteria);
    }

    [Fact]
    public void RecordVerification_Effective_ClosesCapaAsEffective()
    {
        var capa = CreateCAPAWithCompletedActions();
        var verification = capa.ScheduleVerification(
            DateTime.UtcNow.AddDays(60), "Verify no recurrence");

        capa.RecordVerification(verification.Id, "verifier-001", true,
            "No recurrence in 30-day monitoring period", "Inspection data");

        Assert.Equal(CAPAStatus.ClosedEffective, capa.Status);
        Assert.NotNull(capa.ClosedAt);
        Assert.Equal("verifier-001", capa.ClosedBy);
        Assert.Single(capa.DomainEvents.OfType<CAPAClosedEvent>());
    }

    [Fact]
    public void RecordVerification_Ineffective_TransitionsBackToRCAInProgress()
    {
        var capa = CreateCAPAWithCompletedActions();
        var verification = capa.ScheduleVerification(
            DateTime.UtcNow.AddDays(60), "Verify no recurrence");

        capa.RecordVerification(verification.Id, "verifier-001", false,
            "Defects recurred within monitoring period", null);

        Assert.Equal(CAPAStatus.RCAInProgress, capa.Status);
        Assert.Null(capa.RootCauseAnalysis); // RCA is reset
    }

    [Fact]
    public void Initiate_WithSourceNonConformanceId_LinksToNCR()
    {
        var ncrId = Guid.NewGuid();

        var capa = CreateCAPA(ncrId);

        Assert.Equal(ncrId, capa.SourceNonConformanceId);
        var initiatedEvent = capa.DomainEvents.OfType<CAPAInitiatedEvent>().Single();
        Assert.Equal(ncrId, initiatedEvent.SourceNonConformanceId);
    }
}
