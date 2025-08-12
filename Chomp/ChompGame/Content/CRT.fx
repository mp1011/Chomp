sampler2D inputSampler : register(s0);

float2 ScreenSize;
float Time;

float4 main(float2 uv : TEXCOORD0) : COLOR0
{
    float2 center = float2(0.5, 0.5);
    float2 dist = uv - center;

    // Screen bulge: shrink UVs toward center for outward (convex) bulge
    float bulgeStrength = -0.1;
    float dist2 = dot(dist, dist);
    float2 bulgedUV = center + dist * (1.0 - bulgeStrength * dist2);

    // Clamp to valid texture coordinates
    bulgedUV = saturate(bulgedUV);

    float2 pixel = bulgedUV * ScreenSize;

    // Simulate scanlines
    float scanline = 0.85 + 0.15 * sin(pixel.y * 3.14159);

    // Slight curvature (already handled by bulge, but keep for intensity)
    float curve = 1.0; // - 0.07 * dist2;

    // Slight color offset for chromatic aberration
    float3 color;
    color.r = tex2D(inputSampler, bulgedUV + float2(0.001, 0)).r;
    color.g = tex2D(inputSampler, bulgedUV).g;
    color.b = tex2D(inputSampler, bulgedUV - float2(0.001, 0)).b;

    color *= scanline * curve;

    return float4(color, 1.0);
}

technique CRT
{
    pass
    {
        PixelShader = compile ps_3_0 main();
    }
}