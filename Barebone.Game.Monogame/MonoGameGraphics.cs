using System.Numerics;
using System.Reflection;
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
        // -------- Tunables --------
        private const int BloomMipCount = 5;
        private const int SsaaScale = 2;

        public float BloomThreshold { get; set; } = 0.9f; // bloom only when Max(R, G, B) > BloomTreshold.
        public float BloomSoftKnee { get; set; } = 0.5f; // Width of the fade edge of the bloom. So less is sharp edges, more is slower fadeout
        public float BloomBrightIntensity { get; set; } = 1.0f; // mostly irrelevant in non HDR. Use FinalIntensity
        public float BloomUpsampleStrength { get; set; } = 1.0f;
        public float BloomFinalIntensity { get; set; } = 0.5f;

        // -------- GraphicsDevice --------
        private readonly GraphicsDevice _gd;

        // -------- Effects --------
        private readonly Effect _spriteEffect;
        private readonly Effect _brightEffect;
        private readonly Effect _downsampleEffect;
        private readonly Effect _upsampleEffect;
        private readonly Effect _compositeEffect;

        // Effect parameters cached so we don't pay the dictionary lookup every draw.
        private readonly EffectParameter _spriteWorld;
        private readonly EffectParameter _spriteView;
        private readonly EffectParameter _spriteProjection;
        private readonly EffectParameter _spriteTex;

        private readonly EffectParameter _brightScene;
        private readonly EffectParameter _brightThreshold;
        private readonly EffectParameter _brightSoftKnee;
        private readonly EffectParameter _brightIntensity;

        private readonly EffectParameter _downsampleSource;
        private readonly EffectParameter _downsampleSrcTexel;

        private readonly EffectParameter _upsampleSource;
        private readonly EffectParameter _upsampleSrcTexel;
        private readonly EffectParameter _upsampleStrength;

        private readonly EffectParameter _compositeScene;
        private readonly EffectParameter _compositeBloom;
        private readonly EffectParameter _compositeBloomIntensity;

        // -------- White texture (for color-only draws) --------
        private readonly Texture2D _whiteTexture;

        // -------- Vertex buffer + draw queue --------
        private DynamicVertexBuffer? _vertexBuffer;
        private int _vertexCapacity;
        private Vertex[] _cpuVertices = new Vertex[4096];
        private int _cpuVertexCount;
        private readonly List<DrawItem> _drawItems = new(256);

        // -------- Render targets --------
        private Vector2I _backbufferSize;
        private Vector2I _scene2xSize;
        private RenderTarget2D? _scene2x;
        private RenderTarget2D[] _bloomMips = Array.Empty<RenderTarget2D>();
        private Vector2I[] _bloomMipSizes = Array.Empty<Vector2I>();

        // -------- Texture cache --------
        private readonly Dictionary<string, MonoGameTexture> _textureCache = new();

        // -------- Frame state --------
        private XnaColor _clearColor = XnaColor.Black;
        private Matrix3x2 _currentWorld = Matrix3x2.Identity;
        private Matrix3x2 _currentCamera = Matrix3x2.Identity;

        // -------- Static fullscreen triangle (no vertex buffer needed; uses DrawUserPrimitives) --------
        private readonly static VertexPositionTexture[] _fullscreenTri =
        {
            new(new XnaVector3(-1f,  1f, 0f), new XnaVector2(0f, 0f)),
            new(new XnaVector3( 3f,  1f, 0f), new XnaVector2(2f, 0f)),
            new(new XnaVector3(-1f, -3f, 0f), new XnaVector2(0f, 2f)),
        };

        // -------- BlendState used for additive bloom upsample --------
        private readonly static BlendState AdditiveBlend = new()
        {
            ColorSourceBlend = Blend.One,
            ColorDestinationBlend = Blend.One,
            ColorBlendFunction = BlendFunction.Add,
            AlphaSourceBlend = Blend.One,
            AlphaDestinationBlend = Blend.One,
            AlphaBlendFunction = BlendFunction.Add,
        };

        // ------------------------------------------------------------------------------------------------

        public MonoGameGraphics(GraphicsDevice gd)
        {
            _gd = gd;

            _spriteEffect    = LoadEffect("Sprite");
            _brightEffect    = LoadEffect("BrightPass");
            _downsampleEffect = LoadEffect("Downsample");
            _upsampleEffect  = LoadEffect("Upsample");
            _compositeEffect = LoadEffect("Composite");

            _spriteWorld      = _spriteEffect.Parameters["World"];
            _spriteView       = _spriteEffect.Parameters["View"];
            _spriteProjection = _spriteEffect.Parameters["Projection"];
            _spriteTex        = _spriteEffect.Parameters["SpriteTex"];

            _brightScene     = _brightEffect.Parameters["Scene"];
            _brightThreshold = _brightEffect.Parameters["Threshold"];
            _brightSoftKnee  = _brightEffect.Parameters["SoftKnee"];
            _brightIntensity = _brightEffect.Parameters["Intensity"];

            _downsampleSource   = _downsampleEffect.Parameters["Source"];
            _downsampleSrcTexel = _downsampleEffect.Parameters["SrcTexelSize"];

            _upsampleSource   = _upsampleEffect.Parameters["Source"];
            _upsampleSrcTexel = _upsampleEffect.Parameters["SrcTexelSize"];
            _upsampleStrength = _upsampleEffect.Parameters["Strength"];

            _compositeScene          = _compositeEffect.Parameters["Scene"];
            _compositeBloom          = _compositeEffect.Parameters["Bloom"];
            _compositeBloomIntensity = _compositeEffect.Parameters["BloomIntensity"];

            _whiteTexture = new Texture2D(gd, 1, 1, false, SurfaceFormat.Color);
            _whiteTexture.SetData(new[] { XnaColor.White });

            EnsureVertexBuffer(4096);
        }

        private struct DrawItem
        {
            public int FirstVertex;
            public int VertexCount;
            public Texture2D? Texture;
            public Matrix3x2 World;
            public Matrix3x2 Camera;
        }

        // ============================================================================
        // IPlatformGraphics
        // ============================================================================

        public void ClearScreen(in ScreenColor color)
        {
            _clearColor = new XnaColor(color.R, color.G, color.B, color.A);
            _cpuVertexCount = 0;
            _drawItems.Clear();
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

            _drawItems.Add(new DrawItem
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

        // ============================================================================
        // Frame rendering — invoked from MonoGamePlatform.Present BEFORE GraphicsDevice.Present
        // ============================================================================

        internal void RenderFrame()
        {
            var bbW = _gd.PresentationParameters.BackBufferWidth;
            var bbH = _gd.PresentationParameters.BackBufferHeight;
            EnsureRenderTargets(new Vector2I(bbW, bbH));

            // Upload all the frame's vertices in one shot.
            if (_cpuVertexCount > 0)
            {
                EnsureVertexBuffer(_cpuVertexCount);
                _vertexBuffer!.SetData(_cpuVertices, 0, _cpuVertexCount, SetDataOptions.Discard);
            }

            DrawScene(bbW, bbH);
            RunBloom();
            RunComposite();

            _cpuVertexCount = 0;
            _drawItems.Clear();
        }

        // ----------------------------------------------------------------------------
        // Scene pass
        // ----------------------------------------------------------------------------
        private void DrawScene(int backbufferWidth, int backbufferHeight)
        {
            _gd.SetRenderTarget(_scene2x);
            _gd.Clear(_clearColor);

            if (_drawItems.Count == 0) return;

            _gd.BlendState = BlendState.NonPremultiplied;
            _gd.DepthStencilState = DepthStencilState.None;
            _gd.RasterizerState = RasterizerState.CullNone;
            _gd.SamplerStates[0] = SamplerState.LinearClamp;

            _gd.SetVertexBuffer(_vertexBuffer);

            // Projection: pixel coords (0..backbufferWidth, 0..backbufferHeight) -> NDC.
            // The viewport is automatically the 2x render target's bounds, so the rasterizer naturally
            // upscales the 1x-pixel-space output to 2x — that IS the supersampling.
            var projection = XnaMatrix.CreateOrthographicOffCenter(0, backbufferWidth, backbufferHeight, 0, 0, 1);
            _spriteProjection.SetValue(projection);

            for (var i = 0; i < _drawItems.Count; i++)
            {
                ref var di = ref CollectionsMarshal.AsSpan(_drawItems)[i];
                _spriteWorld.SetValue(LiftMatrix(di.World));
                _spriteView.SetValue(LiftMatrix(di.Camera));
                _spriteTex.SetValue(di.Texture ?? _whiteTexture);
                _spriteEffect.CurrentTechnique.Passes[0].Apply();

                _gd.DrawPrimitives(PrimitiveType.TriangleList, di.FirstVertex, di.VertexCount / 3);
            }
        }

        // ----------------------------------------------------------------------------
        // Bloom mip-chain
        // ----------------------------------------------------------------------------
        private void RunBloom()
        {
            // 1) Bright pass: scene2x (with bilinear sampling = SSAA reduce) -> bloom_mips[0]
            _gd.SetRenderTarget(_bloomMips[0]);
            _gd.Clear(XnaColor.Transparent);
            _gd.BlendState = BlendState.Opaque;

            _brightScene.SetValue(_scene2x);
            _brightThreshold.SetValue(BloomThreshold);
            _brightSoftKnee.SetValue(BloomSoftKnee);
            _brightIntensity.SetValue(BloomBrightIntensity);
            _brightEffect.CurrentTechnique.Passes[0].Apply();
            _gd.DrawUserPrimitives(PrimitiveType.TriangleList, _fullscreenTri, 0, 1);

            // 2) Downsample chain: mips[i] -> mips[i+1]
            _gd.BlendState = BlendState.Opaque;
            for (int i = 0; i < BloomMipCount - 1; i++)
            {
                _gd.SetRenderTarget(_bloomMips[i + 1]);
                _gd.Clear(XnaColor.Transparent);

                var srcSize = _bloomMipSizes[i];
                _downsampleSource.SetValue(_bloomMips[i]);
                _downsampleSrcTexel.SetValue(new XnaVector2(1f / srcSize.X, 1f / srcSize.Y));
                _downsampleEffect.CurrentTechnique.Passes[0].Apply();
                _gd.DrawUserPrimitives(PrimitiveType.TriangleList, _fullscreenTri, 0, 1);
            }

            // 3) Upsample chain (additive): mips[i+1] -> mips[i]
            _gd.BlendState = AdditiveBlend;
            for (int i = BloomMipCount - 2; i >= 0; i--)
            {
                _gd.SetRenderTarget(_bloomMips[i]);
                // Don't clear — we want to ADD to whatever was previously written.

                var srcSize = _bloomMipSizes[i + 1];
                _upsampleSource.SetValue(_bloomMips[i + 1]);
                _upsampleSrcTexel.SetValue(new XnaVector2(1f / srcSize.X, 1f / srcSize.Y));
                _upsampleStrength.SetValue(BloomUpsampleStrength);
                _upsampleEffect.CurrentTechnique.Passes[0].Apply();
                _gd.DrawUserPrimitives(PrimitiveType.TriangleList, _fullscreenTri, 0, 1);
            }
        }

        // ----------------------------------------------------------------------------
        // Final composite: scene2x bilinear (SSAA resolve) + bloom_mip[0] -> backbuffer
        // ----------------------------------------------------------------------------
        private void RunComposite()
        {
            _gd.SetRenderTarget(null); // backbuffer
            _gd.Clear(XnaColor.Black);
            _gd.BlendState = BlendState.Opaque;

            _compositeScene.SetValue(_scene2x);
            _compositeBloom.SetValue(_bloomMips[0]);
            _compositeBloomIntensity.SetValue(BloomFinalIntensity);
            _compositeEffect.CurrentTechnique.Passes[0].Apply();

            _gd.DrawUserPrimitives(PrimitiveType.TriangleList, _fullscreenTri, 0, 1);
        }

        // ============================================================================
        // RenderTarget lifecycle
        // ============================================================================
        private void EnsureRenderTargets(Vector2I backbufferSize)
        {
            if (_backbufferSize == backbufferSize && _scene2x != null) return;
            DestroyRenderTargets();
            _backbufferSize = backbufferSize;
            _scene2xSize = new Vector2I(backbufferSize.X * SsaaScale, backbufferSize.Y * SsaaScale);

            // SSAA scene render target
            _scene2x = new RenderTarget2D(
                _gd,
                _scene2xSize.X, _scene2xSize.Y,
                mipMap: false,
                preferredFormat: SurfaceFormat.Color,
                preferredDepthFormat: DepthFormat.None,
                preferredMultiSampleCount: 0,
                usage: RenderTargetUsage.DiscardContents);

            // Bloom mips: mip[0] at backbuffer-size, halving from there.
            _bloomMips = new RenderTarget2D[BloomMipCount];
            _bloomMipSizes = new Vector2I[BloomMipCount];
            int mw = backbufferSize.X, mh = backbufferSize.Y;
            for (int i = 0; i < BloomMipCount; i++)
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
            _scene2x?.Dispose();
            _scene2x = null;
            foreach (var rt in _bloomMips) rt?.Dispose();
            _bloomMips = Array.Empty<RenderTarget2D>();
            _bloomMipSizes = Array.Empty<Vector2I>();
        }

        // ============================================================================
        // Vertex buffer
        // ============================================================================
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

        // Layout matches Barebone.Graphics.Vertex byte-for-byte:
        //   Position : Vector2 (8 bytes)
        //   Color    : ColorF=4 floats (16 bytes)
        //   UV       : Vector2 (8 bytes)
        public static readonly VertexDeclaration BareboneVertexDeclaration = new(
            new VertexElement(0,  VertexElementFormat.Vector2, VertexElementUsage.Position, 0),
            new VertexElement(8,  VertexElementFormat.Vector4, VertexElementUsage.Color, 0),
            new VertexElement(24, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0)
        );

        // ============================================================================
        // Texture loading
        // ============================================================================
        private MonoGameTexture LoadTexture(string assetPath)
        {
            using var stream = File.OpenRead(assetPath);
            var tex = Texture2D.FromStream(_gd, stream);
            return new MonoGameTexture(tex);
        }

        // ============================================================================
        // Effect loading (from embedded resource)
        // ============================================================================
        private Effect LoadEffect(string baseName)
        {
            var resourceName = $"Barebone.Game.Monogame.Shaders.{baseName}.mgfxo";
            using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName)
                               ?? throw new MonoGameException(
                                   $"Embedded shader '{resourceName}' not found. " +
                                   "Run mgfxc Shaders/*.fx to generate the .mgfxo files first.");
            using var ms = new MemoryStream();
            stream.CopyTo(ms);
            return new Effect(_gd, ms.ToArray());
        }

        // ============================================================================
        // Helpers
        // ============================================================================
        private static XnaMatrix LiftMatrix(in Matrix3x2 m)
        {
            // Matrix3x2:           Lifted to row-major Matrix (XNA's convention is row-vector * matrix):
            // | M11 M12 |          | M11 M12 0 0 |
            // | M21 M22 |  ==>     | M21 M22 0 0 |
            // | M31 M32 |          | 0   0   1 0 |
            //                      | M31 M32 0 1 |
            return new XnaMatrix(
                m.M11, m.M12, 0, 0,
                m.M21, m.M22, 0, 0,
                0,     0,     1, 0,
                m.M31, m.M32, 0, 1);
        }

        // ============================================================================
        // Dispose
        // ============================================================================
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
