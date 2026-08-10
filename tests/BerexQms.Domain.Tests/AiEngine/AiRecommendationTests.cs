using BerexQms.Domain.AiEngine.Entities;
using BerexQms.Domain.AiEngine.Enums;
using BerexQms.Domain.AiEngine.Events;
using BerexQms.SharedKernel.Exceptions;
using BerexQms.SharedKernel.ValueObjects;

namespace BerexQms.Domain.Tests.AiEngine;

public class AiRecommendationTests
{
    private static readonly TenantId TestTenantId = TenantId.From(Guid.NewGuid());

    private static AiRecommendation CreateTestRecommendation(
        AiRecommendationType type = AiRecommendationType.DefectTrend,
        AiSeverity severity = AiSeverity.Medium,
        decimal confidence = 0.75m)
    {
        return AiRecommendation.Create(
            Guid.NewGuid(),
            TestTenantId,
            type,
            "Test Recommendation",
            "Defect trend detected in production line.",
            severity,
            "Inspection",
            null,
            confidence,
            "Defect rate increased 25% over 3 months.",
            "Inspection records, NCR history",
            "Review defect frequency and implement corrective action.",
            null);
    }

    [Fact]
    public void Create_ValidParameters_SetsPropertiesCorrectly()
    {
        var id = Guid.NewGuid();
        var recommendation = AiRecommendation.Create(
            id,
            TestTenantId,
            AiRecommendationType.SupplierRisk,
            "Supplier Risk Alert",
            "Supplier reject rate increasing trend.",
            AiSeverity.High,
            "SupplierQuality",
            "supplier-001",
            0.82m,
            "Reject rate increased from 2.1% to 5.4%.",
            "Inspection records, SCAR history",
            "Issue supplier corrective action request.",
            "ctx-001,ctx-002");

        Assert.Equal(id, recommendation.Id);
        Assert.Equal(TestTenantId, recommendation.TenantId);
        Assert.Equal("SupplierRisk", recommendation.RecommendationType);
        Assert.Equal("Supplier Risk Alert", recommendation.Title);
        Assert.Equal("Supplier reject rate increasing trend.", recommendation.Description);
        Assert.Equal("High", recommendation.Severity);
        Assert.Equal("SupplierQuality", recommendation.RelatedModule);
        Assert.Equal("supplier-001", recommendation.RelatedEntityId);
        Assert.Equal(0.82m, recommendation.ConfidenceScore);
        Assert.Equal("Generated", recommendation.Status);
        Assert.Equal("Reject rate increased from 2.1% to 5.4%.", recommendation.Reason);
        Assert.Equal("Inspection records, SCAR history", recommendation.SupportingData);
        Assert.Equal("Issue supplier corrective action request.", recommendation.RecommendedAction);
        Assert.Equal("ctx-001,ctx-002", recommendation.SourceContextIds);
        Assert.Null(recommendation.ReviewedAt);
        Assert.Null(recommendation.ReviewedBy);
        Assert.Null(recommendation.ReviewNotes);
    }

    [Fact]
    public void Create_RaisesCreatedDomainEvent()
    {
        var recommendation = CreateTestRecommendation();

        Assert.Single(recommendation.DomainEvents);
        var evt = recommendation.DomainEvents.First();
        Assert.IsType<AiRecommendationCreatedEvent>(evt);
    }

    [Fact]
    public void Create_EmptyTitle_ThrowsDomainException()
    {
        Assert.Throws<DomainException>(() => AiRecommendation.Create(
            Guid.NewGuid(), TestTenantId,
            AiRecommendationType.DefectTrend,
            "",
            "Description",
            AiSeverity.Medium,
            "Module",
            null, 0.5m, "Reason", null, null, null));
    }

    [Fact]
    public void Create_InvalidConfidenceScore_ThrowsDomainException()
    {
        Assert.Throws<DomainException>(() => AiRecommendation.Create(
            Guid.NewGuid(), TestTenantId,
            AiRecommendationType.DefectTrend,
            "Title",
            "Description",
            AiSeverity.Medium,
            "Module",
            null, 1.5m, "Reason", null, null, null));
    }

    [Fact]
    public void MarkReviewed_FromGenerated_SetsStatusAndReviewer()
    {
        var recommendation = CreateTestRecommendation();
        recommendation.ClearDomainEvents();

        recommendation.MarkReviewed("user-123");

        Assert.Equal("Reviewed", recommendation.Status);
        Assert.Equal("user-123", recommendation.ReviewedBy);
        Assert.NotNull(recommendation.ReviewedAt);
        Assert.Single(recommendation.DomainEvents);
    }

    [Fact]
    public void MarkReviewed_FromAccepted_ThrowsDomainException()
    {
        var recommendation = CreateTestRecommendation();
        recommendation.Accept("user-123", null);

        Assert.Throws<DomainException>(() => recommendation.MarkReviewed("user-456"));
    }

    [Fact]
    public void Accept_FromGenerated_SetsStatusAndNotes()
    {
        var recommendation = CreateTestRecommendation();
        recommendation.ClearDomainEvents();

        recommendation.Accept("user-123", "Approved for corrective action.");

        Assert.Equal("Accepted", recommendation.Status);
        Assert.Equal("user-123", recommendation.ReviewedBy);
        Assert.Equal("Approved for corrective action.", recommendation.ReviewNotes);
        Assert.NotNull(recommendation.ReviewedAt);
        Assert.Single(recommendation.DomainEvents);
    }

    [Fact]
    public void Accept_FromReviewed_Succeeds()
    {
        var recommendation = CreateTestRecommendation();
        recommendation.MarkReviewed("user-111");
        recommendation.ClearDomainEvents();

        recommendation.Accept("user-222", null);

        Assert.Equal("Accepted", recommendation.Status);
        Assert.Equal("user-222", recommendation.ReviewedBy);
    }

    [Fact]
    public void Reject_FromGenerated_SetsStatusAndNotes()
    {
        var recommendation = CreateTestRecommendation();
        recommendation.ClearDomainEvents();

        recommendation.Reject("user-123", "Not applicable to current context.");

        Assert.Equal("Rejected", recommendation.Status);
        Assert.Equal("Not applicable to current context.", recommendation.ReviewNotes);
    }

    [Fact]
    public void Reject_FromAccepted_ThrowsDomainException()
    {
        var recommendation = CreateTestRecommendation();
        recommendation.Accept("user-123", null);

        Assert.Throws<DomainException>(() => recommendation.Reject("user-456", "Reason"));
    }

    [Fact]
    public void MarkExpired_FromGenerated_SetsStatus()
    {
        var recommendation = CreateTestRecommendation();

        recommendation.MarkExpired();

        Assert.Equal("Expired", recommendation.Status);
    }

    [Fact]
    public void MarkExpired_FromAccepted_ThrowsDomainException()
    {
        var recommendation = CreateTestRecommendation();
        recommendation.Accept("user-123", null);

        Assert.Throws<DomainException>(() => recommendation.MarkExpired());
    }

    [Fact]
    public void MarkExpired_FromRejected_ThrowsDomainException()
    {
        var recommendation = CreateTestRecommendation();
        recommendation.Reject("user-123", null);

        Assert.Throws<DomainException>(() => recommendation.MarkExpired());
    }

    [Theory]
    [InlineData(AiRecommendationType.DefectTrend, "DefectTrend")]
    [InlineData(AiRecommendationType.SupplierRisk, "SupplierRisk")]
    [InlineData(AiRecommendationType.ProcessRisk, "ProcessRisk")]
    [InlineData(AiRecommendationType.DocumentGap, "DocumentGap")]
    [InlineData(AiRecommendationType.AuditRisk, "AuditRisk")]
    [InlineData(AiRecommendationType.CAPARecommendation, "CAPARecommendation")]
    public void Create_AllRecommendationTypes_StoresCorrectString(
        AiRecommendationType type, string expected)
    {
        var recommendation = AiRecommendation.Create(
            Guid.NewGuid(), TestTenantId,
            type, "Title", "Description",
            AiSeverity.Low, "Module", null, 0.5m,
            "Reason", null, null, null);

        Assert.Equal(expected, recommendation.RecommendationType);
    }

    [Theory]
    [InlineData(AiSeverity.Low, "Low")]
    [InlineData(AiSeverity.Medium, "Medium")]
    [InlineData(AiSeverity.High, "High")]
    [InlineData(AiSeverity.Critical, "Critical")]
    public void Create_AllSeverityLevels_StoresCorrectString(
        AiSeverity severity, string expected)
    {
        var recommendation = AiRecommendation.Create(
            Guid.NewGuid(), TestTenantId,
            AiRecommendationType.DefectTrend, "Title", "Description",
            severity, "Module", null, 0.5m,
            "Reason", null, null, null);

        Assert.Equal(expected, recommendation.Severity);
    }
}
