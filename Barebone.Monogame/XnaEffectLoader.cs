using System.Reflection;
using Barebone.Graphics;
using Microsoft.Xna.Framework.Graphics;

namespace Barebone.Monogame
{
    /// <summary>
    /// MonoGame implementation of <see cref="IEffectLoader"/>. Loads compiled MonoGame effect blobs (.mgfxo).
    /// Mirrors <see cref="XnaTextureLoader"/> in shape and lifetime: the platform constructs one,
    /// engine code consumes it through <see cref="IEffectLoader"/>.
    /// </summary>
    public class XnaEffectLoader(GraphicsDevice graphicsDevice) : IEffectLoader
    {
        public IEffect Load(string assetPath)
        {
            using var stream = File.OpenRead(assetPath);
            return Load(stream);
        }

        public IEffect Load(Stream stream)
        {
            using var ms = new MemoryStream();
            stream.CopyTo(ms);
            return new XnaEffect(new Effect(graphicsDevice, ms.ToArray()));
        }
    }
}
