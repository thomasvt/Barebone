namespace Barebone.Graphics
{
    /// <summary>
    /// Platform-agnostic blend modes. Backends map these to their native blend states (e.g. MonoGame BlendState).
    /// </summary>
    public enum BlendMode
    {
        /// <summary>Source replaces destination. No blending.</summary>
        Opaque,

        /// <summary>Premultiplied alpha: src + (1 - src.a) * dst.</summary>
        AlphaBlend,

        /// <summary>Non-premultiplied alpha: src.a * src + (1 - src.a) * dst.</summary>
        NonPremultiplied,

        /// <summary>Source-alpha-modulated additive: src.a * src + dst.</summary>
        Additive,

        /// <summary>Pure additive: src + dst. Useful for HDR / bloom upsampling where contributions accumulate raw.</summary>
        AdditiveOneOne,
    }
}
