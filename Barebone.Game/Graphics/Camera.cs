using System.Numerics;

namespace Barebone.Game.Graphics
{
    public interface ICamera
    {
        Vector2 Position { get; set; }
        float Zoom { get; set; }
    }

    public class Camera : ICamera
    {
        public Vector2 Position { get; set; }
        public float Zoom { get; set; } = 1f;

        public void PrepareFrame(Vector2 viewportSize)
        {
            var viewportCenter = viewportSize * 0.5f;
            WorldToScreenTransform = Matrix3x2.CreateTranslation(-Position) * Matrix3x2.CreateScale(Zoom) * Matrix3x2.CreateTranslation(viewportCenter);
            ScreenToWorldTransform = Matrix3x2.CreateTranslation(-viewportCenter) * Matrix3x2.CreateScale(-Zoom) * Matrix3x2.CreateTranslation(Position);
        }

        public Matrix3x2 WorldToScreenTransform { get; private set; }
        public Matrix3x2 ScreenToWorldTransform { get; private set; }
    }
}
