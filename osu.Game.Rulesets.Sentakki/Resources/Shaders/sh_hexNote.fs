#ifndef SENTAKKI_HEX_NOTE_FS
#define SENTAKKI_HEX_NOTE_FS

#include "sh_SDFUtils.fs"

layout(std140, set = 0, binding = 0) uniform m_shapeParameters
{
    float thickness;
    vec2 size;
    float shadowRadius;
    bool glow;
};

// SDF that makes a rounded hexagon
// Adapted from the Star shader provided at https://iquilezles.org/articles/distfunctions2d/
float roundedHexSDF(in vec2 p, in vec2 origin, in float h, in float r)
{
    vec2 P = p - origin;
    const float n = 6.0; // 6 sided star
    const float w = 1.0; // With no angle

    // these 5 lines can be precomputed for a given shape
    //float m = n*(1.0-w) + w*2.0;
    const float m = n + w * (2.0 - n);
    
    const float an = 3.1415927 / n;
    const float en = 3.1415927 / m;
    const vec2  ecs = vec2(cos(en),sin(en)); // ecs=vec2(0,1) and simplify, for regular polygon,

    vec2 racs = r * vec2(cos(an),sin(an));

    float halfHeight = h * 0.5;
    float absY = abs(P.y);

    if(absY <= halfHeight){
        return abs(P.x) - racs.x;
    }

    P = vec2(abs(P.x), (absY - halfHeight));

    // reduce to first sector
    float bn = mod(atan(P.x,P.y), 2.0 * an) - an;
    P = length(P) * vec2(cos(bn), abs(sin(bn)));

    // line sdf
    P -= racs;
    P += ecs * clamp(-dot(P,ecs), 0.0, racs.y / ecs.y);
    return length(P) * sign(P.x);
}

void main(void) {
    vec2 resolution = v_TexRect.zw - v_TexRect.xy;
    vec2 pixelPos = (v_TexCoord - v_TexRect.xy) / resolution;

    vec2 p = pixelPos * size;
    vec2 c = 0.5 * size;

    float shadeRadius = size.x * shadowRadius;
    float noteW = size.x - shadeRadius * 2;
    float borderThickness = thickness * 0.5 * noteW;
    float paddingAmount = - borderThickness - shadeRadius;

    float radius = size.x * 0.5 + paddingAmount;

    float h = size.y - size.x;

    float hex = roundedHexSDF(p, c, h, radius);
    float dotDown = circleSDF(p, c + vec2(0, h * 0.5), borderThickness/4 - 1.5);
    float dotUp =  circleSDF(p, c -vec2(0, h * 0.5), borderThickness/4 - 1.5);

    vec4 dotDownShape = sdfToShape(dotDown, borderThickness, 0, false);
    vec4 dotUpShape = sdfToShape(dotUp, borderThickness, 0, false);

    vec4 r2 = max(dotDownShape - dotUpShape, vec4(0,0,0,0)) + dotUpShape;

    vec4 r = sdfToShape(hex, borderThickness, shadeRadius, glow) + r2;

    o_Colour = r;
}

#endif