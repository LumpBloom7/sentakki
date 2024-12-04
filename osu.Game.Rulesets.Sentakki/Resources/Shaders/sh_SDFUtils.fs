#ifndef SENTAKKI_NOTE_BASE_FS
#define SENTAKKI_NOTE_BASE_FS

#include "sh_Utils.h"

layout(location = 1) in lowp vec4 v_Colour;
layout(location = 2) in highp vec2 v_TexCoord;
layout(location = 3) in highp vec4 v_TexRect;

layout(location = 0) out vec4 o_Colour;

vec4 sdfToShape(in float dist, in float borderThickness, in float shadowThickness, in bool glow){
    vec3 shadowColor =  glow ? vec3(1) : vec3(0);
    float shadowAlpha = glow ? 0.75: 0.6;

    float base = smoothstep(borderThickness - 2.0, borderThickness - 3.0, abs(dist));
    float outline = smoothstep(borderThickness, borderThickness - 1.0, abs(dist));

    if(shadowThickness < 1)
        return vec4(vec3(max(outline * 0.5, base)), outline) * v_Colour;

    float shadowDist = dist - borderThickness;

    float shadow =  pow((1 - clamp(((1 / shadowThickness) * shadowDist), 0.0 , 1.0)) * shadowAlpha, 2.0);
    float exclusion = smoothstep(borderThickness, borderThickness - 1.0, dist); // Inner cutout for shadow

    float innerShading = smoothstep(borderThickness -2.0, 0.0 , abs(dist));

    vec4 shadowPart = vec4(shadowColor,shadow) * (1 - exclusion) * v_Colour;
    vec4 fillPart = vec4(vec3(max(outline * 0.5, base)), outline) * v_Colour;

    //vec4 stylizedFill = mix(fillPart, v_Colour * 0.85, innerShading);
    
    return shadowPart + fillPart;
}

vec4 sdfFill(in float dist, in float borderThickness, in float shadowThickness, in bool glow){
    vec3 shadowColor =  glow ? vec3(1) : vec3(0);
    float shadowAlpha = glow ? 0.75: 0.6;

    float base = smoothstep(borderThickness - 2.0, borderThickness - 3.0, dist);
    float outline = smoothstep(borderThickness, borderThickness - 1.0, dist);

    if(shadowThickness < 1)
        return vec4(vec3(max(outline * 0.5, base)), outline) * v_Colour;

    float shadowDist = dist - borderThickness;

    float shadow =  pow((1 - clamp(((1 / shadowThickness) * shadowDist), 0.0 , 1.0)) * shadowAlpha, 2.0);
    float exclusion = smoothstep(borderThickness, borderThickness - 1.0, dist); // Inner cutout for shadow

    float innerShading = smoothstep(borderThickness -2.0, 0.0 , dist);

    vec4 shadowPart = vec4(shadowColor,shadow) * (1 - exclusion) * v_Colour;
    vec4 fillPart = vec4(vec3(max(outline * 0.5, base)), outline) * v_Colour;

    //vec4 stylizedFill = mix(fillPart, v_Colour * 0.85, innerShading);
    
    return shadowPart + fillPart;
}


float circleSDF(in vec2 p, in vec2 centre, in float radius){
    return length(p - centre) - radius;
}

#endif