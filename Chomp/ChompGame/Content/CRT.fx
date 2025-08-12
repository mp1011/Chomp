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

    // Slight color offset for chromatic aberration
    float3 color;
    color.r = tex2D(inputSampler, bulgedUV + float2(0.001, 0)).r;
    color.g = tex2D(inputSampler, bulgedUV).g;
    color.b = tex2D(inputSampler, bulgedUV - float2(0.001, 0)).b;

    color *= scanline;

    // --- Screen glare effect ---
    // Elliptical highlight near top left, simulating glass reflection
    float2 glareCenter = float2(0.32, 0.18); // position of glare
    float2 glareDist = (uv - glareCenter);
    float glareRadius = 0.28;
    float glare = 0.0;

    // Soft elliptical falloff
    float glareFalloff = dot(glareDist / float2(1.0, 0.6), glareDist / float2(1.0, 0.6));
    glare = 0.18 * exp(-glareFalloff / (glareRadius * glareRadius));

    // Add a subtle streak for realism
    glare += 0.18 * exp(-pow((uv.y - 0.13) * 3.5, 2.0)) * exp(-pow((uv.x - 0.32) * 1.5, 2.0));

    // Blend glare with color, using additive blend for highlight
    color += glare;
    
    return float4(color, 1.0);
}

technique CRT
{
    pass
    {
        PixelShader = compile ps_3_0 main();
    }
}