using Barebone.Geometry;

namespace Barebone.Monogame
{
    public interface IGame
    {
        void OnWindowSizeChanged(Vector2I newSize);
        void Update(float time, float deltaTime);
        void Draw(float time, float deltaTime);
    }
}
