using System.Linq.Expressions;
using AuthService.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Data.Repositories.Implementations;
public abstract class Repository<T> : IRepository<T> where T : class
{
    protected readonly AuthContext _context;

    public Repository(AuthContext context)
    {
        _context = context;
    }

    public async Task Add(T entity)
    {
        await _context.AddAsync(entity);
    }

    public Task AddRange(IEnumerable<T> entities)
    {
        return _context.AddRangeAsync(entities);
    }

    public void Remove(T entity)
    {
        if (entity != null)
        {
            _context.Remove(entity);
        }
    }

    public async Task Remove(int id)
    {
        var entity = await _context.Set<T>().FindAsync(id);
        if (entity != null)
        {
            _context.Remove(entity);
        }
    }

    public void RemoveRange(IEnumerable<T> entities)
    {
        if (entities != null)
        {
            _context.RemoveRange(entities);
        }
    }

    public async Task<T?> Get(int id)
    {
        var item = await _context.Set<T>().FindAsync(id);
        if (item != null)
        {
            _context.Entry(item).State = EntityState.Detached;
            return item;
        }

        return null;
    }

    public Task<T?> Get(Expression<Func<T, bool>> predicate)
    {
        return _context.Set<T>().FirstOrDefaultAsync(predicate)!;
    }

    public async Task<IEnumerable<T>> GetAll()
    {
        return await _context.Set<T>()
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<T>> GetAll(Expression<Func<T, bool>> predicate)
    {
        return await _context.Set<T>()
            .AsNoTracking()
            .Where(predicate)
            .ToListAsync();
    }

    public async Task<IEnumerable<T>> GetAllAsTracking()
    {
        return await _context.Set<T>()
            .ToListAsync();
    }

    public async Task<IEnumerable<T>> GetAllAsTracking(Expression<Func<T, bool>> predicate)
    {
        return await _context.Set<T>()
            .Where(predicate)
            .ToListAsync();
    }

    public async Task<T?> GetAsTracking(int id)
    {
        return await _context.Set<T>().FindAsync(id);
    }

    public Task<T?> GetAsTracking(Expression<Func<T, bool>> predicate)
    {
        return _context.Set<T>().FirstOrDefaultAsync(predicate);
    }

    public Task SaveChanges()
    {
        return _context.SaveChangesAsync();
    }
}