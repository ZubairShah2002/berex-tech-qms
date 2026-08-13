using BerexQms.SharedKernel.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace BerexQms.Infrastructure.Persistence;

/// <summary>
/// EF Core implementation of IExecutionStrategyFactory.
/// Delegates to the DbContext's configured execution strategy, enabling
/// retrying execution strategies to work with user-initiated transactions.
/// </summary>
public sealed class EfExecutionStrategyFactory : IExecutionStrategyFactory
{
    private readonly QmsDbContext _dbContext;

    public EfExecutionStrategyFactory(QmsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<TResult> ExecuteAsync<TResult>(Func<Task<TResult>> operation)
    {
        var strategy = _dbContext.Database.CreateExecutionStrategy();
        return strategy.ExecuteAsync(operation);
    }
}
