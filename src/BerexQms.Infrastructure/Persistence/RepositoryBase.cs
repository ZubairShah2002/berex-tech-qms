using BerexQms.SharedKernel.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace BerexQms.Infrastructure.Persistence;

public abstract class RepositoryBase<T> : IRepository<T> where T : AggregateRoot<Guid>
{
    protected readonly QmsDbContext Context;

    protected RepositoryBase(QmsDbContext context)
    {
        Context = context;
    }

    protected DbSet<T> DbSet => Context.Set<T>();

    public virtual async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet.FindAsync(new object[] { id }, cancellationToken);
    }

    public virtual async Task<IReadOnlyList<T>> ListAllAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet.ToListAsync(cancellationToken);
    }

    public virtual async Task<IReadOnlyList<T>> ListAsync(
        ISpecification<T> specification,
        CancellationToken cancellationToken = default)
    {
        return await ApplySpecification(specification).ToListAsync(cancellationToken);
    }

    public virtual async Task<T?> FirstOrDefaultAsync(
        ISpecification<T> specification,
        CancellationToken cancellationToken = default)
    {
        return await ApplySpecification(specification).FirstOrDefaultAsync(cancellationToken);
    }

    public virtual async Task<int> CountAsync(
        ISpecification<T> specification,
        CancellationToken cancellationToken = default)
    {
        return await ApplySpecification(specification, true).CountAsync(cancellationToken);
    }

    public virtual async Task<bool> AnyAsync(
        ISpecification<T> specification,
        CancellationToken cancellationToken = default)
    {
        return await ApplySpecification(specification, true).AnyAsync(cancellationToken);
    }

    public virtual async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        var entry = await DbSet.AddAsync(entity, cancellationToken);
        return entry.Entity;
    }

    public virtual Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        DbSet.Update(entity);
        return Task.CompletedTask;
    }

    public virtual Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
    {
        DbSet.Remove(entity);
        return Task.CompletedTask;
    }

    private IQueryable<T> ApplySpecification(ISpecification<T> specification, bool countOnly = false)
    {
        IQueryable<T> query = DbSet;

        if (specification.Criteria is not null)
        {
            query = query.Where(specification.Criteria);
        }

        if (!countOnly)
        {
            foreach (var includeString in specification.IncludeStrings)
            {
                query = query.Include(includeString);
            }

            foreach (var include in specification.Includes)
            {
                query = query.Include(include);
            }

            if (specification.OrderBy is not null)
            {
                query = query.OrderBy(specification.OrderBy);
            }
            else if (specification.OrderByDescending is not null)
            {
                query = query.OrderByDescending(specification.OrderByDescending);
            }

            if (specification.IsPagingEnabled)
            {
                query = query.Skip(specification.Skip ?? 0).Take(specification.Take ?? 10);
            }
        }

        return query;
    }
}
