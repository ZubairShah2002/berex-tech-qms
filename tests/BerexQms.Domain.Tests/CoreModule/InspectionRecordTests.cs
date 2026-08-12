using BerexQms.Domain.Inspection.Entities;
using BerexQms.Domain.Inspection.Enums;
using BerexQms.Domain.Inspection.Events;
using BerexQms.SharedKernel.Exceptions;
using BerexQms.SharedKernel.ValueObjects;

namespace BerexQms.Domain.Tests.CoreModule;

public class InspectionRecordTests
{
    private static readonly TenantId TestTenantId = TenantId.From(Guid.NewGuid());
    private const string TestInspectorId = "inspector-001";

    private static InspectionRecord CreateDraftInspection()
    {
        return InspectionRecord.Create(
            Guid.NewGuid(),
            TestTenantId,
            "INS-2026-0001",
            InspectionType.IQC,
            Guid.NewGuid(),
            partRevisionId: null,
            lotNumber: "LOT-001",
            lotSize: 100,
            sampleSize: 10,
            supplierId: null,
            samplingPlanId: null,
            inspectorId: TestInspectorId);
    }

    private static InspectionRecord CreateInProgressInspection()
    {
        var record = CreateDraftInspection();
        record.StartInspection();
        return record;
    }

    private static InspectionRecord CreateCompletedInspection(bool withFailingMeasurement = false)
    {
        var record = CreateInProgressInspection();
        if (withFailingMeasurement)
        {
            record.AddMeasurement(null, "Diameter", 10.5m, null, "mm", MeasurementResult.Fail, null, null);
        }
        else
        {
            record.AddMeasurement(null, "Diameter", 10.0m, null, "mm", MeasurementResult.Pass, null, null);
        }
        record.Complete("reviewer-001");
        return record;
    }

    [Fact]
    public void Create_WithValidData_CreatesInspectionInDraftStatus()
    {
        var id = Guid.NewGuid();
        var partId = Guid.NewGuid();

        var record = InspectionRecord.Create(
            id, TestTenantId, "INS-2026-0001",
            InspectionType.IQC, partId,
            partRevisionId: null, lotNumber: "LOT-001",
            lotSize: 100, sampleSize: 10,
            supplierId: null, samplingPlanId: null,
            inspectorId: TestInspectorId);

        Assert.Equal(id, record.Id);
        Assert.Equal(TestTenantId, record.TenantId);
        Assert.Equal("INS-2026-0001", record.InspectionNumber);
        Assert.Equal(InspectionType.IQC, record.Type);
        Assert.Equal(InspectionStatus.Draft, record.Status);
        Assert.Equal(partId, record.PartId);
        Assert.Single(record.DomainEvents);
        Assert.IsType<InspectionCreatedEvent>(record.DomainEvents.First());
    }

    [Fact]
    public void StartInspection_FromDraft_TransitionsToInProgress()
    {
        var record = CreateDraftInspection();

        record.StartInspection();

        Assert.Equal(InspectionStatus.InProgress, record.Status);
    }

    [Fact]
    public void StartInspection_AlreadyStarted_ThrowsDomainException()
    {
        var record = CreateInProgressInspection();

        Assert.Throws<DomainException>(() => record.StartInspection());
    }

    [Fact]
    public void AddMeasurement_OnInProgressInspection_RecordsMeasurement()
    {
        var record = CreateInProgressInspection();

        var measurement = record.AddMeasurement(
            null, "Length", 15.2m, null, "mm",
            MeasurementResult.Pass, null, "operator-001");

        Assert.Single(record.Measurements);
        Assert.Equal("Length", measurement.CharacteristicName);
        Assert.Equal(MeasurementResult.Pass, measurement.Result);
    }

    [Fact]
    public void AddMeasurement_OnDraftInspection_ThrowsDomainException()
    {
        var record = CreateDraftInspection();

        Assert.Throws<DomainException>(() =>
            record.AddMeasurement(null, "Length", 15.0m, null, "mm",
                MeasurementResult.Pass, null, null));
    }

    [Fact]
    public void Complete_FromInProgress_WithMeasurements_TransitionsToPendingApproval()
    {
        var record = CreateInProgressInspection();
        record.AddMeasurement(null, "Width", 5.0m, null, "mm", MeasurementResult.Pass, null, null);

        record.Complete("reviewer-001");

        Assert.Equal(InspectionStatus.PendingApproval, record.Status);
        Assert.Equal(InspectionResult.Pass, record.Result);
        Assert.NotNull(record.CompletedAt);
    }

    [Fact]
    public void Complete_WithFailingMeasurement_ResultIsFail()
    {
        var record = CreateInProgressInspection();
        record.AddMeasurement(null, "Diameter", 10.5m, null, "mm", MeasurementResult.Fail, null, null);

        record.Complete("reviewer-001");

        Assert.Equal(InspectionResult.Fail, record.Result);
        Assert.Equal(InspectionStatus.PendingApproval, record.Status);
    }

    [Fact]
    public void Complete_FromDraft_ThrowsDomainException()
    {
        var record = CreateDraftInspection();

        Assert.Throws<DomainException>(() => record.Complete("reviewer-001"));
    }

    [Fact]
    public void Approve_FromPendingApproval_TransitionsToApproved()
    {
        var record = CreateCompletedInspection();

        record.Approve("manager-001");

        Assert.Equal(InspectionStatus.Approved, record.Status);
        Assert.NotNull(record.ApprovedAt);
        Assert.Equal("manager-001", record.ApprovedBy);
    }

    [Fact]
    public void Reject_FromPendingApproval_TransitionsToRejected()
    {
        var record = CreateCompletedInspection();

        record.Reject("manager-001", "Quality standards not met");

        Assert.Equal(InspectionStatus.Rejected, record.Status);
        Assert.NotNull(record.RejectedAt);
        Assert.Equal("manager-001", record.RejectedBy);
    }

    [Fact]
    public void Approve_AlreadyApproved_ThrowsDomainException()
    {
        var record = CreateCompletedInspection();
        record.Approve("manager-001");

        Assert.Throws<DomainException>(() => record.Approve("manager-002"));
    }

    [Fact]
    public void Cancel_FromInProgress_TransitionsToCancelled()
    {
        var record = CreateInProgressInspection();

        record.Cancel();

        Assert.Equal(InspectionStatus.Cancelled, record.Status);
    }

    [Fact]
    public void Cancel_FromApproved_ThrowsDomainException()
    {
        var record = CreateCompletedInspection();
        record.Approve("manager-001");

        Assert.Throws<DomainException>(() => record.Cancel());
    }
}
