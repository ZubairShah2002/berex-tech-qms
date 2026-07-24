using BerexQms.Application.Interfaces;
using BerexQms.SharedKernel.ValueObjects;

namespace BerexQms.Infrastructure.Services;

public sealed class TenantContext : ITenantContext
{
    public TenantId CurrentTenantId { get; private set; }

    public void SetTenant(TenantId tenantId)
    {
        CurrentTenantId = tenantId;
    }
}
