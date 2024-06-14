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
    highp float thickness;
    highp vec2 size;
    highp float shadowRadius;
    bool glow;
    //vec4 accentColor;
};

highp float distanceHex(in vec2 p, in vec2 origin, in float h, in float r){
    const highp float Sin =  sqrt(3.0) * 0.5;
    const highp float Cos = .5;

    highp  float sinR =  Sin * r;
    highp float cosR = Cos * r;
    
    highp vec2 P = p - origin;
    
    highp float absYDist = abs(P.y);
    highp float absXDist = abs(P.x);
    
    highp float rhs = h / 2.0 
                + (cosR) 
                + (Cos/Sin) * (absXDist - sinR);


    highp float factor = step(rhs,absYDist);
    
    highp float a = absXDist 
                - sinR 
                + (Sin / (2 * Cos)) 
                    * (absYDist 
                        - (h/2.0) 
                        - (Cos * r) 
                        - (Cos / Sin) * (absXDist - sinR));

    highp float b = absXDist - sinR;
        
    return (a * factor) + b * (1.0-factor);
}

float rand(in vec2 co){
    return fract(sin(dot(co, vec2(12.9898, 78.233))) * 43758.5453);
}


highp vec4 hex(in vec2 p, in vec2 centre,in float radius, in float borderThickness, in float shadowThickness){
    highp float h = size.y - size.x;
    highp float dist = distanceHex(p, centre, h, radius);

    highp vec3 shadowColor = glow ? vec3(1) : vec3(0);
    highp float shadowAlpha = glow ? 0.5 : 0.5;

    highp float shadow = pow(smoothstep(shadowThickness + borderThickness, borderThickness, dist) * shadowAlpha, 2);
    highp float exclusion = smoothstep(borderThickness, borderThickness - 1.0, dist);
    highp float outline = smoothstep(borderThickness, borderThickness - 1.0, abs(dist));
    highp float base = smoothstep(borderThickness - 2.0, borderThickness - 3.0, abs(dist));
    highp float innerBorder = smoothstep(borderThickness -2.0, 0.0 , abs(dist)) * 0.15;

    return vec4(shadowColor,shadow) * (1 - exclusion) + vec4(vec3(max(outline * 0.5, base)), outline);
}
vec4 circle(in vec2 p, in vec2 centre, in float radius){
    float dist = length(p - centre);

    float outline = smoothstep(radius , radius -1, abs(dist));
    float base = smoothstep(radius - 2.0, radius - 3, abs(dist));
    float innerShader = smoothstep(radius - 2, 0.0, abs(dist))*0.15; // Optional inner shader

    return vec4(vec3(min(base + outline*0.5, 1.0)), outline);
}

highp vec4 ring(highp vec2 p, highp vec2 centre, highp float radius,  highp float borderThickness, in float shadowThickness){
    float dist = length(p - centre) - radius;
    
    highp vec3 shadowColor = glow ? vec3(1) : vec3(0);
    highp float shadowAlpha = glow ? 0.5 : 0.5;

    highp float shadow = pow(smoothstep(shadowThickness + borderThickness, borderThickness, dist) * shadowAlpha, 2);
    highp float exclusion = smoothstep(borderThickness, borderThickness - 1.0, dist);
    highp float outline = smoothstep(borderThickness, borderThickness - 1.0, abs(dist));
    highp float base = smoothstep(borderThickness - 2.0, borderThickness - 3.0, abs(dist));
    highp float innerBorder = smoothstep(borderThickness -2.0, 0.0 , abs(dist)) * 0.15;

    return vec4(shadowColor,shadow) * (1 - exclusion) + vec4(vec3(max(outline * 0.5, base)), outline);
}
/*
void main(void) 
{
    vec2 wrappedCoord = wrap(v_TexCoord, v_TexRect);
    o_Colour = getRoundedColor(wrappedSampler(wrappedCoord, v_TexRect, m_Texture, m_Sampler, -0.9), wrappedCoord);
}*/

void main(void) {
    highp vec2 resolution = v_TexRect.zw - v_TexRect.xy;
    highp vec2 pixelPos = (v_TexCoord - v_TexRect.xy) / resolution;

    highp vec2 p = pixelPos * size;
    highp vec2 c = 0.5 * size;

    highp float shadeRadius = size.x * shadowRadius;
    highp float noteW = size.x - shadeRadius * 2;
    highp float borderThickness = thickness * 0.5 * noteW;
    highp float paddingAmount = - borderThickness - shadeRadius;

    highp float radius = size.x * 0.5 + paddingAmount;

    if(hexagon){
        highp float h = size.y - size.x;
        highp vec4 r = hex(p, c, radius, borderThickness , shadeRadius) + circle(p, c+vec2(0, h * 0.5), borderThickness+1);

        vec4 circle2 = circle(p, c -vec2(0, h * 0.5), borderThickness+1);

        r -= circle2;
        r = max(r, circle2);

        o_Colour = r * v_Colour;
        
    }
    else{
        highp vec4 r = ring(p, c, radius, borderThickness, shadeRadius)+ circle(p,c, borderThickness+1);
        o_Colour = r * v_Colour;
    } 
}

#endif