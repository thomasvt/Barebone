// 9-tap tent-filter upsample (mip N -> mip N-1).
// Output is added to the destination via Additive BlendState configured C#-side.

#include "Fullscreen.fxh"

Texture2D Source;
sampler SourceSampler = sampler_state
{
    Texture = <Source>;
    MinFilter = LINEAR;
    MagFilter = LINEAR;
    AddressU = CLAMP;
    AddressV = CLAMP;
};

float2 SrcTexelSize;
float Strength;

float3 S(float2 uv) { return tex2D(SourceSampler, uv).rgb; }

float4 UpsamplePS(PostVSOutput input) : COLOR0
{
    float2 t = SrcTexelSize;
    float2 uv = input.UV;

    float3 r  = S(uv + t * float2(-1, -1));
    r        += S(uv + t * float2( 0, -1)) * 2.0;
    r        += S(uv + t * float2( 1, -1));
    r        += S(uv + t * float2(-1,  0)) * 2.0;
    r        += S(uv + t * float2( 0,  0)) * 4.0;
    r        += S(uv + t * float2( 1,  0)) * 2.0;
    r        += S(uv + t * float2(-1,  1));
    r        += S(uv + t * float2( 0,  1)) * 2.0;
    r        += S(uv + t * float2( 1,  1));

    return float4(r * (Strength * (1.0 / 16.0)), 1.0);
}

technique Upsample
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL PostVS();
        PixelShader  = compile PS_SHADERMODEL UpsamplePS();
    }
}
