using BerexQms.Application.Interfaces;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Data.Common;

namespace BerexQms.Infrastructure.Persistence.Interceptors;

/// <summary>
/// Sets the PostgreSQL session variable 'app.current_tenant_id' on each connection,
/// synchronizing EF Core's application-level tenant scoping with PostgreSQL's
/// Row-Level Security policies.
/// </summary>
public sealed class TenantConnectionInterceptor : DbConnectionInterceptor
{
    private readonly ITenantContext _tenantContext;

    public TenantConnectionInterceptor(ITenantContext tenantContext)
    {
        _tenantContext = tenantContext;
    }

    public override async Task ConnectionOpenedAsync(
        DbConnection connection,
        ConnectionEndEventData eventData,
        CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantContext.CurrentTenantId.Value;

        if (tenantId != Guid.Empty)
        {
            await using var cmd = connection.CreateCommand();
            cmd.CommandText = $"SET LOCAL app.current_tenant_id = '{tenantId}'";
            await cmd.ExecuteNonQueryAsync(cancellationToken);
        }
    }

    public override void ConnectionOpened(
        DbConnection connection,
        ConnectionEndEventData eventData)
    {
        var tenantId = _tenantContext.CurrentTenantId.Value;

        if (tenantId != Guid.Empty)
        {
            using var cmd = connection.CreateCommand();
            cmd.CommandText = $"SET LOCAL app.current_tenant_id = '{tenantId}'";
            cmd.ExecuteNonQuery();
        }
    }
}
