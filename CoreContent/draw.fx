float4x4 World;
float4x4 View;
float4x4 Projection;

float4x4 WorldInverseTranspose;

float3 DiffuseLightDirection = float3(1, 0, 1);
float4 DiffuseColor = float4(1, 1, 1, 1);
float DiffuseIntensity = 1.0;
float3 FillLightDirection = float3(0,1,1);
float4 FillColor = float4(0.5,0.5,1,0.5);
float FillIntensity = 1.0;
float4 FogColor = float4(0,0,0,0);
texture Texture;

sampler diffuseSampler = sampler_state
{
    Texture = (Texture);
    MAGFILTER = LINEAR;
    MINFILTER = LINEAR;
    MIPFILTER = LINEAR;
    AddressU = Wrap;
    AddressV = Wrap;
};

struct VertexShaderInput
{
	float4 Position : POSITION0;
	float4 Color : COLOR0;
	float3 Normal : NORMAL0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
	float4 Color : COLOR0;
	float FogFactor : TEXCOORD1;
	float3 Normal : TEXCOORD2;
};


VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;
    input.Position.w = 1.0f;
    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);

	output.Color = input.Color;

	float4 cameraPosition;
    
	//We first calculate the Z coordinate of the vertex in view space.
	//We then use that with the fog end and start position in the fog factor equation to produce a fog factor 
	//that we send into the pixel shader.

    // Calculate the camera position.
    cameraPosition = mul(input.Position, World);
    cameraPosition = mul(cameraPosition, View);

    // Calculate linear fog.    
    output.FogFactor = saturate((30.0f + cameraPosition.z) / (20.0f));

	output.Normal = input.Normal;
    return output;
}

struct TexturedVertexShaderInput
{
	float4 Position : POSITION0;
	float4 Color : COLOR0;
	float3 Normal : NORMAL0;
	float2 Texcoord : TEXCOORD0;
};

struct TexturedVertexShaderOutput
{
    float4 Position : POSITION0;
	float4 Color : COLOR0;
	float2 Texcoord : TEXCOORD0;
	float FogFactor : TEXCOORD1;
	float3 Normal : TEXCOORD2;
};


TexturedVertexShaderOutput TexturedVertexShaderFunction(TexturedVertexShaderInput input)
{
    TexturedVertexShaderOutput output;
    input.Position.w = 1.0f;
    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);

	output.Color = input.Color;

	float4 cameraPosition;
    
	//We first calculate the Z coordinate of the vertex in view space.
	//We then use that with the fog end and start position in the fog factor equation to produce a fog factor 
	//that we send into the pixel shader.

    // Calculate the camera position.
    cameraPosition = mul(input.Position, World);
    cameraPosition = mul(cameraPosition, View);

    // Calculate linear fog.    
    output.FogFactor = saturate((30.0f + cameraPosition.z) / (20.0f));

	output.Normal = input.Normal;
	output.Texcoord = input.Texcoord;
    return output;
}

struct PixelShaderOutput
{
    float4 Color : COLOR0;
};

PixelShaderOutput PSColor(VertexShaderOutput input) : COLOR0
{
    PixelShaderOutput output;

	float4 normal = float4(normalize(mul(input.Normal, WorldInverseTranspose)));
	float4 texColor = input.Color;//tex2D(diffuseSampler, input.TexCoord);
    float lightIntensity = dot(normal, DiffuseLightDirection);
    output.Color = saturate(
		(DiffuseColor * DiffuseIntensity * lightIntensity * texColor)
		+ (FillColor * FillIntensity * dot(normal, FillLightDirection) * texColor));
	output.Color += texColor * 0.1;
    //output.Color = lerp(output.Color, FogColor, 1.0f - input.FogFactor);
	output.Color.a = input.Color.a;
    return output;
}

PixelShaderOutput PSTexturedColor(TexturedVertexShaderOutput input) : COLOR0
{
    PixelShaderOutput output;
	float4 normal = float4(normalize(mul(input.Normal, WorldInverseTranspose)));
	float4 texColor = tex2D(diffuseSampler, input.Texcoord);
	float4 diffuse = input.Color * texColor;
    float lightIntensity = dot(normal, DiffuseLightDirection);
    output.Color = saturate(
		(DiffuseColor * DiffuseIntensity * lightIntensity * diffuse)
		);//+ (FillColor * FillIntensity * dot(normal, FillLightDirection) * diffuse));
	output.Color += diffuse * 0.1;
	output.Color.a = texColor.a;
    return output;
}

PixelShaderOutput PSTexturedColorNolight(TexturedVertexShaderOutput input) : COLOR0
{
    PixelShaderOutput output;
	float4 texColor = tex2D(diffuseSampler, input.Texcoord);
    output.Color = input.Color * texColor;
	output.Color.a = texColor.a;
	clip(texColor.a - 0.1);
    return output;
}

technique DrawColor
{
    pass Pass1
    {
		AlphaBlendEnable = true;
		BlendOp = Add;
		SrcBlend = One;
		DestBlend = InvSrcAlpha;
		ZEnable = true;
		ZWriteEnable = true;
		ZFunc = LessEqual;
		CullMode = None;
		
        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PSColor();
    }
}

technique DrawTexturedColor
{
    pass Pass1
    {
		AlphaBlendEnable = true;
		BlendOp = Add;
		SrcBlend = One;
		DestBlend = InvSrcAlpha;
		ZEnable = true;
		ZWriteEnable = true;
		ZFunc = LessEqual;
		CullMode = None;
		
        VertexShader = compile vs_2_0 TexturedVertexShaderFunction();
        PixelShader = compile ps_2_0 PSTexturedColor();
    }
}

technique DrawTexturedColorNolight
{
    pass Pass1
    {
		AlphaBlendEnable = true;
		BlendOp = Add;
		SrcBlend = One;
		DestBlend = InvSrcAlpha;
		ZEnable = true;
		ZWriteEnable = true;
		ZFunc = LessEqual;
		CullMode = None;
		
        VertexShader = compile vs_2_0 TexturedVertexShaderFunction();
        PixelShader = compile ps_2_0 PSTexturedColorNolight();
    }
}


