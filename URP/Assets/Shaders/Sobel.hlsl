#ifndef SOBEL_INCLUDED
#define SOBEL_INCLUDED

float SobelLuma(float3 c)
{
    return dot(c, float3(0.2126, 0.7152, 0.0722));
}

void Sobel_float(
    UnityTexture2D Source,
    float2 UV,
    float Width,
    float Strength,
    float Threshold,
    float Softness,
    out float Edge
)
{
    float2 p = Source.texelSize.xy * Width;

    float tl = SobelLuma(SAMPLE_TEXTURE2D(Source.tex, Source.samplerstate, UV + p * float2(-1, 1)).rgb);
    float tc = SobelLuma(SAMPLE_TEXTURE2D(Source.tex, Source.samplerstate, UV + p * float2(0, 1)).rgb);
    float tr = SobelLuma(SAMPLE_TEXTURE2D(Source.tex, Source.samplerstate, UV + p * float2(1, 1)).rgb);

    float ml = SobelLuma(SAMPLE_TEXTURE2D(Source.tex, Source.samplerstate, UV + p * float2(-1, 0)).rgb);
    float mr = SobelLuma(SAMPLE_TEXTURE2D(Source.tex, Source.samplerstate, UV + p * float2(1, 0)).rgb);

    float bl = SobelLuma(SAMPLE_TEXTURE2D(Source.tex, Source.samplerstate, UV + p * float2(-1, -1)).rgb);
    float bc = SobelLuma(SAMPLE_TEXTURE2D(Source.tex, Source.samplerstate, UV + p * float2(0, -1)).rgb);
    float br = SobelLuma(SAMPLE_TEXTURE2D(Source.tex, Source.samplerstate, UV + p * float2(1, -1)).rgb);

    float gx = -tl - 2.0 * ml - bl + tr + 2.0 * mr + br;
    float gy = tl + 2.0 * tc + tr - bl - 2.0 * bc - br;

    float mag = sqrt(gx * gx + gy * gy) * Strength;
    Edge = smoothstep(Threshold, Threshold + Softness, mag);
}

#endif
