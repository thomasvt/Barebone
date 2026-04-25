// Soft-knee threshold for bloom bright extraction.
// Reads the SSAA scene (e.g. 2x backbuffer size) with bilinear, so this also acts as the SSAA resolve
// in the path that flows into bloom.

#include "Fullscreen.fxh"

Texture2D Scene;
sampler SceneSampler = sampler_state
{
    Texture = <Scene>;
    MinFilter = LINEAR;
    MagFilter = LINEAR;
    AddressU = CLAMP;
    AddressV = CLAMP;
};

float Threshold;
float SoftKnee;
float Intensity;

float4 BrightPS(PostVSOutput input) : COLOR0
{
    float3 c = tex2D(SceneSampler, input.UV).rgb;
    float brightness = max(c.r, max(c.g, c.b));

    float knee = max(SoftKnee, 1e-4);
    float soft = clamp(brightness - Threshold + knee, 0.0, 2.0 * knee);
    soft = soft * soft / (4.0 * knee);
    float contribution = max(soft, brightness - Threshold);
    contribution /= max(brightness, 1e-4);

    return float4(c * contribution * Intensity, 1.0);
}

technique Bright
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL PostVS();
        PixelShader  = compile PS_SHADERMODEL BrightPS();
    }
}
