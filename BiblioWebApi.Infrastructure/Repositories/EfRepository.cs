using BiblioWebApi.Application.Abstractions;
using BiblioWebApi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BiblioWebApi.Infrastructure.Repositories;

public sealed class EfRepository<TEntity>(BiblioDbContext db) : IRepository<TEntity> where TEntity : class
{
    public IQueryable<TEntity> Query(bool asNoTracking = true)
    {
        var query = db.Set<TEntity>().AsQueryable();
        return asNoTracking ? query.AsNoTracking() : query;
    }

    public ValueTask<TEntity?> GetByIdAsync(object id, CancellationToken cancellationToken = default) => db.Set<TEntity>().FindAsync([id], cancellationToken);
    public Task AddAsync(TEntity entity, CancellationToken cancellationToken = default) => db.Set<TEntity>().AddAsync(entity, cancellationToken).AsTask();
    public void Remove(TEntity entity) => db.Set<TEntity>().Remove(entity);
}

public sealed class EfUnitOfWork(BiblioDbContext db) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => db.SaveChangesAsync(cancellationToken);
}


