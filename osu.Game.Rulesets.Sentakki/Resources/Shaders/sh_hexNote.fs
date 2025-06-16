#ifndef SENTAKKI_HEX_NOTE_FS
#define SENTAKKI_HEX_NOTE_FS

#include "sh_SDFUtils.fs"

layout(std140, set = 0, binding = 0) uniform m_shapeParameters
{
    float borderThickness;
    vec2 size;
    float shadowRadius;
    bool glow;
};

// SDF that makes a rounded hexagon
// Adapted from the Star shader provided at https://iquilezles.org/articles/distfunctions2d/
float roundedHexSDF(in vec2 p, in vec2 origin, in float h, in float r) {
    vec2 P = p - origin;
    const float n = 6.0; // 6 sided star
    const float w = 1.0; // With no angle

    // these 5 lines can be precomputed for a given shape
    //float m = n*(1.0-w) + w*2.0;
    const float m = n + w * (2.0 - n);

    const float an = 3.1415927 / n;
    const float en = 3.1415927 / m;
    const vec2 ecs = vec2(cos(en), sin(en)); // ecs=vec2(0,1) and simplify, for regular polygon,

    vec2 racs = r * vec2(cos(an), sin(an));

    float halfHeight = h * 0.5;
    float absY = abs(P.y);

    if (absY <= halfHeight)
        return abs(P.x) - racs.x;

    P = vec2(abs(P.x), (absY - halfHeight));

    // reduce to first sector
    float bn = mod(atan(P.x, P.y), 2.0 * an) - an;
    P = length(P) * vec2(cos(bn), abs(sin(bn)));

    // line sdf
    P -= racs;
    P += ecs * clamp(-dot(P, ecs), 0.0, racs.y / ecs.y);
    return length(P) * sign(P.x);
}

void main(void) {
    vec2 resolution = v_TexRect.zw - v_TexRect.xy;
    vec2 origin = size * 0.5;
    vec2 pixelPos = ((v_TexCoord - v_TexRect.xy) / resolution) * size;

    float radius = size.x / 2.0;

    // Since our edge effect is centred along the sdf path
    //// each side of the sdf will have the same thickness
    float strokeRadius = borderThickness * 0.5;

    float sdfRadius = radius - strokeRadius - shadowRadius;
    float sdfHeight = size.y - (shadowRadius + sdfRadius) * 2.0 - borderThickness;

    float sdf = roundedHexSDF(pixelPos, origin, sdfHeight, sdfRadius);

    vec4 shape = strokeSDF(sdf, strokeRadius);

    vec4 edgeEffect = sdfShadow(sdf, strokeRadius, shadowRadius, glow);

    // We add 1 here to better match o!f's built in edge smoothing
    float dotStrokeRadius = (strokeRadius + 1.0) / 2.0;

    vec4 dotShapeUp = fillSDF(circleSDF(pixelPos, origin + vec2(0, sdfHeight * 0.5), dotStrokeRadius), dotStrokeRadius);
    vec4 dotShapeDown = fillSDF(circleSDF(pixelPos, origin - vec2(0, sdfHeight * 0.5), dotStrokeRadius), dotStrokeRadius);

    vec4 dots = max(dotShapeDown - dotShapeUp, vec4(0, 0, 0, 0)) + dotShapeUp;

    o_Colour = (shape + dots + edgeEffect) * toPremultipliedAlpha(v_Colour);
}

#endif
