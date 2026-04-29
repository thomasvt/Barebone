using System.Numerics;
using Barebone.Geometry;
using Barebone.Graphics;
using Barebone.Graphics.Cameras;
using Barebone.Graphics.Gpu;
using Barebone.Graphics.Sprites;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Color = System.Drawing.Color;
using Vector2 = System.Numerics.Vector2;
using Vector4 = Microsoft.Xna.Framework.Vector4;
using Viewport = Barebone.Graphics.Viewport;

namespace Barebone.Monogame
{
    public class XnaImmediateRenderer(GraphicsDevice graphicsDevice, GraphicsDeviceManager gdm) : IDisposable, IImmediateRenderer
    {
        private readonly BasicEffect _effect = new(graphicsDevice)
        {
            VertexColorEnabled = true,
            LightingEnabled = false
        };

        private readonly RasterizerState _rasterizerStateScissorNoCull = new() { CullMode = CullMode.None, ScissorTestEnable = true };
        private readonly RasterizerState _rasterizerStateScissorCull = new() { CullMode = CullMode.CullCounterClockwiseFace, ScissorTestEnable = true };

        // Pure additive (One + One) — for HDR-style accumulation (e.g. bloom upsampling).
        private static readonly BlendState _blendAdditiveOneOne = new()
        {
            ColorSourceBlend = Blend.One,
            ColorDestinationBlend = Blend.One,
            ColorBlendFunction = BlendFunction.Add,
            AlphaSourceBlend = Blend.One,
            AlphaDestinationBlend = Blend.One,
            AlphaBlendFunction = BlendFunction.Add,
        };

        private ICamera? _camera;
        private readonly BBList<VertexPositionColor> _xnaVerticesBuffer = new();
        private readonly BBList<VertexPositionColorTexture> _xnaVerticesTexBuffer = new();
        private readonly BBList<AabbI> _clipStack = new();

        private bool _inRenderPass = false;

        // Custom effect override (set via SetEffect, cleared via ResetEffect / Begin / End).
        private XnaEffect? _currentEffect;
        private BlendMode _passBlendMode = BlendMode.NonPremultiplied;

        /// <summary>
        /// Clears screen, depth buffer and stencil buffer.
        /// </summary>
        public void ClearScreen(in Color color)
        {
            graphicsDevice.Clear(color.ToXna());
        }

        public void ClearDepthBuffer()
        {
            graphicsDevice.Clear(ClearOptions.DepthBuffer, Vector4.Zero, graphicsDevice.Viewport.MaxDepth, 0);
        }

        /// <summary>
        /// Configures how to render your next batch of Draw calls. This resets the Clip area and any custom effect.
        /// </summary>
        public void Begin(ICamera camera, bool enableDepthBuffer, bool additiveBlend, bool cullCounterClockwise, bool linearSampling = true)
        {
            if (_inRenderPass) throw new Exception("Cannot start a new renderpass: the previous one was not ended.");
            _inRenderPass = true;
            _clipStack.Clear();
            _camera = camera;

            _passBlendMode = additiveBlend ? BlendMode.Additive : BlendMode.NonPremultiplied;
            _currentEffect = null; // every Begin starts fresh — any prior SetEffect is cleared.

            graphicsDevice.BlendState = ToBlendState(_passBlendMode);
            graphicsDevice.DepthStencilState = enableDepthBuffer ? DepthStencilState.Default : DepthStencilState.None;
            graphicsDevice.RasterizerState = cullCounterClockwise ? RasterizerState.CullCounterClockwise : RasterizerState.CullNone;
            graphicsDevice.SamplerStates[0] = linearSampling ? SamplerState.LinearClamp : SamplerState.PointClamp;

            _effect.View = _camera.GetViewTransform().ToXna();
            var viewport = new Viewport(graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height);
            _effect.Projection = _camera.GetProjectionTransform(viewport).ToXna();
        }

        public void SetEffect(IEffect effect, BlendMode blendMode)
        {
            if (effect is null) throw new ArgumentNullException(nameof(effect));
            _currentEffect = effect as XnaEffect
                ?? throw new ArgumentException($"`effect` must be an {nameof(XnaEffect)} produced by {nameof(XnaEffectLoader)}.", nameof(effect));
            graphicsDevice.BlendState = ToBlendState(blendMode);
        }

        public void ResetEffect()
        {
            _currentEffect = null;
            graphicsDevice.BlendState = ToBlendState(_passBlendMode);
        }

        /// <summary>
        /// Creates a special <see cref="ITexture"/> that you can render to from this <see cref="IImmediateRenderer"/>. Call `SetRenderTarget()` to render to it. You must Dispose() this ITexture yourself.
        /// </summary>
        public Sprite CreateRenderTargetSprite(Vector2I size, bool supportDepthBuffer, int preferredMultiSampleCount = 0, bool preserveContents = false)
        {
            var renderTarget = new RenderTarget2D(
                graphicsDevice,
                width: size.X,
                height: size.Y,
                mipMap: false,
                SurfaceFormat.Color,
                supportDepthBuffer ? DepthFormat.Depth24 : DepthFormat.None,
                preferredMultiSampleCount,
                preserveContents ? RenderTargetUsage.PreserveContents : RenderTargetUsage.DiscardContents
                );

            return new Sprite(new XnaTexture(renderTarget), new Aabb(new(0, 0), new(1, 1)), new Aabb(Vector2.Zero, size), true);
        }

        public void SetRenderTarget(ITexture texture)
        {
            var xnaSprite = texture as XnaTexture ?? throw new ArgumentException($"`texture` should be created by calling `{nameof(CreateRenderTargetSprite)}()`.");
            var renderTarget = xnaSprite.Texture as RenderTarget2D ?? throw new ArgumentException($"`texture` should be created by calling `{nameof(CreateRenderTargetSprite)}()`.");

            graphicsDevice.SetRenderTarget(renderTarget);
        }

        public void ResetRenderTargetToScreen()
        {
            graphicsDevice.SetRenderTarget(null);
        }

        public void EnableMultiSampling()
        {
            gdm.PreferMultiSampling = true;
            gdm.ApplyChanges();
        }

        public void Draw(in Matrix4x4 worldTransform, in Mesh mesh)
        {
            foreach (var subMesh in mesh.SubMeshes)
            {
                Draw(worldTransform, subMesh);
            }
        }

        public void Draw(in Matrix4x4 worldTransform, in ColorMesh mesh)
        {
            Draw(worldTransform, mesh.Triangles.AsReadOnlySpan());
        }

        public void Draw(in Matrix4x4 worldTransform, in TexMesh mesh)
        {
            Draw(worldTransform, mesh.Triangles.AsReadOnlySpan(), mesh.Texture);
        }

        public void Draw(in Matrix4x4 worldTransform, in ReadOnlySpan<GpuTriangle> triangles)
        {
            if (triangles.Length == 0 || _camera == null) return;

            Effect activeEffect;
            if (_currentEffect != null)
            {
                // Custom effect manages its own World/View/Projection/Texture parameters.
                activeEffect = _currentEffect.Effect;
            }
            else
            {
                _effect.World = worldTransform.ToXna();
                _effect.Texture = null;
                _effect.TextureEnabled = false;
                activeEffect = _effect;
            }
            activeEffect.CurrentTechnique.Passes[0].Apply(); // don't use First() to prevent iterator allocation

            _xnaVerticesBuffer.Clear();
            _xnaVerticesBuffer.EnsureCapacity(triangles.Length * 3);
            triangles.MapToXna(_xnaVerticesBuffer.InternalArray);
            graphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, _xnaVerticesBuffer.InternalArray, 0, triangles.Length);
        }

        public void Draw(in Matrix4x4 worldTransform, in ReadOnlySpan<GpuTexTriangle> triangles, in ITexture? texture = null)
        {
            if (triangles.Length == 0 || _camera == null) return;

            Effect activeEffect;
            if (_currentEffect != null)
            {
                activeEffect = _currentEffect.Effect;
            }
            else
            {
                var xnaTexture = (XnaTexture?)texture;
                _effect.World = worldTransform.ToXna();
                _effect.Texture = xnaTexture?.Texture;
                _effect.TextureEnabled = xnaTexture != null;
                activeEffect = _effect;
            }
            activeEffect.CurrentTechnique.Passes[0].Apply(); // don't use First() to prevent iterator allocation

            _xnaVerticesTexBuffer.Clear();
            _xnaVerticesTexBuffer.EnsureCapacity(triangles.Length * 3);
            triangles.MapToXna(_xnaVerticesTexBuffer.InternalArray);
            graphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, _xnaVerticesTexBuffer.InternalArray, 0, triangles.Length);
        }

        /// <summary>
        /// Draws a quad with the sprite (texture) on it. The quad takes the size of the sprite, scaled by `scale`.
        /// </summary>
        public void Draw(in Matrix4x4 worldTransform, in Sprite sprite, Color? tint = null, bool flipX = false, float scale = 1f)
        {
            if (_camera == null) return;
            tint ??= Color.White;

            var xnaTexture = (XnaTexture)sprite.Texture;

            Effect activeEffect;
            if (_currentEffect != null)
            {
                activeEffect = _currentEffect.Effect;
            }
            else
            {
                _effect.World = worldTransform.ToXna();
                _effect.Texture = xnaTexture.Texture;
                _effect.TextureEnabled = true;
                activeEffect = _effect;
            }
            activeEffect.CurrentTechnique.Passes[0].Apply(); // don't use First() to prevent iterator allocation

            _xnaVerticesTexBuffer.Clear();

            var a = sprite.AabbPx.BottomLeft.ToVector3(0).ToXna();
            var b = sprite.AabbPx.TopLeft.ToVector3(0).ToXna();
            var c = sprite.AabbPx.TopRight.ToVector3(0).ToXna();
            var d = sprite.AabbPx.BottomRight.ToVector3(0).ToXna();

            var aUV = sprite.UvCoords.BottomLeft.ToXna();
            var bUV = sprite.UvCoords.TopLeft.ToXna();
            var cUV = sprite.UvCoords.TopRight.ToXna();
            var dUV = sprite.UvCoords.BottomRight.ToXna();

            var xnaTint = tint.Value.ToXna();

            if (flipX)
            {
                a.X = -a.X;
                b.X = -b.X;
                c.X = -c.X;
                d.X = -d.X;

                _xnaVerticesTexBuffer.Add(new VertexPositionColorTexture(a, xnaTint, aUV));
                _xnaVerticesTexBuffer.Add(new VertexPositionColorTexture(c, xnaTint, cUV));
                _xnaVerticesTexBuffer.Add(new VertexPositionColorTexture(b, xnaTint, bUV));

                _xnaVerticesTexBuffer.Add(new VertexPositionColorTexture(a, xnaTint, aUV));
                _xnaVerticesTexBuffer.Add(new VertexPositionColorTexture(d, xnaTint, dUV));
                _xnaVerticesTexBuffer.Add(new VertexPositionColorTexture(c, xnaTint, cUV));
            }
            else
            {
                _xnaVerticesTexBuffer.Add(new VertexPositionColorTexture(a, xnaTint, aUV));
                _xnaVerticesTexBuffer.Add(new VertexPositionColorTexture(b, xnaTint, bUV));
                _xnaVerticesTexBuffer.Add(new VertexPositionColorTexture(c, xnaTint, cUV));

                _xnaVerticesTexBuffer.Add(new VertexPositionColorTexture(a, xnaTint, aUV));
                _xnaVerticesTexBuffer.Add(new VertexPositionColorTexture(c, xnaTint, cUV));
                _xnaVerticesTexBuffer.Add(new VertexPositionColorTexture(d, xnaTint, dUV));
            }

            graphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, _xnaVerticesTexBuffer.InternalArray, 0, _xnaVerticesTexBuffer.Count / 3);
        }


        public void End()
        {
            _inRenderPass = false;
            _camera = null;
            _currentEffect = null;
        }

        public void PushClip(AabbI clipInScreenPx)
        {
            _clipStack.Add(clipInScreenPx);
            ApplyClip();
        }

        public void PopClip()
        {
            _clipStack.Pop();
            ApplyClip();
        }

        private void ApplyClip()
        {
            if (_clipStack.Count > 0)
            {
                var clip = _clipStack.Peek();
                graphicsDevice.ScissorRectangle = new Rectangle(clip.MinCorner.X, clip.MinCorner.Y, clip.Width, clip.Height);

                graphicsDevice.RasterizerState = graphicsDevice.RasterizerState.CullMode == CullMode.CullCounterClockwiseFace
                    ? _rasterizerStateScissorCull
                    : _rasterizerStateScissorNoCull;
            }
            else
            {
                graphicsDevice.ScissorRectangle = graphicsDevice.Viewport.Bounds;

                graphicsDevice.RasterizerState = graphicsDevice.RasterizerState.CullMode == CullMode.CullCounterClockwiseFace
                    ? RasterizerState.CullCounterClockwise
                    : RasterizerState.CullNone;
            }
        }

        public void ResetClip()
        {
            _clipStack.Clear();
            ApplyClip();
        }

        public Viewport Viewport => new(graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height);

        private static BlendState ToBlendState(BlendMode mode) => mode switch
        {
            BlendMode.Opaque => BlendState.Opaque,
            BlendMode.AlphaBlend => BlendState.AlphaBlend,
            BlendMode.NonPremultiplied => BlendState.NonPremultiplied,
            BlendMode.Additive => BlendState.Additive,
            BlendMode.AdditiveOneOne => _blendAdditiveOneOne,
            _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
        };

        public void Dispose()
        {
            _effect.Dispose();
        }
    }
}
