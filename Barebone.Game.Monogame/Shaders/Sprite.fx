// Sprite shader.
// Vertices arrive in WORLD space (or model space when SetWorldTransform != identity).
// World × View × Projection is applied on the GPU.
//   World      = caller's SetWorldTransform (Matrix3x2 lifted to Matrix4x4)
//   View       = active camera's WorldToScreen transform (Matrix3x2 lifted to Matrix4x4) — maps world -> screen pixels
//   Projection = orthographic mapping (0..viewport_w, 0..viewport_h) -> NDC
// For "color only" draws, the C# side binds a 1x1 white texture so this single shader handles both cases.

#if OPENGL
    #define SV_POSITION POSITION
    #define VS_SHADERMODEL vs_3_0
    #define PS_SHADERMODEL ps_3_0
#else
    #define VS_SHADERMODEL vs_4_0_level_9_1
    #define PS_SHADERMODEL ps_4_0_level_9_1
#endif

float4x4 World;
float4x4 View;
float4x4 Projection;

Texture2D SpriteTex;
sampler SpriteSampler = sampler_state
{
    Texture = <SpriteTex>;
    MinFilter = LINEAR;
    MagFilter = LINEAR;
    MipFilter = LINEAR;
    AddressU = CLAMP;
    AddressV = CLAMP;
};

struct VSInput
{
    float2 Position : POSITION0;
    float4 Color    : COLOR0;
    float2 UV       : TEXCOORD0;
};

struct VSOutput
{
    float4 Position : SV_POSITION;
    float4 Color    : COLOR0;
    float2 UV       : TEXCOORD0;
};

VSOutput SpriteVS(VSInput input)
{
    VSOutput o;
    float4 worldPos = mul(float4(input.Position, 0.0, 1.0), World);
    float4 viewPos  = mul(worldPos, View);
    o.Position      = mul(viewPos, Projection);
    o.Color         = input.Color;
    o.UV            = input.UV;
    return o;
}

float4 SpritePS(VSOutput input) : COLOR0
{
    float4 t = tex2D(SpriteSampler, input.UV);
    return t * input.Color;
}

technique Sprite
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL SpriteVS();
        PixelShader  = compile PS_SHADERMODEL SpritePS();
    }
}
