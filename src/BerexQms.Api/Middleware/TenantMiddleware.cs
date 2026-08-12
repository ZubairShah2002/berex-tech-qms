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
        // JWT tenant claim is the authoritative source for authenticated users.
        // The X-Tenant-Id header is only accepted when it matches the JWT claim
        // (for multi-tenant frontend routing), or for unauthenticated endpoints.
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var tenantClaim = context.User.FindFirst("tenant_id")?.Value;
            if (!Guid.TryParse(tenantClaim, out var claimTenantGuid)
                || claimTenantGuid == Guid.Empty)
            {
                // Authenticated user with no valid tenant claim — reject.
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsJsonAsync(new { error = "Missing or invalid tenant context." });
                return;
            }

            // If header is also present, it must match the JWT claim.
            if (context.Request.Headers.TryGetValue(TenantHeader, out var tenantHeader)
                && Guid.TryParse(tenantHeader, out var headerGuid)
                && headerGuid != claimTenantGuid)
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsJsonAsync(new { error = "Tenant header does not match authenticated tenant." });
                return;
            }

            tenantContext.SetTenant(new TenantId(claimTenantGuid));
        }
        else
        {
            // Unauthenticated requests (login, register, health checks) — no tenant required.
            if (context.Request.Headers.TryGetValue(TenantHeader, out var tenantHeader)
                && Guid.TryParse(tenantHeader, out var headerGuid))
            {
                tenantContext.SetTenant(new TenantId(headerGuid));
            }
        }

        using (Serilog.Context.LogContext.PushProperty("TenantId", tenantContext.CurrentTenantId.Value))
        {
            await _next(context);
        }
    }
}
