using System.Numerics;
using System.Runtime.InteropServices;
using Barebone.Game.Graphics;
using Barebone.Geometry;
using Barebone.Graphics;
using Microsoft.Xna.Framework.Graphics;
using XnaColor = Microsoft.Xna.Framework.Color;
using XnaMatrix = Microsoft.Xna.Framework.Matrix;
using XnaVector2 = Microsoft.Xna.Framework.Vector2;
using XnaVector3 = Microsoft.Xna.Framework.Vector3;
using ScreenColor = System.Drawing.Color;

namespace Barebone.Game.Monogame
{
    /// <summary>
    /// MonoGame-based <see cref="IPlatformGraphics"/> with manual SSAA antialiasing and mip-chain bloom.
    /// Draws are buffered during the frame and replayed in <see cref="RenderFrame"/>:
    ///  - Scene -> 2x-size <c>scene2x</c> RenderTarget (the SSAA target)
    ///  - Bright pass with bilinear downsample -> bloom mip[0] (also acts as the SSAA resolve into bloom)
    ///  - 4 downsample passes (mip[0] -> mip[1] -> ... -> mip[4])
    ///  - 4 additive upsample passes (mip[4] -> mip[3] -> ... -> mip[0])
    ///  - Composite: scene2x bilinear (SSAA resolve) + bloom mip[0] -> backbuffer
    /// </summary>
    public class MonoGameGraphics : IPlatformGraphics, IDisposable
    {
        private const int BloomMipCount = 5;
        private const int SsaaScale = 2;

        public float BloomThreshold { get; set; } = 0.9f; // bloom only when Max(R, G, B) > BloomTreshold.
        public float BloomSoftKnee { get; set; } = 0.5f; // Width of the fade edge of the bloom. So less is sharp edges, more is slower fadeout
        public float BloomBrightIntensity { get; set; } = 1.0f; // mostly irrelevant in non HDR. Use FinalIntensity
        public float BloomUpsampleStrength { get; set; } = 1.0f;
        public float BloomFinalIntensity { get; set; } = 0.5f;

        private readonly GraphicsDevice _gd;

        private readonly SpriteEffect _spriteEffect;
        private readonly BrightEffect _brightEffect;
        private readonly DownsampleEffect _downsampleEffect;
        private readonly UpsampleEffect _upsampleEffect;
        private readonly CompositeEffect _compositeEffect;

        private readonly Texture2D _whiteTexture;

        private DynamicVertexBuffer? _vertexBuffer;
        private int _vertexCapacity;
        private Vertex[] _cpuVertices = new Vertex[4096];
        private int _cpuVertexCount;
        private readonly List<DrawCommand> _drawCommandQueue = new(256);

        private Vector2I _backbufferSize;
        private Vector2I _scene2XSize;
        private RenderTarget2D? _scene2X;
        private RenderTarget2D[] _bloomMips = [];
        private Vector2I[] _bloomMipSizes = [];

        private readonly Dictionary<string, MonoGameTexture> _textureCache = new();

        private XnaColor _clearColor = XnaColor.Black;
        private Matrix3x2 _currentWorld = Matrix3x2.Identity;
        private Matrix3x2 _currentCamera = Matrix3x2.Identity;

        private readonly static VertexPositionTexture[] FullscreenTriangle =
        [
            new(new XnaVector3(-1f,  1f, 0f), new XnaVector2(0f, 0f)),
            new(new XnaVector3( 3f,  1f, 0f), new XnaVector2(2f, 0f)),
            new(new XnaVector3(-1f, -3f, 0f), new XnaVector2(0f, 2f))
        ];

        private readonly static BlendState AdditiveBlendForBloom = new()
        {
            ColorSourceBlend = Blend.One,
            ColorDestinationBlend = Blend.One,
            ColorBlendFunction = BlendFunction.Add,
            AlphaSourceBlend = Blend.One,
            AlphaDestinationBlend = Blend.One,
            AlphaBlendFunction = BlendFunction.Add,
        };

        public MonoGameGraphics(GraphicsDevice gd)
        {
            _gd = gd;

            _spriteEffect    = new SpriteEffect(gd);
            _brightEffect    = new BrightEffect(gd);
            _downsampleEffect = new DownsampleEffect(gd);
            _upsampleEffect = new UpsampleEffect(gd);
            _compositeEffect = new CompositeEffect(gd);

            _whiteTexture = new Texture2D(gd, 1, 1, false, SurfaceFormat.Color);
            _whiteTexture.SetData([XnaColor.White]);

            EnsureVertexBuffer(4096);
        }

        private struct DrawCommand
        {
            public int FirstVertex;
            public int VertexCount;
            public Texture2D? Texture;
            public Matrix3x2 World;
            public Matrix3x2 Camera;
        }
        
        public void ClearScreen(in ScreenColor color)
        {
            _clearColor = new XnaColor(color.R, color.G, color.B, color.A);
            _cpuVertexCount = 0;
            _drawCommandQueue.Clear();
        }

        public void SetTransform(in Matrix3x2 worldTransform, in Matrix3x2 cameraTransform)
        {
            _currentWorld = worldTransform;
            _currentCamera = cameraTransform;
        }

        public void FillTriangles(in ReadOnlySpan<Vertex> vertices, ITexture? texture)
        {
            if (vertices.Length == 0) return;

            EnsureCpuVertexCapacity(_cpuVertexCount + vertices.Length);
            vertices.CopyTo(_cpuVertices.AsSpan(_cpuVertexCount));

            _drawCommandQueue.Add(new DrawCommand
            {
                FirstVertex = _cpuVertexCount,
                VertexCount = vertices.Length,
                Texture = (texture as MonoGameTexture)?.Texture,
                World = _currentWorld,
                Camera = _currentCamera,
            });

            _cpuVertexCount += vertices.Length;
        }

        public ITexture GetTexture(string assetPath)
        {
            if (!_textureCache.TryGetValue(assetPath, out var t))
                _textureCache.Add(assetPath, t = LoadTexture(assetPath));
            return t;
        }

        internal void RenderFrame()
        {
            var bbW = _gd.PresentationParameters.BackBufferWidth;
            var bbH = _gd.PresentationParameters.BackBufferHeight;
            EnsureRenderTargets(new Vector2I(bbW, bbH));

            if (_cpuVertexCount > 0)
            {
                EnsureVertexBuffer(_cpuVertexCount);
                _vertexBuffer!.SetData(_cpuVertices, 0, _cpuVertexCount, SetDataOptions.Discard);
            }

            DrawScenePass(bbW, bbH);
            BloomPass();
            ComposePass();

            _cpuVertexCount = 0;
            _drawCommandQueue.Clear();
        }

        private void DrawScenePass(int backbufferWidth, int backbufferHeight)
        {
            _gd.SetRenderTarget(_scene2X);
            _gd.Clear(_clearColor);

            if (_drawCommandQueue.Count == 0) return;

            _gd.BlendState = BlendState.NonPremultiplied;
            _gd.DepthStencilState = DepthStencilState.None;
            _gd.RasterizerState = RasterizerState.CullNone;
            _gd.SamplerStates[0] = SamplerState.LinearClamp;

            _gd.SetVertexBuffer(_vertexBuffer);

            // Projection: pixel coords (0..backbufferWidth, 0..backbufferHeight) -> NDC.
            // The viewport is automatically the 2x render target's bounds, so the rasterizer naturally
            // upscales the 1x-pixel-space output to 2x — that IS the supersampling.
            var projection = XnaMatrix.CreateOrthographicOffCenter(0, backbufferWidth, backbufferHeight, 0, 0, 1);
            _spriteEffect.Projection.SetValue(projection);

            for (var i = 0; i < _drawCommandQueue.Count; i++)
            {
                ref var di = ref CollectionsMarshal.AsSpan(_drawCommandQueue)[i];
                _spriteEffect.World.SetValue(ToXnaMatrix(di.World));
                _spriteEffect.View.SetValue(ToXnaMatrix(di.Camera));
                _spriteEffect.Texture.SetValue(di.Texture ?? _whiteTexture);
                _spriteEffect.Apply();

                _gd.DrawPrimitives(PrimitiveType.TriangleList, di.FirstVertex, di.VertexCount / 3);
            }
        }

        private void BloomPass()
        {
            // 1) Bright pass: scene2x (with bilinear sampling = SSAA reduce) -> bloom_mips[0]
            _gd.SetRenderTarget(_bloomMips[0]);
            _gd.Clear(XnaColor.Transparent);
            _gd.BlendState = BlendState.Opaque;

            _brightEffect.Scene.SetValue(_scene2X);
            _brightEffect.Threshold.SetValue(BloomThreshold);
            _brightEffect.SoftKnee.SetValue(BloomSoftKnee);
            _brightEffect.Intensity.SetValue(BloomBrightIntensity);
            _brightEffect.Apply();
            _gd.DrawUserPrimitives(PrimitiveType.TriangleList, FullscreenTriangle, 0, 1);

            // 2) Downsample chain: mips[i] -> mips[i+1]
            _gd.BlendState = BlendState.Opaque;
            for (int i = 0; i < BloomMipCount - 1; i++)
            {
                _gd.SetRenderTarget(_bloomMips[i + 1]);
                _gd.Clear(XnaColor.Transparent);

                var srcSize = _bloomMipSizes[i];
                _downsampleEffect.Source.SetValue(_bloomMips[i]);
                _downsampleEffect.SrcTexel.SetValue(new XnaVector2(1f / srcSize.X, 1f / srcSize.Y));
                _downsampleEffect.Apply();
                _gd.DrawUserPrimitives(PrimitiveType.TriangleList, FullscreenTriangle, 0, 1);
            }

            // 3) Upsample chain (additive): mips[i+1] -> mips[i]
            _gd.BlendState = AdditiveBlendForBloom;
            for (int i = BloomMipCount - 2; i >= 0; i--)
            {
                _gd.SetRenderTarget(_bloomMips[i]);
                // Don't clear — we want to ADD to whatever was previously written.

                var srcSize = _bloomMipSizes[i + 1];
                _upsampleEffect.Source.SetValue(_bloomMips[i + 1]);
                _upsampleEffect.SrcTexel.SetValue(new XnaVector2(1f / srcSize.X, 1f / srcSize.Y));
                _upsampleEffect.Strength.SetValue(BloomUpsampleStrength);
                _upsampleEffect.Apply();
                _gd.DrawUserPrimitives(PrimitiveType.TriangleList, FullscreenTriangle, 0, 1);
            }
        }

        /// <summary>
        /// Final composite: scene2x bilinear (SSAA resolve) + bloom_mip[0] -> backbuffer
        /// </summary>
        private void ComposePass()
        {
            _gd.SetRenderTarget(null); // backbuffer
            _gd.Clear(XnaColor.Black);
            _gd.BlendState = BlendState.Opaque;

            _compositeEffect.Scene.SetValue(_scene2X);
            _compositeEffect.Bloom.SetValue(_bloomMips[0]);
            _compositeEffect.BloomIntensity.SetValue(BloomFinalIntensity);
            _compositeEffect.Apply();

            _gd.DrawUserPrimitives(PrimitiveType.TriangleList, FullscreenTriangle, 0, 1);
        }

        private void EnsureRenderTargets(Vector2I backbufferSize)
        {
            if (_backbufferSize == backbufferSize && _scene2X != null) return;
            DestroyRenderTargets();
            _backbufferSize = backbufferSize;
            _scene2XSize = new Vector2I(backbufferSize.X * SsaaScale, backbufferSize.Y * SsaaScale);

            // SSAA scene render target
            _scene2X = new RenderTarget2D(
                _gd,
                _scene2XSize.X, _scene2XSize.Y,
                mipMap: false,
                preferredFormat: SurfaceFormat.Color,
                preferredDepthFormat: DepthFormat.None,
                preferredMultiSampleCount: 0,
                usage: RenderTargetUsage.DiscardContents);

            // Bloom mips: mip[0] at backbuffer-size, halving from there.
            _bloomMips = new RenderTarget2D[BloomMipCount];
            _bloomMipSizes = new Vector2I[BloomMipCount];
            int mw = backbufferSize.X, mh = backbufferSize.Y;
            for (var i = 0; i < BloomMipCount; i++)
            {
                mw = Math.Max(1, mw);
                mh = Math.Max(1, mh);
                _bloomMipSizes[i] = new Vector2I(mw, mh);
                _bloomMips[i] = new RenderTarget2D(
                    _gd,
                    mw, mh,
                    mipMap: false,
                    preferredFormat: SurfaceFormat.Color,
                    preferredDepthFormat: DepthFormat.None,
                    preferredMultiSampleCount: 0,
                    usage: RenderTargetUsage.PreserveContents); // upsample chain reads back & adds
                mw = Math.Max(1, mw / 2);
                mh = Math.Max(1, mh / 2);
            }
        }

        private void DestroyRenderTargets()
        {
            _scene2X?.Dispose();
            _scene2X = null;
            foreach (var rt in _bloomMips) rt?.Dispose();
            _bloomMips = Array.Empty<RenderTarget2D>();
            _bloomMipSizes = Array.Empty<Vector2I>();
        }

        private void EnsureCpuVertexCapacity(int required)
        {
            if (_cpuVertices.Length >= required) return;
            var newCap = Math.Max(required, _cpuVertices.Length * 2);
            Array.Resize(ref _cpuVertices, newCap);
        }

        private void EnsureVertexBuffer(int requiredVertices)
        {
            if (_vertexBuffer != null && _vertexCapacity >= requiredVertices) return;
            _vertexBuffer?.Dispose();
            var newCap = Math.Max(requiredVertices, _vertexCapacity == 0 ? 4096 : _vertexCapacity * 2);
            _vertexBuffer = new DynamicVertexBuffer(_gd, BareboneVertexDeclaration, newCap, BufferUsage.WriteOnly);
            _vertexCapacity = newCap;
        }

        public readonly static VertexDeclaration BareboneVertexDeclaration = new(
            new VertexElement(0,  VertexElementFormat.Vector2, VertexElementUsage.Position, 0),
            new VertexElement(8,  VertexElementFormat.Vector4, VertexElementUsage.Color, 0),
            new VertexElement(24, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0)
        );

        private MonoGameTexture LoadTexture(string assetPath)
        {
            using var stream = File.OpenRead(assetPath);
            var tex = Texture2D.FromStream(_gd, stream);
            return new MonoGameTexture(tex);
        }

        /// <summary>
        /// Lifts the 2D matrix to the 4x4 which XNA needs.
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        private static XnaMatrix ToXnaMatrix(in Matrix3x2 m)
        {
            return new XnaMatrix(
                m.M11, m.M12, 0, 0,
                m.M21, m.M22, 0, 0,
                0,     0,     1, 0,
                m.M31, m.M32, 0, 1);
        }

        public void Dispose()
        {
            DestroyRenderTargets();
            foreach (var t in _textureCache.Values) t.Dispose();
            _textureCache.Clear();

            _whiteTexture.Dispose();
            _vertexBuffer?.Dispose();

            _spriteEffect.Dispose();
            _brightEffect.Dispose();
            _downsampleEffect.Dispose();
            _upsampleEffect.Dispose();
            _compositeEffect.Dispose();
        }
    }
}
