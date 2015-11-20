

struct VertexInputType
{
	float4 position : POSITION;
	float2 tex : TEXCOORD0;
};

struct PixelInputType
{
	float4 position : SV_POSITION;
	float2 tex : TEXCOORD0;
};
PixelInputType VShader(VertexInputType input)
{
	PixelInputType output;
	output.position = input.position;
	output.tex = input.tex;
	return output;
}
cbuffer vtEntry
{
	int Width;
	int Height;
	//int PosX;
	//int PosY;
	int PosOffsetX;
	int PosOffsetY;
	int TilesX;
	int TilesY;
	int ViewportWidth;
	int ViewportHeight;
	int TextureWidth;
	int TextureHeight;
	//int[] TilePosX;
	//int[] TilePosY;
};
Texture2DArray shaderTexture;
Texture2DArray indirectionTexture;
SamplerState SampleType;
SamplerState indSampleType;

float4 PShader(PixelInputType input) : SV_Target
{
	float vHalf = 0.5f;
float4 textureColor;

//float2 indirectionCoord = (((input.tex * float2(Width, Height)) / 120) / float2(TilesX, TilesY));
//float indirection = indirectionTexture.Sample(indSampleType, float3(input.tex, 0)) * (TilesX * TilesY);

//if (input.tex.x * ViewportWidth > TextureWidth || input.tex.y * ViewportHeight > TextureHeight) return float4(0.5f, 0.5f, 1.0f, 1.0f);
float w = input.tex.x * ViewportWidth; //clamp(input.tex.x * ViewportWidth, 0, TextureWidth);
float w2 = input.tex.x * Width;
float h = input.tex.y * ViewportHeight;// clamp(input.tex.y * ViewportHeight, 0, TextureHeight);
float h2 = input.tex.y * Height;

float wScale = 1;
float hScale = 1;
//if (ViewportWidth > ViewportHeight)
{

	if (Width >= Height && Width > ViewportWidth)
	{
		//h2 = clamp(input.tex.y * ViewportHeight * (ViewportWidth / Width), 0, Height * (ViewportWidth / Width));
		/*wScale = (ViewportWidth) / (Width);
		hScale = (ViewportHeight) / (Height);
		w = input.tex.x * Width;
		h = input.tex.y * Height * (Height / Width);*/
	}
	else if (Height > Width && Height > ViewportHeight)
	{
		//w2 = clamp(input.tex.x * ViewportWidth * (ViewportHeight / Height), 0, Width * (ViewportHeight / Height));
		/*wScale = (ViewportWidth) / (Width);
		hScale = (ViewportHeight) / (Height);
		w = input.tex.x * Width * (Width / Height);
		h = input.tex.y * Height;*/
	}
	else
	{
		w2 = w2;
		h2 = h2;
	}
}
/*else if (ViewportWidth < ViewportHeight)
{
float scale = (ViewportHeight) / (Height);

if (Width > ViewportWidth)
{
w = input.tex.x * Width * scale;
}
if (Height > ViewportHeight)
{
h = input.tex.y * Height;
}
}*/
//vHalf = vHalf*scale;
int tileRow = floor((h2 + vHalf) / (120));
int tileCol = floor((w2 + vHalf) / (120));
int tileNum = tileCol + tileRow * (TilesX)-tileRow;
float tCoordW = w2;// / TextureWidth * Width;
float tCoordH = h2;// / TextureHeight * Height;
float2 tileCoord = float2(((tCoordW + vHalf) % 120 + 4) / 128, ((tCoordH + vHalf) % 120 + 4) / 128);
// Sample the pixel color from the texture using the sampler at this texture coordinate location.
textureColor = shaderTexture.Sample(SampleType, float3(tileCoord,tileNum));

return textureColor;
}

technique11 VTEffect
{
	pass P0
	{
		VertexShader = compile vs_5_0 VShader();

		PixelShader = compile ps_5_0 PShader();
	}
}