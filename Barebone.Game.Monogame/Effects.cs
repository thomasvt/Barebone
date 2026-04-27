using System.Reflection;
using Microsoft.Xna.Framework.Graphics;

namespace Barebone.Game.Monogame
{
    internal abstract class EffectBase : IDisposable
    {
        protected Effect XnaEffect;

        protected EffectBase(GraphicsDevice gd, string resourceName)
        {
            var fullName = $"Barebone.Game.Monogame.Shaders.{resourceName}.mgfxo";
            using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(fullName)
                               ?? throw new MonoGameException(
                                   $"Embedded shader '{fullName}' not found. " +
                                   "Run mgfxc Shaders/*.fx to generate the .mgfxo files first.");
            using var ms = new MemoryStream();
            stream.CopyTo(ms);
            XnaEffect = new Effect(gd, ms.ToArray());
        }

        public void Apply()
        {
            XnaEffect.CurrentTechnique.Passes[0].Apply();
        }

        public void Dispose()
        {
            XnaEffect.Dispose();
        }
    }

    internal class SpriteEffect : EffectBase
    {
        public EffectParameter World { get; }
        public EffectParameter View { get; }
        public EffectParameter Projection { get; }
        public EffectParameter Texture { get; }

        public SpriteEffect(GraphicsDevice gd) : base(gd, "Sprite")
        {
            World = XnaEffect.Parameters["World"];
            View = XnaEffect.Parameters["View"];
            Projection = XnaEffect.Parameters["Projection"];
            Texture = XnaEffect.Parameters["SpriteTex"];
        }
    }

    internal class BrightEffect : EffectBase
    {
        public EffectParameter Scene { get; }
        public EffectParameter Threshold { get; }
        public EffectParameter SoftKnee { get; }
        public EffectParameter Intensity { get; }

        public BrightEffect(GraphicsDevice gd) : base(gd, "BrightPass")
        {
            Scene = XnaEffect.Parameters["Scene"];
            Threshold = XnaEffect.Parameters["Threshold"];
            SoftKnee = XnaEffect.Parameters["SoftKnee"];
            Intensity = XnaEffect.Parameters["Intensity"];
        }
    }

    internal class DownsampleEffect : EffectBase
    {
        public EffectParameter Source { get; }
        public EffectParameter SrcTexel { get; }

        public DownsampleEffect(GraphicsDevice gd) : base(gd, "Downsample")
        {
            Source = XnaEffect.Parameters["Source"];
            SrcTexel = XnaEffect.Parameters["SrcTexelSize"];
        }
    }

    internal class UpsampleEffect : EffectBase
    {
        public EffectParameter Source { get; }
        public EffectParameter SrcTexel { get; }
        public EffectParameter Strength { get; }

        public UpsampleEffect(GraphicsDevice gd) : base(gd, "Upsample")
        {
            Source = XnaEffect.Parameters["Source"];
            SrcTexel = XnaEffect.Parameters["SrcTexelSize"];
            Strength = XnaEffect.Parameters["Strength"];
        }
    }

    internal class CompositeEffect : EffectBase
    {
        public EffectParameter Scene { get; }
        public EffectParameter Bloom { get; }
        public EffectParameter BloomIntensity { get; }

        public CompositeEffect(GraphicsDevice gd) : base(gd, "Composite")
        {
            Scene = XnaEffect.Parameters["Scene"];
            Bloom = XnaEffect.Parameters["Bloom"];
            BloomIntensity = XnaEffect.Parameters["BloomIntensity"];
        }
    }
}
