﻿using System.Numerics;
using Barebone.Geometry;
using Barebone.Graphics;
using Barebone.Graphics.Cameras;
using Barebone.Graphics.Gpu;
using Barebone.Graphics.Sprites;
using Barebone.Platform;
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

        private ICamera? _camera;
        private readonly BBList<VertexPositionColor> _xnaVerticesBuffer = new();
        private readonly BBList<VertexPositionColorTexture> _xnaVerticesTexBuffer = new();

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

        public void Begin(ICamera camera, bool enableDepthBuffer, bool additiveBlend, bool cullCounterClockwise, bool linearSampling = true)
        {
            _camera = camera;

            graphicsDevice.BlendState = additiveBlend ? BlendState.Additive : BlendState.NonPremultiplied;
            graphicsDevice.DepthStencilState = enableDepthBuffer ? DepthStencilState.Default : DepthStencilState.None;
            graphicsDevice.RasterizerState = cullCounterClockwise ? RasterizerState.CullCounterClockwise : RasterizerState.CullNone;
            graphicsDevice.SamplerStates[0] = linearSampling ? SamplerState.LinearClamp : SamplerState.PointClamp;

            _effect.View = _camera.GetViewTransform().ToXna();
            var viewport = new Viewport(graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height);
            _effect.Projection = _camera.GetProjectionTransform(viewport).ToXna();
        }

        /// <summary>
        /// Creates a special <see cref="ITexture"/> that you can render to from this <see cref="IImmediateRenderer"/>. Call `SwitchRenderTargetTo()` to render to it. You must Dispose() this ITexture yourself.
        /// </summary>
        public Sprite CreateRenderTargetSprite(Vector2I size, bool supportDepthBuffer, int preferredMultiSampleCount = 0)
        {
            var renderTarget = new RenderTarget2D(
                graphicsDevice,
                width: size.X,
                height: size.Y,
                mipMap: false,
                SurfaceFormat.Color,
                supportDepthBuffer ? DepthFormat.Depth24 : DepthFormat.None,
                preferredMultiSampleCount,
                RenderTargetUsage.DiscardContents
                );

            return new Sprite(new XnaTexture(renderTarget), new Aabb(new(0, 0), new(1, 1)), new Aabb(Vector2.Zero, size), true);
        }

        public void SwitchRenderTargetTo(ITexture texture)
        {
            var xnaSprite = texture as XnaTexture ?? throw new ArgumentException($"`texture` should be created by calling `{nameof(CreateRenderTargetSprite)}()`.");
            var renderTarget = xnaSprite.Texture as RenderTarget2D ?? throw new ArgumentException($"`texture` should be created by calling `{nameof(CreateRenderTargetSprite)}()`.");

            graphicsDevice.SetRenderTarget(renderTarget);
        }

        public void SwitchRenderTargetToScreen()
        {
            graphicsDevice.SetRenderTarget(null);
        }

        public void EnableMultiSampling()
        {
            gdm.PreferMultiSampling = true;
            gdm.ApplyChanges();
        }

        public void Draw(in Matrix4x4 worldTransform, in Mesh mesh, in Color? replacementColor = null)
        {
            Draw(worldTransform, mesh.Triangles.AsReadOnlySpan(), replacementColor);
        }

        public void Draw(in Matrix4x4 worldTransform, in ReadOnlySpan<GpuTriangle> triangles, in Color? replacementColor = null)
        {
            if (triangles.Length == 0 || _camera == null) return;

            _effect.World = worldTransform.ToXna();
            _effect.Texture = null;
            _effect.TextureEnabled = false;
            _effect.CurrentTechnique.Passes[0].Apply(); // don't use First() to prevent iterator allocation

            _xnaVerticesBuffer.Clear();
            _xnaVerticesBuffer.EnsureCapacity(triangles.Length * 3);
            triangles.MapToXna(_xnaVerticesBuffer.InternalArray);
            _effect.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, _xnaVerticesBuffer.InternalArray, 0, triangles.Length);
        }

        public void Draw(in Matrix4x4 worldTransform, in ReadOnlySpan<GpuTexTriangle> triangles, in ITexture texture)
        {
            if (triangles.Length == 0 || _camera == null) return;

            var xnaTexture = (XnaTexture)texture;

            _effect.World = worldTransform.ToXna();
            _effect.Texture = xnaTexture.Texture;
            _effect.TextureEnabled = true;
            _effect.CurrentTechnique.Passes[0].Apply(); // don't use First() to prevent iterator allocation

            _xnaVerticesTexBuffer.Clear();
            _xnaVerticesTexBuffer.EnsureCapacity(triangles.Length * 3);
            triangles.MapToXna(_xnaVerticesTexBuffer.InternalArray);
            _effect.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, _xnaVerticesTexBuffer.InternalArray, 0, triangles.Length);
        }

        /// <summary>
        /// Draws a quad with the sprite (texture) on it. The quad takes the size of the sprite, scaled by `scale`.
        /// </summary>
        public void Draw(in Matrix4x4 worldTransform, in Sprite sprite, Color tint, bool flipX = false, float scale = 1f)
        {
            if (_camera == null) return;

            var xnaTexture = (XnaTexture)sprite.Texture;

            _effect.World = worldTransform.ToXna();
            _effect.Texture = xnaTexture.Texture;
            _effect.TextureEnabled = true;
            _effect.CurrentTechnique.Passes[0].Apply(); // don't use First() to prevent iterator allocation

            _xnaVerticesTexBuffer.Clear();

            var a = sprite.AabbPx.BottomLeft.ToVector3(0).ToXna();
            var b = sprite.AabbPx.TopLeft.ToVector3(0).ToXna();
            var c = sprite.AabbPx.TopRight.ToVector3(0).ToXna();
            var d = sprite.AabbPx.BottomRight.ToVector3(0).ToXna();

            var aUV = sprite.UvCoords.BottomLeft.ToXna();
            var bUV = sprite.UvCoords.TopLeft.ToXna();
            var cUV = sprite.UvCoords.TopRight.ToXna();
            var dUV = sprite.UvCoords.BottomRight.ToXna();

            var xnaTint = tint.ToXna();

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

            _effect.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, _xnaVerticesTexBuffer.InternalArray, 0, _xnaVerticesTexBuffer.Count / 3);
        }


        public void End()
        {
            _camera = null;
        }
        public Viewport Viewport => new(graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height);

        public void Dispose()
        {
            _effect.Dispose();
        }
    }
}
