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

        void Draw(in Matrix4x4 worldTransform, in ReadOnlySpan<GpuTriangle> triangles);
        void Draw(in Matrix4x4 worldTransform, in ReadOnlySpan<GpuTexTriangle> triangles, in ITexture? texture = null);
        void Draw(in Matrix4x4 worldTransform, in Mesh mesh);
        void Draw(in Matrix4x4 worldTransform, in ColorMesh mesh);
        void Draw(in Matrix4x4 worldTransform, in TexMesh mesh);

        /// <summary>
        /// Draws a quad with the sprite on it. If Scale is 1, 1 world unit equals one pixel of the sprite.
        /// </summary>
        void Draw(in Matrix4x4 worldTransform, in Sprite sprite, Color? tint = null, bool flipX = false, float scale = 1f);

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
        /// <param name="preserveContents">Keep the existing pixels when this RT is bound (e.g. for additive read-modify-write).
        /// Default <c>false</c> = backend may discard previous contents on bind, which is faster.</param>
        Sprite CreateRenderTargetSprite(Vector2I size, bool supportDepthBuffer, int preferredMultiSampleCount = 0, bool preserveContents = false);

        void SetRenderTarget(ITexture texture);
        void ResetRenderTargetToScreen();
        void EnableMultiSampling();

        /// <summary>
        /// Activates a custom <paramref name="effect"/> for subsequent <see cref="Draw"/> calls and switches the blend mode.
        /// The custom effect owns its own parameter values; the renderer no longer touches World/Texture on the default effect.
        /// Cleared by <see cref="ResetEffect"/>, <see cref="Begin"/>, or <see cref="End"/>.
        /// </summary>
        void SetEffect(IEffect effect, BlendMode blendMode);

        /// <summary>
        /// Reverts to the renderer's default effect and restores the blend mode that <see cref="Begin"/> established.
        /// </summary>
        void ResetEffect();
    }
}
