using System.Linq.Expressions;
using ClubHub.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ClubHub.Data;

/// <summary>EF Core implementation of <see cref="IRepository{T}"/> that works for any model class.</summary>
public class Repository<T> : IRepository<T> where T : class
{
    private readonly ClubHubDbContext _context;
    private readonly DbSet<T> _set;

    public Repository(ClubHubDbContext context)
    {
        _context = context;
        _set = context.Set<T>();
    }

    public List<T> GetAll() => _set.ToList();

    public T? GetById(int id) => _set.Find(id);

    public List<T> Find(Expression<Func<T, bool>> predicate) => _set.Where(predicate).ToList();

    public List<T> Find(Expression<Func<T, bool>> predicate, params Expression<Func<T, object?>>[] includes)
    {
        IQueryable<T> query = _set;
        foreach (var include in includes)
            query = query.Include(include);

        return query.Where(predicate).ToList();
    }

    public void Add(T item)
    {
        _set.Add(item);
        try
        {
            _context.SaveChanges();
        }
        catch
        {
            // Stop tracking the failed item so later saves are not blocked by it
            _context.Entry(item).State = EntityState.Detached;
            throw;
        }
    }

    public void Update(T item)
    {
        _set.Update(item);
        SaveOrUndo(item);
    }

    public void Delete(T item)
    {
        _set.Remove(item);
        SaveOrUndo(item);
    }

    // On failure, reload the item from the database so the in-memory copy matches what is saved
    private void SaveOrUndo(T item)
    {
        try
        {
            _context.SaveChanges();
        }
        catch
        {
            _context.Entry(item).Reload();
            throw;
        }
    }
}
