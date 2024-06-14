#ifndef SENTAKKI_RING_FS
#define SENTAKKI_RING_FS

#include "sh_Utils.h"

layout(location = 1) in lowp vec4 v_Colour;
layout(location = 2) in highp vec2 v_TexCoord;
layout(location = 3) in highp vec4 v_TexRect;

layout(location = 0) out vec4 o_Colour;

layout(std140, set = 0, binding = 0) uniform m_shapeParameters
{
    bool hexagon;
    float thickness;
    vec2 size;
    float shadowRadius;
    bool glow;
    //vec4 accentColor;
};

float distanceHex(in vec2 p, in vec2 origin, in float h, in float r){
    const float Sin =  sqrt(3.0) * 0.5;
    const float Cos = .5;

    float sinR =  Sin * r;
    float cosR = Cos * r;
    
    vec2 P = p - origin;
    
    float absYDist = abs(P.y);
    float absXDist = abs(P.x);
    
    float rhs = h / 2.0 
                + (cosR) 
                + (Cos/Sin) * (absXDist - sinR);

    float factor = step(rhs,absYDist);
    
    float a = absXDist 
                - sinR 
                + (Sin / (2 * Cos)) 
                    * (absYDist 
                        - (h/2.0) 
                        - (Cos * r) 
                        - (Cos / Sin) * (absXDist - sinR));

    float b = absXDist - sinR;
        
    return (a * factor) + b * (1.0-factor);
}

float rand(in vec2 co){
    return fract(sin(dot(co, vec2(12.9898, 78.233))) * 43758.5453);
}


vec4 hex(in vec2 p, in vec2 centre,in float radius, in float borderThickness, in float shadowThickness){
    float h = size.y - size.x;
    float dist = distanceHex(p, centre, h, radius);

    vec3 shadowColor = glow ? vec3(1) : vec3(0);
    float shadowAlpha = glow ? 0.5 : 0.5;

    float shadow = pow(smoothstep(shadowThickness + borderThickness, borderThickness, dist) * shadowAlpha, 2);
    float exclusion = smoothstep(borderThickness, borderThickness - 1.0, dist);
    float outline = smoothstep(borderThickness, borderThickness - 1.0, abs(dist));
    float base = smoothstep(borderThickness - 2.0, borderThickness - 3.0, abs(dist));
    float innerBorder = smoothstep(borderThickness -2.0, 0.0 , abs(dist)) * 0.15;

    return vec4(shadowColor,shadow) * (1 - exclusion) + vec4(vec3(max(outline * 0.5, base)), outline);
}
vec4 circle(in vec2 p, in vec2 centre, in float radius){
    float dist = length(p - centre);

    float outline = smoothstep(radius , radius -1, abs(dist));
    float base = smoothstep(radius - 2.0, radius - 3, abs(dist));
    float innerShader = smoothstep(radius - 2, 0.0, abs(dist))*0.15; // Optional inner shader

    return vec4(vec3(min(base + outline*0.5, 1.0)), outline);
}

vec4 ring(in vec2 p, in vec2 centre, in highp float radius, in float borderThickness, in float shadowThickness){
    float dist = length(p - centre) - radius;
    
    vec3 shadowColor = glow ? vec3(1) : vec3(0);
    float shadowAlpha = glow ? 0.5 : 0.5;

    float shadow = pow(smoothstep(shadowThickness + borderThickness, borderThickness, dist) * shadowAlpha, 2);
    float exclusion = smoothstep(borderThickness, borderThickness - 1.0, dist);
    float outline = smoothstep(borderThickness, borderThickness - 1.0, abs(dist));
    float base = smoothstep(borderThickness - 2.0, borderThickness - 3.0, abs(dist));
    float innerBorder = smoothstep(borderThickness -2.0, 0.0 , abs(dist)) * 0.15;

    return vec4(shadowColor,shadow) * (1 - exclusion) + vec4(vec3(max(outline * 0.5, base)), outline);
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

    if(hexagon){
        float h = size.y - size.x;
        vec4 r = hex(p, c, radius, borderThickness , shadeRadius) + circle(p, c+vec2(0, h * 0.5), borderThickness+1);

        vec4 circle2 = circle(p, c -vec2(0, h * 0.5), borderThickness+1);

        r -= circle2;
        r = max(r, circle2);

        o_Colour = r * v_Colour;
        
    }
    else{
        vec4 r = ring(p, c, radius, borderThickness, shadeRadius)+ circle(p,c, borderThickness+1);
        o_Colour = r * v_Colour;
    } 
}

#endif