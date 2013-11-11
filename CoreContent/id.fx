float4x4 World;
float4x4 View;
float4x4 Projection;

float4 ID;

struct VertexShaderInput
{
	float4 Position : POSITION0;
	float4 Color : COLOR0;
	float3 Normal : NORMAL0;
};

struct VertexShaderOutput
{
	float4 Position : POSITION0;
};

VertexShaderOutput vertexShader(VertexShaderInput input)
{
    VertexShaderOutput output;
    input.Position.w = 1.0f;
    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);
	return output;
}

struct PixelShaderOutput
{
    float4 Color : COLOR0;
};

PixelShaderOutput pixelShader(VertexShaderOutput input) : COLOR0
{
    PixelShaderOutput output;
	output.Color = ID;
    return output;
}

technique DrawID
{
    pass Pass1
    {
		AlphaBlendEnable = false;
		ZEnable = true;
		ZWriteEnable = true;
		ZFunc = LessEqual;
		CullMode = None;
		
        VertexShader = compile vs_2_0 vertexShader();
        PixelShader = compile ps_2_0 pixelShader();
    }
}