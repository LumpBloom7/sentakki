#ifndef SENTAKKI_HEX_NOTE_FS
#define SENTAKKI_HEX_NOTE_FS

#include "sh_noteBase.fs"

float hexSDF(in vec2 p, in vec2 origin, in float h, in float r){
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

    float hex = hexSDF(p, c, h, radius);
    float dotDown = circleSDF(p, c + vec2(0, h * 0.5), borderThickness/4 - 1.5 );
    float dotUp =  circleSDF(p, c -vec2(0, h * 0.5), borderThickness/4 - 1.5);

    vec4 dotDownShape = sdfToShape(dotDown, borderThickness, 0);
    vec4 dotUpShape = sdfToShape(dotUp, borderThickness, 0);

    vec4 r2 = max(dotDownShape - dotUpShape, vec4(0,0,0,0)) + dotUpShape;

    vec4 r = sdfToShape(hex, borderThickness, shadeRadius) + r2;

    o_Colour = r;
}

#endif