namespace ClubHub.Core.Interfaces;

/// <summary>Generic data access for any model class.</summary>
public interface IRepository<T> where T : class
{
    List<T> GetAll();
    T? GetById(int id);
    List<T> Find(Func<T, bool> predicate);
    void Add(T item);
    void Update(T item);
    void Delete(T item);
}
