#ifndef SENTAKKI_STAR_NOTE_FS
#define SENTAKKI_STAR_NOTE_FS

#include "sh_SDFUtils.fs"

layout(std140, set = 0, binding = 0) uniform m_shapeParameters
{
    float thickness;
    vec2 size;
    float shadowRadius;
    bool glow;
};

// signed distance to a n-star polygon, with external angle w
float sdStar(in vec2 p, in vec2 origin, in float r, in float n, in float w)
{
    vec2 P = p-origin;
    P.y = -P.y;
    // these 5 lines can be precomputed for a given shape
    //float m = n*(1.0-w) + w*2.0;
    float m = n + w*(2.0-n);
    
    float an = 3.1415927/n;
    float en = 3.1415927/m;
    vec2  racs = r*vec2(cos(an),sin(an));
    vec2   ecs =   vec2(cos(en),sin(en)); // ecs=vec2(0,1) and simplify, for regular polygon,

    // symmetry (optional)
    P.x = abs(P.x);
    
    // reduce to first sector
    float bn = mod(atan(P.x,P.y),2.0*an) - an;
    P = length(P)*vec2(cos(bn),abs(sin(bn)));

    // line sdf
    P -= racs;
    P += ecs*clamp( -dot(P,ecs), 0.0, racs.y/ecs.y);
    return length(P)*sign(P.x);
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

    float star = sdStar(p, c, radius, 5, 0.6);

    vec4 r = sdfToShape(star, borderThickness * 0.75, shadeRadius, glow);

    o_Colour = r;
}

#endif