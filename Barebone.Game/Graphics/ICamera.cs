using System.Numerics;
using Barebone.Geometry;

namespace Barebone.Game.Graphics
{
    public interface ICamera
    {
        Vector2 Position { get; set; }
        float Zoom { get; set; }
        float? LogicalViewHeight { get; set; }
        Matrix3x2 WorldToScreenTransform { get; }
        Matrix3x2 ScreenToWorldTransform { get; }
        Vector2 ScreenToWorld(Vector2 screenPosition);
        float ScreenLengthToWorld(float length);
        Vector2 WorldToScreen(Vector2 position);
        float WorldLengthToScreen(float length);
    }
}
