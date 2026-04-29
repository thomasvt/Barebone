using System.Reflection;

namespace Barebone.Graphics
{
    /// <summary>
    /// Platform-agnostic effect loader. Mirrors <see cref="Barebone.Assets.ITextureLoader"/>: the platform implements it
    /// (e.g. Barebone.Monogame.XnaEffectLoader), engine code consumes it through this interface only.
    /// </summary>
    public interface IEffectLoader
    {
        IEffect Load(string assetPath);
        IEffect Load(Stream stream);
    }
}
