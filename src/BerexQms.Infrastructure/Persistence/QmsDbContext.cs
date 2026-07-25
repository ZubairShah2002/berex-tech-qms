using BerexQms.Application.Interfaces;
using BerexQms.Domain.Identity.Entities;
using BerexQms.Domain.ProductCatalog.Entities;
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
