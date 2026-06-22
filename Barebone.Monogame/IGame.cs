using Barebone.Geometry;

namespace Barebone.Monogame
{
    public interface IGame
    {
        void OnWindowSizeChanged(Vector2I newSize);
        void Update(float time, float deltaTime);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fraction">Fraction [0,1) of where we are between updates.</param>
        /// <param name="time">Current total application time in seconds</param>
        /// <param name="deltaTime">Time in seconds sinds previous Draw call.</param>
        void Draw(float fraction, float time, float deltaTime);
    }
}
