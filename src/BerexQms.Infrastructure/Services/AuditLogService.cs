using BerexQms.Application.Interfaces;
using BerexQms.Infrastructure.Persistence;

namespace BerexQms.Infrastructure.Services;

public sealed class AuditLogService : IAuditLogService
{
    private readonly QmsDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ITenantContext _tenantContext;
    private readonly IClockService _clockService;

    public AuditLogService(
        QmsDbContext context,
        ICurrentUserService currentUserService,
        ITenantContext tenantContext,
        IClockService clockService)
    {
        _context = context;
        _currentUserService = currentUserService;
        _tenantContext = tenantContext;
        _clockService = clockService;
    }

    public async Task LogAsync(
        string entityType,
        string entityId,
        string action,
        string? oldValue,
        string? newValue,
        CancellationToken ct)
    {
        var entry = new AuditLogEntry
        {
            TenantId = _tenantContext.CurrentTenantId.Value,
            UserId = _currentUserService.IsAuthenticated ? _currentUserService.UserId : Guid.Empty,
            Timestamp = _clockService.UtcNow,
            EntityType = entityType,
            EntityId = Guid.TryParse(entityId, out var parsed) ? parsed : Guid.Empty,
            Action = action,
            OldValue = oldValue,
            NewValue = newValue,
            ModuleName = entityType.Split('.').FirstOrDefault() ?? "Unknown"
        };

        _context.AuditLogs.Add(entry);
        await _context.SaveChangesAsync(ct);
    }
}
