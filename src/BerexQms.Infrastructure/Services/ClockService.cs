using BerexQms.Application.Interfaces;

namespace BerexQms.Infrastructure.Services;

public sealed class ClockService : IClockService
{
    public DateTime UtcNow => DateTime.UtcNow;
}
