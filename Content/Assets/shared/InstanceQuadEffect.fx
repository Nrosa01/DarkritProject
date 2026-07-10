#if OPENGL
    #define SV_POSITION POSITION
    #define VS_SHADERMODEL vs_3_0
    #define PS_SHADERMODEL ps_3_0
#else
    #define VS_SHADERMODEL vs_4_0_level_9_1
    #define PS_SHADERMODEL ps_4_0_level_9_1
#endif

matrix Transform;

texture Texture;

sampler TextureSampler = sampler_state
{
    Texture = <Texture>;
};

struct VertexShaderInput
{
    float3 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float2 TexCoord : TEXCOORD0;
    float4 Color : COLOR0;
};

VertexShaderOutput MainVS(
    VertexShaderInput input,
    float2 InstancePosition : POSITION1,
    float2 InstanceSize : TEXCOORD1,
    float4 SourceUV : TEXCOORD2,
    float4 InstanceColor : COLOR1)
{
    VertexShaderOutput output;

    float2 position = input.Position.xy * InstanceSize + InstancePosition;

    output.Position = mul(float4(position, 0.0f, 1.0f), Transform);

    output.TexCoord = SourceUV.xy + input.TexCoord * SourceUV.zw;
    output.Color = InstanceColor;

    return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
    return tex2D(TextureSampler, input.TexCoord) * input.Color;
}

technique SpriteInstancing
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
}