#ifndef SENTAKKI_RING_FS
#define SENTAKKI_RING_FS

#include "sh_noteBase.fs"

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

    float dotSDF = circleSDF(p,c, borderThickness / 4  - 1.5);
    float ringSDF = circleSDF(p, c, radius);

    vec4 r = sdfToShape(ringSDF, borderThickness, shadeRadius) + sdfToShape(dotSDF, borderThickness, 0);
    o_Colour = r;
}

#endif