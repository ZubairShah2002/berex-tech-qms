using BerexQms.Application.Interfaces;
using BerexQms.SharedKernel.ValueObjects;

namespace BerexQms.Api.Middleware;

public sealed class TenantMiddleware
{
    private const string TenantHeader = "X-Tenant-Id";
    private readonly RequestDelegate _next;

    public TenantMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ITenantContext tenantContext)
    {
        if (context.Request.Headers.TryGetValue(TenantHeader, out var tenantHeader)
            && Guid.TryParse(tenantHeader, out var tenantGuid))
        {
            tenantContext.SetTenant(new TenantId(tenantGuid));
        }
        else if (context.User.Identity?.IsAuthenticated == true)
        {
            var tenantClaim = context.User.FindFirst("tenant_id")?.Value;
            if (Guid.TryParse(tenantClaim, out var claimTenantGuid))
            {
                tenantContext.SetTenant(new TenantId(claimTenantGuid));
            }
        }

        using (Serilog.Context.LogContext.PushProperty("TenantId", tenantContext.CurrentTenantId.Value))
        {
            await _next(context);
        }
    }
}
