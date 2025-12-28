using Barebone.Graphics.Gpu;
using System.Numerics;
using System.Runtime.CompilerServices;
using Barebone.Geometry;

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
