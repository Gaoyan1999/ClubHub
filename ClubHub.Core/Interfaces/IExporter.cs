namespace ClubHub.Core.Interfaces;

public interface IExporter<T>
{
    void Export(IEnumerable<T> items, string filePath);
}
