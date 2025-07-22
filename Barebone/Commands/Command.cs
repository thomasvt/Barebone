using Barebone.Pools;

namespace Barebone.Commands
{
    public abstract class Command : Poolable
    {
        public abstract void Handle();
    }
}
