#ifndef SENTAKKI_RING_FS
#define SENTAKKI_RING_FS

#include "sh_SDFUtils.fs"

layout(std140, set = 0, binding = 0) uniform m_shapeParameters
{
    float borderThickness;
    vec2 size;
    float shadowRadius;
    bool glow;
};

void main(void) {
    vec2 resolution = v_TexRect.zw - v_TexRect.xy;
    vec2 origin = size * 0.5;
    vec2 pixelPos = ((v_TexCoord - v_TexRect.xy) / resolution) * size;

    float radius = min(size.x, size.y) / 2.0;

    // Since our edge effect is centred along the sdf path
    //// each side of the sdf will have the same thickness
    float strokeRadius = borderThickness * 0.5;

    float sdfRadius = radius - strokeRadius - shadowRadius;

    float sdf = circleSDF(pixelPos, origin, sdfRadius);

    vec4 shape = strokeSDF(sdf, strokeRadius);

    vec4 edgeEffect = sdfShadow(sdf, strokeRadius, shadowRadius, glow);

    // We add 1 here to better match o!f's built in edge smoothing
    float dotStrokeRadius = (strokeRadius + 1.0) / 2.0;

    vec4 dotShape = fillSDF(circleSDF(pixelPos, origin, dotStrokeRadius), dotStrokeRadius);

    o_Colour = (dotShape + shape + edgeEffect) * toPremultipliedAlpha(v_Colour);
}

#endif
