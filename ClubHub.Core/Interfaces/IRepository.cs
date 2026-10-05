using System.Linq.Expressions;

namespace ClubHub.Core.Interfaces;

/// <summary>Generic data access for any model class.</summary>
public interface IRepository<T> where T : class
{
    List<T> GetAll();
    T? GetById(int id);
    // Expression (not Func) so EF can turn the filter into a SQL WHERE clause
    List<T> Find(Expression<Func<T, bool>> predicate);

    /// <summary>Like Find, but also loads related data, e.g. <c>e => e.Room</c>.</summary>
    List<T> Find(Expression<Func<T, bool>> predicate, params Expression<Func<T, object?>>[] includes);
    void Add(T item);
    void Update(T item);
    void Delete(T item);
}
