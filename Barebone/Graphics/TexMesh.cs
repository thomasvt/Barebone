using System.Drawing;
using Barebone.Graphics.Gpu;
using System.Numerics;
using System.Runtime.CompilerServices;
using Barebone.Geometry;
using Barebone.UI.Text;

namespace Barebone.Graphics
{
    /// <summary>
    /// Renderable Mesh for 2D procedural drawing methods, with a single Texture projected onto the Z plane. Not threadsafe!
    /// </summary>
    public class TexMesh : MeshBase<TexMesh, GpuTexTriangle, GpuTexVertex>
    {
        public Vector2 TextureOrigin { get; set; } = Vector2.Zero;
        public Vector2 TextureScale { get; set; } = Vector2.One;
        public ITexture? Texture { get; set; }

        /// <summary>
        /// Prints text onto the TexMesh using the specified Font. Note that the texture of the font will be assigned as the texture of this TexMesh and a Texmesh can have only one texture.
        /// </summary>
        public void Print(bool yPointsDown, Vector2 position, string text, Color color, Font font, float scale = 1f, float z = 0f)
        {
            if (Texture != null && Texture != font.Texture)
                throw new InvalidOperationException("Cannot print text: the TexMesh already has a different texture assigned than that of the font.");

            Texture = font.Texture;
            font.AppendString(yPointsDown, Triangles, text, color, position, scale, z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override GpuTexTriangle ToTriangle(in GpuTexVertex a, in GpuTexVertex b, in GpuTexVertex c)
        {
            return new GpuTexTriangle(a, b, c);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override GpuTexTriangle ToTriangle(Vector3 a, Vector3 b, Vector3 c, GpuColor gpuColor)
        {
            var vertexA = new GpuTexVertex(a, gpuColor, (a.XY() - TextureOrigin) * TextureScale);
            var vertexB = new GpuTexVertex(b, gpuColor, (b.XY() - TextureOrigin) * TextureScale);
            var vertexC = new GpuTexVertex(c, gpuColor, (c.XY() - TextureOrigin) * TextureScale);

            return new GpuTexTriangle(vertexA, vertexB, vertexC);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override GpuTexVertex ToVertex(Vector3 a, GpuColor gpuColor)
        {
            return new GpuTexVertex(a, gpuColor, (a.XY() - TextureOrigin) * TextureScale);
        }
    }
}
