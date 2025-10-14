#ifndef SENTAKKI_NOTE_BASE_FS
#define SENTAKKI_NOTE_BASE_FS

#include "sh_Utils.h"

layout(location = 1) in lowp vec4 v_Colour;
layout(location = 2) in highp vec2 v_TexCoord;
layout(location = 3) in highp vec4 v_TexRect;

// HACK: I use blendRangeOverride to pass in the actual size of the drawable, to avoid using a uniform for it.
layout(location = 4) in mediump vec2 v_DrawSize;

layout(location = 0) out vec4 o_Colour;

vec4 strokeSDF(in float dist, in float strokeRadius) {
    float base = smoothstep(strokeRadius - 1.0, strokeRadius, abs(dist));
    float inner = smoothstep(strokeRadius - 3.0, strokeRadius - 2.0, abs(dist));

    float innerRing = 1.0 - inner;
    float basePlate = (1.0 - base) * (1.0 - innerRing);

    return basePlate * vec4(vec3(0.5), 1.0) + innerRing * vec4(vec3(1.0), 1.0);
}

vec4 fillSDF(in float dist, in float strokeRadius) {
    float base = smoothstep(strokeRadius - 1.0, strokeRadius, dist);
    float inner = smoothstep(strokeRadius - 3.0, strokeRadius - 2.0, dist);

    float innerRing = 1.0 - inner;
    float basePlate = (1.0 - base) * (1.0 - innerRing);

    return basePlate * vec4(vec3(0.5), 1.0) + innerRing * vec4(vec3(1.0), 1.0);
}

vec4 sdfShadow(float dist, float strokeRadius, float shadowThickness, bool glow) {
    vec3 shadowColor = glow ? vec3(0.3) : vec3(0);
    float shadowAlpha = glow ? 0 : 0.3;

    float glow_ = pow(smoothstep(shadowThickness + strokeRadius, strokeRadius - 1.0, dist), 1.0);
    float glowCutOut = smoothstep(strokeRadius - 1.0, strokeRadius, dist);

    glow_ *= glowCutOut;
    return glow_ * vec4(shadowColor, shadowAlpha);
}

float circleSDF(vec2 p, vec2 centre, float radius) {
    return length(p - centre) - radius;
}

vec4 toPremultipliedAlpha(in vec4 straightColour) {
    return straightColour * vec4(vec3(v_Colour.w), 1.0);
}

#endif
