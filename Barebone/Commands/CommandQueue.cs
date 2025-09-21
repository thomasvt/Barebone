using Barebone.Pools;

namespace Barebone.Commands
{
    /// <summary>
    /// In games this is used to trigger certain logic, but delay actual execution until a phase in the gameloop that is safe to do so.
    /// For instance in concurrency situations, or when altering the scene tree from within loops that would cause the looped collection to be modified.
    /// Commands also formalize game functionality.
    /// </summary>
    public class CommandQueue
    {
        private readonly Queue<Command> _commands = new ();

        public T Dispatch<T>() where T : Command, new()
        {
            var cmd = Pool.Rent<T>();
            _commands.Enqueue(cmd);
            return cmd;
        }

        public void HandleAll()
        {
            while (_commands.Count > 0)
            {
                var command = _commands.Dequeue();
                command.Handle();
                command.Return();
            }
        }

        public void Clear()
        {
            _commands.Clear();
        }
    }
}
