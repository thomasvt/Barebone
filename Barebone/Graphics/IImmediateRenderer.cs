using System.Drawing;
using System.Numerics;
using Barebone.Geometry;
using Barebone.Graphics.Cameras;
using Barebone.Graphics.Gpu;
using Barebone.Graphics.Sprites;

namespace Barebone.Graphics
{
    public interface IImmediateRenderer
    {
        void Begin(ICamera camera, bool enableDepthBuffer, bool additiveBlend, bool cullCounterClockwise, bool linearSampling = true);

        void Draw(in Matrix4x4 worldTransform, in ReadOnlySpan<GpuTriangle> triangles, in Color? replacementColor = null);
        void Draw(in Matrix4x4 worldTransform, in ReadOnlySpan<GpuTexTriangle> triangles, in ITexture texture);
        void Draw(in Matrix4x4 worldTransform, in Mesh mesh, in Color? replacementColor = null);

        /// <summary>
        /// Draws a quad with the sprite on it. If Scale is 1, 1 world unit equals one pixel of the sprite.
        /// </summary>
        void Draw(in Matrix4x4 worldTransform, in Sprite sprite, Color tint, bool flipX = false, float scale = 1f);

        void End();

        void PushClip(AabbI clipInScreenPx);
        void ResetClip();
        void PopClip();

        Viewport Viewport { get; }

        void ClearScreen(in System.Drawing.Color color);

        void ClearDepthBuffer();


        /// <summary>
        /// Creates a special <see cref="ITexture"/> that you can set as RenderTarget on this Renderer. You must Dispose() this yourself.
        /// </summary>
        /// <param name="supportDepthBuffer">Also allow to render with Z-buffer to this sprite. This allocates more memory.</param>
        Sprite CreateRenderTargetSprite(Vector2I size, bool supportDepthBuffer, int preferredMultiSampleCount = 0);

        void SwitchRenderTargetTo(ITexture texture);
        void SwitchRenderTargetToScreen();
        void EnableMultiSampling();
    }
}
