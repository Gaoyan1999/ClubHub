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

    public List<T> Find(Func<T, bool> predicate) => _set.Where(predicate).ToList();

    public void Add(T item)
    {
        _set.Add(item);
        _context.SaveChanges();
    }

    public void Update(T item)
    {
        _set.Update(item);
        _context.SaveChanges();
    }

    public void Delete(T item)
    {
        _set.Remove(item);
        _context.SaveChanges();
    }
}
