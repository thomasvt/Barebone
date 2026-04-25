// 13-tap COD/Karis downsample (mip N-1 -> mip N).

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

float2 SrcTexelSize; // 1/srcWidth, 1/srcHeight

float3 S(float2 uv) { return tex2D(SourceSampler, uv).rgb; }

float4 DownsamplePS(PostVSOutput input) : COLOR0
{
    float2 t = SrcTexelSize;
    float2 uv = input.UV;

    float3 a = S(uv + t * float2(-2, -2));
    float3 b = S(uv + t * float2( 0, -2));
    float3 c = S(uv + t * float2( 2, -2));
    float3 d = S(uv + t * float2(-2,  0));
    float3 e = S(uv + t * float2( 0,  0));
    float3 f = S(uv + t * float2( 2,  0));
    float3 g = S(uv + t * float2(-2,  2));
    float3 h = S(uv + t * float2( 0,  2));
    float3 i = S(uv + t * float2( 2,  2));
    float3 j = S(uv + t * float2(-1, -1));
    float3 k = S(uv + t * float2( 1, -1));
    float3 l = S(uv + t * float2(-1,  1));
    float3 m = S(uv + t * float2( 1,  1));

    float3 r = e * 0.125;
    r += (a + c + g + i) * 0.03125;
    r += (b + d + f + h) * 0.0625;
    r += (j + k + l + m) * 0.125;

    return float4(r, 1.0);
}

technique Downsample
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL PostVS();
        PixelShader  = compile PS_SHADERMODEL DownsamplePS();
    }
}
