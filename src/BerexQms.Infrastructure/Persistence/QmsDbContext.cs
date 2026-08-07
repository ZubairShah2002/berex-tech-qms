using BerexQms.Application.Interfaces;
using BerexQms.Domain.Identity.Entities;
using BerexQms.Domain.Inspection.Entities;
using BerexQms.Domain.NonConformance.Entities;
using BerexQms.Domain.ProductCatalog.Entities;
using BerexQms.Domain.SupplierQuality.Entities;
using BerexQms.Domain.Calibration.Entities;
using BerexQms.Domain.Training.Entities;
using BerexQms.Domain.Spc.Entities;
using BerexQms.Domain.AiEngine.Entities;
using BerexQms.SharedKernel.Abstractions;
using BerexQms.SharedKernel.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BerexQms.Infrastructure.Persistence;

public class QmsDbContext : DbContext, IUnitOfWork
{
    private readonly ITenantContext _tenantContext;
    private readonly IClockService _clockService;
    private readonly ICurrentUserService _currentUserService;
    private IDbContextTransaction? _currentTransaction;

    public QmsDbContext(
        DbContextOptions<QmsDbContext> options,
        ITenantContext tenantContext,
        IClockService clockService,
        ICurrentUserService currentUserService)
        : base(options)
    {
        _tenantContext = tenantContext;
        _clockService = clockService;
        _currentUserService = currentUserService;
    }

    public DbSet<AuditLogEntry> AuditLogs => Set<AuditLogEntry>();
    public DbSet<DomainEventOutboxEntry> DomainEventsOutbox => Set<DomainEventOutboxEntry>();

    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<Tenant> Tenants => Set<Tenant>();

    public DbSet<Part> Parts => Set<Part>();
    public DbSet<PartRevision> PartRevisions => Set<PartRevision>();
    public DbSet<SpecificationParameter> SpecificationParameters => Set<SpecificationParameter>();
    public DbSet<BomReference> BomReferences => Set<BomReference>();

    public DbSet<InspectionRecord> InspectionRecords => Set<InspectionRecord>();
    public DbSet<Measurement> Measurements => Set<Measurement>();
    public DbSet<InspectionChecklist> InspectionChecklists => Set<InspectionChecklist>();
    public DbSet<ChecklistItem> ChecklistItems => Set<ChecklistItem>();
    public DbSet<SamplingPlan> SamplingPlans => Set<SamplingPlan>();

    public DbSet<NonConformanceRecord> NonConformanceRecords => Set<NonConformanceRecord>();
    public DbSet<ContainmentAction> ContainmentActions => Set<ContainmentAction>();
    public DbSet<Investigation> Investigations => Set<Investigation>();

    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<SupplierApproval> SupplierApprovals => Set<SupplierApproval>();
    public DbSet<SupplierScorecard> SupplierScorecards => Set<SupplierScorecard>();
    public DbSet<SCARRecord> ScarRecords => Set<SCARRecord>();
    public DbSet<ApprovedPart> ApprovedParts => Set<ApprovedPart>();

    public DbSet<Equipment> Equipment => Set<Equipment>();
    public DbSet<CalibrationRecord> CalibrationRecords => Set<CalibrationRecord>();
    public DbSet<CalibrationSchedule> CalibrationSchedules => Set<CalibrationSchedule>();
    public DbSet<GaugeControl> GaugeControls => Set<GaugeControl>();
    public DbSet<ImpactAssessment> ImpactAssessments => Set<ImpactAssessment>();

    public DbSet<Qualification> Qualifications => Set<Qualification>();
    public DbSet<TrainingCourse> TrainingCourses => Set<TrainingCourse>();
    public DbSet<CompetencyRecord> CompetencyRecords => Set<CompetencyRecord>();
    public DbSet<TrainingAssignment> TrainingAssignments => Set<TrainingAssignment>();

    public DbSet<ControlChart> ControlCharts => Set<ControlChart>();
    public DbSet<DataPoint> SpcDataPoints => Set<DataPoint>();

    public DbSet<AiInteraction> AiInteractions => Set<AiInteraction>();
    public DbSet<AiModel> AiModels => Set<AiModel>();
    public DbSet<AiCapabilityConfig> AiCapabilityConfigs => Set<AiCapabilityConfig>();
    public DbSet<AiActionLog> AiActionLogs => Set<AiActionLog>();
    public DbSet<AiPermissionPolicy> AiPermissionPolicies => Set<AiPermissionPolicy>();
    public DbSet<AiWorkflowDefinition> AiWorkflowDefinitions => Set<AiWorkflowDefinition>();
    public DbSet<AiWorkflowExecution> AiWorkflowExecutions => Set<AiWorkflowExecution>();
    public DbSet<AiContextDocument> AiContextDocuments => Set<AiContextDocument>();
    public DbSet<AiKnowledgeSource> AiKnowledgeSources => Set<AiKnowledgeSource>();
    public DbSet<AiRecommendation> AiRecommendations => Set<AiRecommendation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("shared");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(QmsDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SetAuditFields();
        return await base.SaveChangesAsync(cancellationToken);
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        _currentTransaction ??= await Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction is null) return;
        await _currentTransaction.CommitAsync(cancellationToken);
        await _currentTransaction.DisposeAsync();
        _currentTransaction = null;
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction is null) return;
        await _currentTransaction.RollbackAsync(cancellationToken);
        await _currentTransaction.DisposeAsync();
        _currentTransaction = null;
    }

    private void SetAuditFields()
    {
        var now = _clockService.UtcNow;
        var userId = _currentUserService.IsAuthenticated
            ? _currentUserService.UserId.ToString()
            : Guid.Empty.ToString();

        foreach (var entry in ChangeTracker.Entries<IAuditableEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = now;
                    entry.Entity.CreatedBy = userId;
                    break;
                case EntityState.Modified:
                    entry.Entity.ModifiedAt = now;
                    entry.Entity.ModifiedBy = userId;
                    break;
            }
        }
    }
}

public class AuditLogEntry
{
    public long Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid UserId { get; set; }
    public DateTime Timestamp { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public Guid EntityId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public string? SourceIp { get; set; }
    public string? CorrelationId { get; set; }
    public string ModuleName { get; set; } = string.Empty;
}

public class DomainEventOutboxEntry
{
    public long Id { get; set; }
    public Guid EventId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public Guid TenantId { get; set; }
    public string AggregateType { get; set; } = string.Empty;
    public Guid AggregateId { get; set; }
    public string Payload { get; set; } = string.Empty;
    public DateTime OccurredOn { get; set; }
    public DateTime? ProcessedOn { get; set; }
    public string? Error { get; set; }
    public int RetryCount { get; set; }
}
