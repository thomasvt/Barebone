using System.Drawing;
using System.Numerics;
using Barebone.Graphics;

namespace Barebone.Game.Graphics
{
    public interface IPlatformGraphics
    {
        void ClearScreen(in Color color);

        /// <summary>
        /// Sets the active world + camera transforms used by subsequent <see cref="FillTriangles"/> calls.
        /// The intended pipeline applied per vertex is: <c>vertex * worldTransform * cameraWorldToScreen</c>.
        /// Backends that have a real GPU pipeline upload these as shader uniforms; backends rendering through
        /// a CPU/screen-space rasterizer (e.g. SDL2 2D) apply them on the CPU before drawing.
        /// </summary>
        void SetTransform(in Matrix3x2 worldTransform, in Matrix3x2 cameraTransform);

        /// <summary>
        /// Submits triangles. Vertex positions are in WORLD space (i.e. before the active camera transform);
        /// the transforms set via <see cref="SetTransform"/> are applied by the backend.
        /// </summary>
        void FillTriangles(in ReadOnlySpan<Vertex> vertices, ITexture? texture, in float zLayer);

        ITexture GetTexture(string assetPath);

        float BloomThreshold { get; set; } // bloom only when Max(R, G, B) > BloomTreshold.
        float BloomSoftKnee { get; set; } // Width of the fade edge of the bloom. So less is sharp edges, more is slower fadeout
        float BloomBrightIntensity { get; set; } // mostly irrelevant in non HDR. Use FinalIntensity
        float BloomUpsampleStrength { get; set; }
        float BloomFinalIntensity { get; set; }
    }
}
