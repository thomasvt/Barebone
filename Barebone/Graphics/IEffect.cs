using System.Numerics;

namespace Barebone.Graphics
{
    /// <summary>
    /// Platform-agnostic shader/effect handle. Parameter names match what the underlying shader declares.
    /// Loaded via <see cref="IEffectLoader"/>; activated on a renderer via <see cref="IImmediateRenderer.SetEffect"/>.
    /// Mirrors the <see cref="ITextureLoader"/>/<see cref="ITexture"/> split so the engine code stays platform-free.
    /// </summary>
    public interface IEffect
    {
        void SetMatrix(string parameterName, in Matrix4x4 value);
        void SetVector2(string parameterName, in Vector2 value);
        void SetFloat(string parameterName, float value);
        void SetTexture(string parameterName, ITexture texture);
    }
}
