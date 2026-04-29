// Shared fullscreen-triangle vertex shader for post-process passes.
// Vertex layout matches VertexPositionColorTexture (= Barebone.Graphics.Gpu.GpuTexTriangle byte layout)
// so the post-process triangles flow through XnaImmediateRenderer's standard Draw(GpuTexTriangle) path.
// Color is unused by the post-process pixel shaders but is present in the vertex declaration; declaring it
// here keeps the input layout consistent with the buffer.
// We feed in 3 vertices that draw a triangle covering NDC [-1..1] with UV in [0..1] inside the visible region.

#if OPENGL
    #define SV_POSITION POSITION
    #define VS_SHADERMODEL vs_3_0
    #define PS_SHADERMODEL ps_3_0
#else
    #define VS_SHADERMODEL vs_4_0_level_9_1
    #define PS_SHADERMODEL ps_4_0_level_9_1
#endif

struct PostVSInput
{
    float3 Position : POSITION0;
    float4 Color    : COLOR0;       // unused, but matches VertexPositionColorTexture / GpuTexTriangle
    float2 UV       : TEXCOORD0;
};

struct PostVSOutput
{
    float4 Position : SV_POSITION;
    float2 UV       : TEXCOORD0;
};

PostVSOutput PostVS(PostVSInput input)
{
    PostVSOutput o;
    o.Position = float4(input.Position, 1.0);
    o.UV       = input.UV;
    return o;
}
