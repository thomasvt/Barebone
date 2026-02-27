namespace Barebone.Commands
{
    public interface ICommandQueue
    {
        T Dispatch<T>() where T : Command, new();
        void HandleAll();
        void Clear();
    }
}
