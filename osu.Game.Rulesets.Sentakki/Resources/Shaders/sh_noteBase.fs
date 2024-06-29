#ifndef SENTAKKI_NOTE_BASE_FS
#define SENTAKKI_NOTE_BASE_FS

#include "sh_Utils.h"

layout(location = 1) in lowp vec4 v_Colour;
layout(location = 2) in highp vec2 v_TexCoord;
layout(location = 3) in highp vec4 v_TexRect;

layout(location = 0) out vec4 o_Colour;

layout(std140, set = 0, binding = 0) uniform m_shapeParameters
{
    float thickness;
    vec2 size;
    float shadowRadius;
    bool glow;
};

vec4 sdfToShape(in float dist, in float borderThickness, in float shadowThickness){
    vec3 shadowColor =  glow ? vec3(1) : vec3(0);
    float shadowAlpha = 0.5;

    float base = smoothstep(borderThickness - 2.0, borderThickness - 3.0, abs(dist));
    float outline = smoothstep(borderThickness, borderThickness - 1.0, abs(dist));

    if(shadowThickness < 1)
        return vec4(vec3(max(outline * 0.5, base)), outline) * v_Colour;

    float shadow = pow(smoothstep(shadowThickness + borderThickness, borderThickness - 1.0, dist) * shadowAlpha, 2);
    float exclusion = smoothstep(borderThickness, borderThickness - 1.0, dist); // Inner cutout for shadow

    float innerShading = smoothstep(borderThickness -2.0, 0.0 , abs(dist));

    vec4 shadowPart = vec4(shadowColor,shadow) * (1 - exclusion) * v_Colour;
    vec4 fillPart = vec4(vec3(max(outline * 0.5, base)), outline) * v_Colour;

    vec4 stylizedFill = mix(fillPart, v_Colour * 0.85, innerShading);
    
    return shadowPart + stylizedFill;
}

float circleSDF(in vec2 p, in vec2 centre, in float radius){
    return length(p - centre) - radius;
}

#endif