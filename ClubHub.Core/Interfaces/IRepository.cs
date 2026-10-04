using System.Linq.Expressions;

namespace ClubHub.Core.Interfaces;

/// <summary>Generic data access for any model class.</summary>
public interface IRepository<T> where T : class
{
    List<T> GetAll();
    T? GetById(int id);
    // Expression (not Func) so EF can turn the filter into a SQL WHERE clause
    List<T> Find(Expression<Func<T, bool>> predicate);
    void Add(T item);
    void Update(T item);
    void Delete(T item);
}
