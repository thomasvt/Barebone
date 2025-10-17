using Barebone.Geometry;

namespace Barebone
{
    public interface IGame : IDisposable
    {
        void Update(float time, float deltaT);
        void Draw(float time, float deltaT);
        void OnWindowSizeChanged(Vector2I newSize);
    }
}
