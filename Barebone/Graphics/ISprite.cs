using Barebone.Geometry;

namespace Barebone.Graphics
{
    public interface ISprite : IDisposable
    {
        Aabb AabbPx { get; }
}
}
