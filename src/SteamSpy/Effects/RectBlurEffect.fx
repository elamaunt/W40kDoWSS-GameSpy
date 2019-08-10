sampler2D rectBlurEffect : register(S0);
float2 upperLeftCorner : register(C0);
float2 lowerRightCorner : register(C1);

float Angle : register(C2);
float BlurAmount : register(C3);

float PI = 3.14159265358979323846;
float EPSILON = 0.0001;

float ComputeGaussian(float n)
{
    float theta = 2.0f + EPSILON; //float.Epsilon;

    return theta = (float)((1.0 / sqrt(2 * PI * theta)) *
        exp(-(n * n) / (2 * theta * theta)));
}

float4 gaussianblur(float2 texCoord: TEXCOORD0) : COLOR
{

    float SampleWeights[7];
    float2 SampleOffsets[15];

    // The first sample always has a zero offset.
    float2 initer = { 0.0f, 0.0f };
    SampleWeights[0] = ComputeGaussian(0);
    SampleOffsets[0] = initer;

    // Maintain a sum of all the weighting values.
    float totalWeights = SampleWeights[0];

    // Add pairs of additional sample taps, positioned
    // along a line in both directions from the center.
    for (int i = 0; i < 7 / 2; i++)
    {
        // Store weights for the positive and negative taps.
        float weight = ComputeGaussian(i + 1);

        SampleWeights[i * 2 + 1] = weight;
        SampleWeights[i * 2 + 2] = weight;

        totalWeights += weight * 2;


        float sampleOffset = i * 2 + 1.5f;

        float2 delta = { (1.0f / 512), 0 };
        delta = delta * sampleOffset;

        // Store texture coordinate offsets for the positive and negative taps.
        SampleOffsets[i * 2 + 1] = delta;
        SampleOffsets[i * 2 + 2] = -delta;
    }

    // Normalize the list of sample weightings, so they will always sum to one.
    for (int j = 0; j < 7; j++)
    {
        SampleWeights[j] /= totalWeights;
    }

    float4 color = 0.0f;

    for (int k = 0; k < 7; k++)
    {
        color += tex2D(rectBlurEffect,
            texCoord + SampleOffsets[k]) * SampleWeights[k];
    }

    return color;
}

float4 directionalBlur(float2 uv : TEXCOORD) : COLOR
{
    float4 c = 0;
    float rad = Angle * 0.0174533f;
    float xOffset = cos(rad);
    float yOffset = sin(rad);

    for (int i = 0; i < 12; i++)
    {
        uv.x = uv.x - BlurAmount * xOffset;
        uv.y = uv.y - BlurAmount * yOffset;
        c += tex2D(rectBlurEffect, uv);
    }
    c /= 12;

    return c;
}

float4 main(float2 uv : TEXCOORD) : COLOR
{
    if (uv.x < upperLeftCorner.x || uv.y < upperLeftCorner.y || uv.x > lowerRightCorner.x || uv.y > lowerRightCorner.y)
    {
        return tex2D(rectBlurEffect, uv);
    }

    return gaussianblur(uv);
}