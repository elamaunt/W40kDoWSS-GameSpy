sampler2D texture0 : register(S0);
float Time : register(C0);

float4 main(float2 uv : TEXCOORD) : COLOR
{
	float4 Color = tex2D(texture0, uv);

	Color*= Time - floor(Time);

	return Color;
}


