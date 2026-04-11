using System.Numerics;

namespace Barebone.Game.Graphics
{
    public interface ICamera
    {
        Vector2 Position { get; set; }
        float Zoom { get; set; }
        Vector2 ScreenToWorld(Vector2 screenPosition);
        Vector2 WorldToScreen(Vector2 position);
        float WorldLengthToScreen(float length);

        /// <summary>
        /// Forces the game to always render a fixed amount of world-height disregarding the window's size.
        /// What is horizontally visible depends on aspect ratio of the window.
        /// This combines with Zoom: the LogicalViewHeight is exactly applied when Zoom is 1.
        /// </summary>
        float? LogicalViewHeight { get; set; }
        float ScreenLengthToWorld(float length);
    }
}
