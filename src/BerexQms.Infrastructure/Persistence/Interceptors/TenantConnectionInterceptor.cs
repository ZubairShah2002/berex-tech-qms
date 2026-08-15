using BerexQms.Application.Interfaces;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using System.Data.Common;

namespace BerexQms.Infrastructure.Persistence.Interceptors;

/// <summary>
/// Sets the PostgreSQL session variable 'app.current_tenant_id' on each connection,
/// synchronizing EF Core's application-level tenant scoping with PostgreSQL's
/// Row-Level Security policies.
///
/// Failures are logged but swallowed so that a managed PostgreSQL host that
/// rejects custom GUC parameters does not cascade into every DB operation.
/// </summary>
public sealed class TenantConnectionInterceptor : DbConnectionInterceptor
{
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<TenantConnectionInterceptor> _logger;

    public TenantConnectionInterceptor(
        ITenantContext tenantContext,
        ILogger<TenantConnectionInterceptor> logger)
    {
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public override async Task ConnectionOpenedAsync(
        DbConnection connection,
        ConnectionEndEventData eventData,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var tenantId = _tenantContext.CurrentTenantId.Value;

            await using var cmd = connection.CreateCommand();
            if (tenantId != Guid.Empty)
            {
                cmd.CommandText = $"SET LOCAL app.current_tenant_id = '{tenantId}'";
            }
            else
            {
                // Clear any stale tenant from a previously pooled connection
                cmd.CommandText = "RESET app.current_tenant_id";
            }
            await cmd.ExecuteNonQueryAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            // Log but don't throw — managed PostgreSQL may reject custom GUC params.
            // RLS tenant filtering is enforced at the EF Core level via global query filters.
            _logger.LogWarning(ex,
                "Failed to set/reset tenant session variable — RLS variable not applied, " +
                "but EF Core global query filters still enforce tenant isolation");
        }
    }

    public override void ConnectionOpened(
        DbConnection connection,
        ConnectionEndEventData eventData)
    {
        try
        {
            var tenantId = _tenantContext.CurrentTenantId.Value;

            using var cmd = connection.CreateCommand();
            if (tenantId != Guid.Empty)
            {
                cmd.CommandText = $"SET LOCAL app.current_tenant_id = '{tenantId}'";
            }
            else
            {
                // Clear any stale tenant from a previously pooled connection
                cmd.CommandText = "RESET app.current_tenant_id";
            }
            cmd.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "Failed to set/reset tenant session variable — RLS variable not applied, " +
                "but EF Core global query filters still enforce tenant isolation");
        }
    }
}
