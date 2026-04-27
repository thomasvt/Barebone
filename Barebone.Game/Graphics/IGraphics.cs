using System.Drawing;
using System.Numerics;
using Barebone.Geometry;
using Barebone.Graphics;

namespace Barebone.Game.Graphics
{
    public interface IGraphics
    {
        void ClearScreen(in Color color);
        void FillPolygon(in Polygon8 polygon, in Color? color = null);
        void FillCircle(Vector2 center, float radius, in int segmentCount, in Color color);

        /// <summary>
        /// The following draw calls will only use vertex colors. No textures.
        /// </summary>
        void SetColorOnly();

        /// <summary>
        /// Gets a texture by its assetPath. All textures are reused from memory after loading it from file once.
        /// </summary>
        ITexture GetTexture(string assetPath);
        /// <summary>
        /// Set a texture for projection onto your subsequent drawing of geometry, using the uvTransform for controlling that projection.
        /// </summary>
        void SetTexture(in ITexture texture, in Matrix3x2 projection);
        void Print(in Vector2 position, in string text, in Color color, in float scale = 1f);
        /// <summary>
        /// Sets the camera to use for subsequent rendering.
        /// </summary>
        void SetCamera(in ICamera camera);

        /// <summary>
        /// Sets a world transform applied to vertices BEFORE the active camera's WorldToScreen transform.
        /// The effective draw transform becomes <c>vertex * world * cameraWorldToScreen</c>.
        /// Defaults to <see cref="Matrix3x2.Identity"/>; pass identity to clear. Persists across draw calls until changed again.
        /// On platforms with GPU rendering this matrix is uploaded as a shader uniform.
        /// </summary>
        void SetWorldTransform(in Matrix3x2 world);

        /// <summary>
        /// Sets the world transform to zero translation, zero rotation and zero scaling. (= Identity matrix).
        /// </summary>
        void ResetWorldTransform();

        /// <summary>
        /// The currently active world transform (defaults to <see cref="Matrix3x2.Identity"/>).
        /// </summary>
        Matrix3x2 WorldTransform { get; }

        ICamera CreateCamera(float viewHeight, ScreenOrigin screenOrigin);

        /// <summary>
        /// Returns the active camera. Change it with ActivateCamera()
        /// </summary>
        ICamera Camera { get; }

        /// <summary>
        /// Calculates a projection to use when drawing geometry.
        /// </summary>
        /// <param name="textureOrigin">Location in teh world where the texture's TopLeft should align with.</param>
        /// <param name="texelsPerUnit">How many texture texels fit in 1 world unit.</param>
        Matrix3x2 CalculateTextureProjection(in ITexture texture, in Vector2 textureOrigin, in float texelsPerUnit);

        void SetBloom(in BloomConfig config);
        BloomConfig GetBloom();
    }
}
