namespace Barebone.Pools;

public interface IPool
{
    string GetReport();
    int RentedCount { get; }
    string Name { get; }
    void Return(object o);
    void ReturnNoDestruct(object o);
}