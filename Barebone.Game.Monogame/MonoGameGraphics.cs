using System.Drawing;
using System.Numerics;
using Barebone.Assets;
using Barebone.Game.Graphics;
using Barebone.Geometry;
using Barebone.Graphics;
using Barebone.Graphics.Gpu;
using Barebone.Graphics.Sprites;
using ICamera = Barebone.Graphics.Cameras.ICamera;

namespace Barebone.Game.Monogame
{
    /// <summary>
    /// MonoGame-based <see cref="IPlatformGraphics"/> with manual SSAA antialiasing and mip-chain bloom.
    /// Per frame:
    ///   - Scene  -> 2x-size <c>scene2x</c> RT (the SSAA target)
    ///   - Bright pass with bilinear downsample -> bloom mip[0] (also acts as the SSAA resolve into bloom)
    ///   - 4 downsample passes (mip[0] -> mip[1] -> ... -> mip[4])
    ///   - 4 additive upsample passes (mip[4] -> mip[3] -> ... -> mip[0])
    ///   - Composite: scene2x bilinear (SSAA resolve) + bloom mip[0] -> backbuffer
    /// Bloom/composite passes drive their own shaders through <see cref="IImmediateRenderer.SetEffect"/>.
    /// </summary>
    public class MonoGameGraphics(IImmediateRenderer renderer, ITextureLoader textureLoader, IEffectLoader effectLoader) : IPlatformGraphics, IDisposable
    {
        private const int BloomMipCount = 5;
        private const int SsaaScale = 2;

        public float BloomThreshold { get; set; } = 0.9f;       // bloom only when Max(R, G, B) > BloomThreshold.
        public float BloomSoftKnee { get; set; } = 0.5f;        // Width of the fade edge of the bloom. Less = sharp edges, more = slower fadeout.
        public float BloomBrightIntensity { get; set; } = 1.0f; // mostly irrelevant in non-HDR. Use BloomFinalIntensity.
        public float BloomUpsampleStrength { get; set; } = 1.0f;
        public float BloomFinalIntensity { get; set; } = 0.5f;

        private readonly IEffect _brightEffect = LoadEmbeddedEffect(effectLoader, "BrightPass");
        private readonly IEffect _downsampleEffect = LoadEmbeddedEffect(effectLoader, "Downsample");
        private readonly IEffect _upsampleEffect = LoadEmbeddedEffect(effectLoader, "Upsample");
        private readonly IEffect _compositeEffect = LoadEmbeddedEffect(effectLoader, "Composite");

        // Buffered per-frame draw stream, replayed in RenderFrame.
        private GpuTexTriangle[] _cpuTriangles = new GpuTexTriangle[1024];
        private int _cpuTriangleCount;
        private readonly BBList<DrawBatch> _batches = new();

        // Camera adapters used to drive XnaImmediateRenderer.Begin().
        private readonly Scene2DCamera _sceneCamera = new();
        private readonly static NullCamera _ndcCamera = new();

        // Render targets (Sprite wraps an ITexture / RenderTarget2D).
        private Vector2I _backbufferSize;
        private Vector2I _scene2XSize;
        private Sprite? _scene2X;
        private Sprite[] _bloomMips = [];
        private Vector2I[] _bloomMipSizes = [];

        private readonly Dictionary<string, ITexture> _textureCache = new();
        private Color _clearColor = Color.Black;

        private readonly static Color TransparentBlack = Color.FromArgb(0, 0, 0, 0);

        private Matrix3x2 _currentWorld = Matrix3x2.Identity;
        private Matrix3x2 _currentCamera = Matrix3x2.Identity;

        // fullscreen triangle covering [-1..1] with UV in [0..1] inside the visible region.
        private readonly static GpuTexTriangle[] FullscreenTriangle =
        {
            new(
                new GpuTexVertex(new Vector3(-1f,  1f, 0f), GpuColor.White, new Vector2(0f, 0f)),
                new GpuTexVertex(new Vector3( 3f,  1f, 0f), GpuColor.White, new Vector2(2f, 0f)),
                new GpuTexVertex(new Vector3(-1f, -3f, 0f), GpuColor.White, new Vector2(0f, 2f))
            )
        };

        private static IEffect LoadEmbeddedEffect(IEffectLoader loader, string resourceName)
        {
            var fullName = $"Barebone.Game.Monogame.Shaders.{resourceName}.mgfxo";
            using var stream = typeof(MonoGameGraphics).Assembly.GetManifestResourceStream(fullName)
                               ?? throw new MonoGameException(
                                   $"Embedded shader '{fullName}' not found. " +
                                   "Run Shaders/build_shaders.cmd to generate the .mgfxo files first.");
            return loader.Load(stream);
        }

        public void ClearScreen(in Color color)
        {
            _clearColor = color;
            _cpuTriangleCount = 0;
            _batches.Clear();
        }

        public void SetTransform(in Matrix3x2 worldTransform, in Matrix3x2 cameraTransform)
        {
            _currentWorld = worldTransform;
            _currentCamera = cameraTransform;
        }

        public void FillTriangles(in ReadOnlySpan<Vertex> vertices, ITexture? texture, in float z)
        {
            if (vertices.Length == 0) return;
            if (vertices.Length % 3 != 0) throw new ArgumentException("FillTriangles expects a triangle list (vertices.Length % 3 == 0).");

            var triCount = vertices.Length / 3;
            EnsureCpuTriangleCapacity(_cpuTriangleCount + triCount);

            var startTri = _cpuTriangleCount;
            for (var t = 0; t < triCount; t++)
            {
                var i = t * 3;
                _cpuTriangles[_cpuTriangleCount + t] = new GpuTexTriangle(
                    ToGpuVertex(vertices[i + 0]),
                    ToGpuVertex(vertices[i + 1]),
                    ToGpuVertex(vertices[i + 2])
                );
            }
            _cpuTriangleCount += triCount;

            // Fold the user's (worldTransform * cameraTransform) into a single Matrix4x4 with translation.Z = zLayer.
            // The renderer's BasicEffect then does world * View(I) * Projection(ortho) per vertex; ortho znear/zfar = 0..1
            // means zLayer should sit in [0..1] just like in the previous SpriteEffect path.
            var combined = _currentWorld * _currentCamera;
            _batches.Add(new DrawBatch(
                FirstTriangle: startTri,
                TriangleCount: triCount,
                World: ToMatrix4x4(combined, z),
                Texture: texture
            ));
        }

        public ITexture GetTexture(string assetPath)
        {
            if (!_textureCache.TryGetValue(assetPath, out var t))
                _textureCache.Add(assetPath, t = textureLoader.LoadTexture(assetPath));
            return t;
        }

        internal void RenderFrame()
        {
            var bb = renderer.Viewport.Size;
            EnsureRenderTargets(bb);

            DrawScenePass();
            BloomPass();
            ComposePass();

            _cpuTriangleCount = 0;
            _batches.Clear();
        }

        private void DrawScenePass()
        {
            _sceneCamera.BackbufferSize = _backbufferSize; // projection ignores the (2x) GPU viewport, ortho stays at 1x

            renderer.SetRenderTarget(_scene2X!.Texture);
            renderer.ClearScreen(_clearColor);

            if (_batches.Count == 0) return;

            renderer.Begin(_sceneCamera,
                enableDepthBuffer: true,
                additiveBlend: false,
                cullCounterClockwise: false,
                linearSampling: true);

            foreach (var batch in _batches.AsReadOnlySpan())
            {
                ReadOnlySpan<GpuTexTriangle> tris = _cpuTriangles.AsSpan(batch.FirstTriangle, batch.TriangleCount);
                renderer.Draw(batch.World, tris, batch.Texture);
            }

            renderer.End();
        }

        private void BloomPass()
        {
            // 1) Bright pass: scene2x (with bilinear sampling = SSAA reduce) -> bloom_mips[0]
            renderer.SetRenderTarget(_bloomMips[0].Texture);
            renderer.ClearScreen(TransparentBlack);
            renderer.Begin(_ndcCamera, enableDepthBuffer: false, additiveBlend: false, cullCounterClockwise: false, linearSampling: true);

            _brightEffect.SetTexture("Scene", _scene2X!.Texture);
            _brightEffect.SetFloat("Threshold", BloomThreshold);
            _brightEffect.SetFloat("SoftKnee", BloomSoftKnee);
            _brightEffect.SetFloat("Intensity", BloomBrightIntensity);
            renderer.SetEffect(_brightEffect, BlendMode.Opaque);
            DrawFullscreenTri();

            renderer.End();

            // 2) Downsample chain: mips[i] -> mips[i+1]
            for (var i = 0; i < BloomMipCount - 1; i++)
            {
                renderer.SetRenderTarget(_bloomMips[i + 1].Texture);
                renderer.ClearScreen(TransparentBlack);
                renderer.Begin(_ndcCamera, enableDepthBuffer: false, additiveBlend: false, cullCounterClockwise: false, linearSampling: true);

                var srcSize = _bloomMipSizes[i];
                _downsampleEffect.SetTexture("Source", _bloomMips[i].Texture);
                _downsampleEffect.SetVector2("SrcTexelSize", new Vector2(1f / srcSize.X, 1f / srcSize.Y));
                renderer.SetEffect(_downsampleEffect, BlendMode.Opaque);
                DrawFullscreenTri();

                renderer.End();
            }

            // 3) Upsample chain (additive, One+One): mips[i+1] -> mips[i]
            // We don't clear — we want to ADD to the downsample result already in the mip.
            for (var i = BloomMipCount - 2; i >= 0; i--)
            {
                renderer.SetRenderTarget(_bloomMips[i].Texture);
                renderer.Begin(_ndcCamera, enableDepthBuffer: false, additiveBlend: false, cullCounterClockwise: false, linearSampling: true);

                var srcSize = _bloomMipSizes[i + 1];
                _upsampleEffect.SetTexture("Source", _bloomMips[i + 1].Texture);
                _upsampleEffect.SetVector2("SrcTexelSize", new Vector2(1f / srcSize.X, 1f / srcSize.Y));
                _upsampleEffect.SetFloat("Strength", BloomUpsampleStrength);
                renderer.SetEffect(_upsampleEffect, BlendMode.AdditiveOneOne);
                DrawFullscreenTri();

                renderer.End();
            }
        }

        /// <summary>Final composite: scene2x bilinear (SSAA resolve) + bloom_mip[0] -> backbuffer.</summary>
        private void ComposePass()
        {
            renderer.ResetRenderTargetToScreen();
            renderer.ClearScreen(Color.Black);
            renderer.Begin(_ndcCamera, enableDepthBuffer: false, additiveBlend: false, cullCounterClockwise: false, linearSampling: true);

            _compositeEffect.SetTexture("Scene", _scene2X!.Texture);
            _compositeEffect.SetTexture("Bloom", _bloomMips[0].Texture);
            _compositeEffect.SetFloat("BloomIntensity", BloomFinalIntensity);
            renderer.SetEffect(_compositeEffect, BlendMode.Opaque);
            DrawFullscreenTri();

            renderer.End();
        }

        private void DrawFullscreenTri()
        {
            // The active custom effect (set via SetEffect) drives its own textures, so the texture arg is unused here.
            renderer.Draw(Matrix4x4.Identity, FullscreenTriangle.AsSpan(), texture: null);
        }

        private void EnsureRenderTargets(Vector2I backbufferSize)
        {
            if (_backbufferSize == backbufferSize && _scene2X != null) return;
            DestroyRenderTargets();
            _backbufferSize = backbufferSize;
            _scene2XSize = new Vector2I(backbufferSize.X * SsaaScale, backbufferSize.Y * SsaaScale);

            // SSAA scene render target — depth buffer needed for the zLayer ordering during the scene pass.
            _scene2X = renderer.CreateRenderTargetSprite(_scene2XSize, supportDepthBuffer: true);

            // Bloom mips: mip[0] at backbuffer-size, halving from there.
            // PreserveContents because the upsample chain reads back what the previous pass wrote and adds onto it.
            _bloomMips = new Sprite[BloomMipCount];
            _bloomMipSizes = new Vector2I[BloomMipCount];
            int mw = backbufferSize.X, mh = backbufferSize.Y;
            for (var i = 0; i < BloomMipCount; i++)
            {
                mw = Math.Max(1, mw);
                mh = Math.Max(1, mh);
                _bloomMipSizes[i] = new Vector2I(mw, mh);
                _bloomMips[i] = renderer.CreateRenderTargetSprite(new Vector2I(mw, mh), supportDepthBuffer: false, preserveContents: true);
                mw = Math.Max(1, mw / 2);
                mh = Math.Max(1, mh / 2);
            }
        }

        private void DestroyRenderTargets()
        {
            _scene2X?.Dispose();
            _scene2X = null;
            foreach (var s in _bloomMips) s.Dispose();
            _bloomMips = [];
            _bloomMipSizes = [];
        }

        private void EnsureCpuTriangleCapacity(int required)
        {
            if (_cpuTriangles.Length >= required) return;
            var newCap = Math.Max(required, _cpuTriangles.Length * 2);
            Array.Resize(ref _cpuTriangles, newCap);
        }

        private static GpuTexVertex ToGpuVertex(in Vertex v)
        {
            return new GpuTexVertex(new Vector3(v.Position.X, v.Position.Y, 0f), ToGpuColor(v.Color), v.UV);
        }

        private static GpuColor ToGpuColor(in ColorF c)
        {
            // 4-byte packed RGBA, matching MonoGame Color memory layout (also what GpuColor.White uses).
            var r = (byte)Math.Clamp((int)(c.R * 255f), 0, 255);
            var g = (byte)Math.Clamp((int)(c.G * 255f), 0, 255);
            var b = (byte)Math.Clamp((int)(c.B * 255f), 0, 255);
            var a = (byte)Math.Clamp((int)(c.A * 255f), 0, 255);
            return new GpuColor((uint)(r | (g << 8) | (b << 16) | (a << 24)));
        }

        private static Matrix4x4 ToMatrix4x4(in Matrix3x2 m, float z)
        {
            // Same layout as XnaConversionExtensions.ToXna(Matrix3x2) but with translation.Z = z so the per-call zLayer
            // ends up in vertex.z after world transform. Matches the original SpriteEffect-based behavior.
            return new Matrix4x4(
                m.M11, m.M12, 0, 0,
                m.M21, m.M22, 0, 0,
                0,     0,     1, 0,
                m.M31, m.M32, z, 1);
        }

        public void Dispose()
        {
            DestroyRenderTargets();
            foreach (var t in _textureCache.Values)
                (t as IDisposable)?.Dispose();
            _textureCache.Clear();

            (_brightEffect as IDisposable)?.Dispose();
            (_downsampleEffect as IDisposable)?.Dispose();
            (_upsampleEffect as IDisposable)?.Dispose();
            (_compositeEffect as IDisposable)?.Dispose();
        }

        private record struct DrawBatch(int FirstTriangle, int TriangleCount, Matrix4x4 World, ITexture? Texture);

        /// <summary>
        /// Scene-pass camera adapter. View = identity (per-draw world matrices already fold in the user's camera).
        /// Projection = orthographic over the 1x backbuffer (the GPU viewport is 2x; rasterizing 1x-pixel-space output
        /// at 2x is the supersampling).
        /// </summary>
        private class Scene2DCamera : ICamera
        {
            public Vector2I BackbufferSize { get; set; }
            public Matrix4x4 GetViewTransform() => Matrix4x4.CreateTranslation(0, 0, -1);
            public Matrix4x4 GetProjectionTransform(in Viewport viewport)
                => Matrix4x4.CreateOrthographicOffCenter(0, BackbufferSize.X, BackbufferSize.Y, 0, 0, 1); // must support our Z of [1,0]
        }

        /// <summary>
        /// Identity camera for fullscreen post-process passes. The custom shaders pass Position straight through
        /// to clip space, so View/Projection are irrelevant — but Begin() still needs an ICamera.
        /// </summary>
        private class NullCamera : ICamera
        {
            public Matrix4x4 GetViewTransform() => Matrix4x4.Identity;
            public Matrix4x4 GetProjectionTransform(in Viewport viewport) => Matrix4x4.Identity;
        }
    }
}
