using System.Numerics;
using Barebone.Graphics;
using Microsoft.Xna.Framework.Graphics;
using XnaVector2 = Microsoft.Xna.Framework.Vector2;

namespace Barebone.Monogame
{
    /// <summary>
    /// MonoGame-backed <see cref="IEffect"/>. Wraps a <see cref="Effect"/> and caches <see cref="EffectParameter"/> handles by name
    /// so subsequent SetX calls don't pay the dictionary cost on the underlying Effect.Parameters indexer each time.
    /// </summary>
    internal class XnaEffect : IEffect, IDisposable
    {
        internal Effect Effect { get; }
        private readonly Dictionary<string, EffectParameter> _parameterCache = new();

        public XnaEffect(Effect effect)
        {
            Effect = effect;
        }

        private EffectParameter Parameter(string name)
        {
            if (_parameterCache.TryGetValue(name, out var p)) return p;
            p = Effect.Parameters[name]
                ?? throw new ArgumentException($"Effect has no parameter named '{name}'.", nameof(name));
            _parameterCache[name] = p;
            return p;
        }

        public void SetMatrix(string parameterName, in Matrix4x4 value)
        {
            Parameter(parameterName).SetValue(value.ToXna());
        }

        public void SetVector2(string parameterName, in Vector2 value)
        {
            Parameter(parameterName).SetValue(new XnaVector2(value.X, value.Y));
        }

        public void SetFloat(string parameterName, float value)
        {
            Parameter(parameterName).SetValue(value);
        }

        public void SetTexture(string parameterName, ITexture texture)
        {
            var xna = texture as XnaTexture
                ?? throw new ArgumentException($"`texture` must be an {nameof(XnaTexture)}.", nameof(texture));
            Parameter(parameterName).SetValue(xna.Texture);
        }

        public void Dispose()
        {
            Effect.Dispose();
        }
    }
}
