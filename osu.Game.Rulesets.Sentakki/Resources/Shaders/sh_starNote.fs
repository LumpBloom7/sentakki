#ifndef SENTAKKI_STAR_NOTE_FS
#define SENTAKKI_STAR_NOTE_FS

#include "sh_SDFUtils.fs"

layout(std140, set = 0, binding = 0) uniform m_shapeParameters
{
    float borderThickness;
    float shadowRadius;
    bool glow;
};

// signed distance to a n-star polygon, with external angle w
float sdStar(in vec2 p, in vec2 origin, in float r, in float n, in float w)
{
    vec2 P = p - origin;
    P.y = -P.y;
    // these 5 lines can be precomputed for a given shape
    //float m = n*(1.0-w) + w*2.0;
    float m = n + w * (2.0 - n);

    float an = 3.1415927 / n;
    float en = 3.1415927 / m;
    vec2 racs = r * vec2(cos(an), sin(an));
    vec2 ecs = vec2(cos(en), sin(en)); // ecs=vec2(0,1) and simplify, for regular polygon,

    // symmetry (optional)
    P.x = abs(P.x);

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
    vec2 origin = v_DrawSize * 0.5;
    vec2 pixelPos = ((v_TexCoord - v_TexRect.xy) / resolution) * v_DrawSize;

    float radius = min(v_DrawSize.x, v_DrawSize.y) / 2.0;

    // Fixed modifier to make it look right when put with other notes with the same thickness
    float adjustedBorderThickness = borderThickness * 0.75;

    // Since our edge effect is centred along the sdf path
    //// each side of the sdf will have the same thickness
    float strokeRadius = adjustedBorderThickness * 0.5;

    float sdfRadius = radius - strokeRadius - shadowRadius;

    float sdf = sdStar(pixelPos, origin, sdfRadius, 5.0, 0.6);

    vec4 shape = strokeSDF(sdf, strokeRadius);
    vec4 edgeEffect = sdfShadow(sdf, strokeRadius, shadowRadius, glow);

    o_Colour = (shape + edgeEffect) * toPremultipliedAlpha(v_Colour);
}

#endif
