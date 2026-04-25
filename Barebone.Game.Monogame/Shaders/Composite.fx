// Final composite: bilinear-samples the 2x SSAA scene (= the actual antialiasing happens here)
// and adds the bloom mip[0] on top, output to backbuffer.

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

Texture2D Bloom;
sampler BloomSampler = sampler_state
{
    Texture = <Bloom>;
    MinFilter = LINEAR;
    MagFilter = LINEAR;
    AddressU = CLAMP;
    AddressV = CLAMP;
};

float BloomIntensity;

float4 CompositePS(PostVSOutput input) : COLOR0
{
    float3 scene = tex2D(SceneSampler, input.UV).rgb;
    float3 bloom = tex2D(BloomSampler, input.UV).rgb;
    return float4(scene + bloom * BloomIntensity, 1.0);
}

technique Composite
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL PostVS();
        PixelShader  = compile PS_SHADERMODEL CompositePS();
    }
}
