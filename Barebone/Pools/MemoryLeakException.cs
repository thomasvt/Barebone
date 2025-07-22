namespace Barebone.Pools;

public class MemoryLeakException : Exception
{
    public MemoryLeakException(string message) : base(message)
    {
    }
}