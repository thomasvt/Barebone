namespace Barebone.Game.Graphics
{
    public struct BloomSettings(float treshold, float softKnee, float brightIntensity, float upsampleStrength, float finalIntensity)
    {
        /// <summary>
        /// Blooms only when Max(R, G, B) > Treshold.
        /// </summary>
        public float BloomThreshold { get; set; } = treshold;

        /// <summary>
        /// Width of the fade edge of the bloom. So less is sharp edges, more is slower fadeout
        /// </summary>
        public float BloomSoftKnee { get; set; } = softKnee;

        /// <summary>
        /// mostly irrelevant in non HDR. Use FinalIntensity
        /// </summary>
        public float BloomBrightIntensity { get; set; } = brightIntensity;

        /// <summary>
        /// 
        /// </summary>
        public float BloomUpsampleStrength { get; set; } = upsampleStrength;

        /// <summary>
        /// 
        /// </summary>
        public float BloomFinalIntensity { get; set; } = finalIntensity;

        // Subtle realism — threshold 0.85, knee 0.5, upsample 1.0, final 0.45
        // Punchy synthwave / retro — threshold 0.6, knee 0.2, upsample 1.3, final 0.8
        // Dreamy / fog-of-light — threshold 0.4, knee 0.7, upsample 1.5, final 0.6
        // Sharp neon highlights only — threshold 0.95, knee 0.1, upsample 0.7, final 0.7

        public static BloomSettings Realistic = new(0.85f, 0.5f, 1, 1, 0.45f);
        public static BloomSettings RealisticIntense = new(0.85f, 0.5f, 1, 1, 0.6f);
        public static BloomSettings Retro = new(0.6f, 0.2f, 1f, 1.3f, 0.8f);
        public static BloomSettings Foggy = new(0.4f, 0.7f, 1, 1.5f, 0.6f);
        public static BloomSettings SharpNeon = new(0.95f, 0.1f, 1, 0.7f, 0.7f);
        public static BloomSettings None = new(0.0f, 0f, 0, 0f, 0f);
    }
}
