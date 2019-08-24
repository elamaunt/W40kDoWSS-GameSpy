sampler2D texture0 : register(S0);

float Time : register(C0);

float rand(float2 co)
{
    return frac(sin(fmod(dot(co.xy, float2(12.9898f, 78.233f)), 3.14f)) * 43758.5453f);
}

float4 main(float2 uv : TEXCOORD) : COLOR
{
	float4 Color = tex2D(texture0, uv);
    
    float m = Time - floor(Time);

    if (m > 0.5)
        m = 1 - m;

    Color *= m;

    float l = cos(uv.y);
    l *= l;
    l /= 3.;
    l += 0.6 + rand(uv * Time);

    Color *= l;

	return Color;
}

