#ifndef SENTAKKI_CHEVRON_FS
#define SENTAKKI_CHEVRON_FS

#include "sh_SDFUtils.fs"

layout(std140, set = 0, binding = 0) uniform m_shapeParameters
{
    float thickness;
    vec2 size;
    float shadowRadius;
    bool glow;
    bool fillTriangle;
    bool shadowOnly;
};

float sdTriangle( in vec2 p, in vec2 p0, in vec2 p1, in vec2 p2 )
{
    vec2 e0 = p1-p0, e1 = p2-p1, e2 = p0-p2;
    vec2 v0 = p -p0, v1 = p -p1, v2 = p -p2;
    vec2 pq0 = v0 - e0*clamp( dot(v0,e0)/dot(e0,e0), 0.0, 1.0 );
    vec2 pq1 = v1 - e1*clamp( dot(v1,e1)/dot(e1,e1), 0.0, 1.0 );
    vec2 pq2 = v2 - e2*clamp( dot(v2,e2)/dot(e2,e2), 0.0, 1.0 );
    float s = sign( e0.x*e2.y - e0.y*e2.x );
    vec2 d = min(min(vec2(dot(pq0,pq0), s*(v0.x*e0.y-v0.y*e0.x)),
                     vec2(dot(pq1,pq1), s*(v1.x*e1.y-v1.y*e1.x))),
                     vec2(dot(pq2,pq2), s*(v2.x*e2.y-v2.y*e2.x)));
    return -sqrt(d.x)*sign(d.y);
}

float triangle(in vec2 p, in vec2 centre, in vec2 size){    

    p -= centre;

    float w = size.x * 0.5;
    float h = size.y * 0.5;
    vec2 p0 = vec2(-w, -h);
    vec2 p1 = vec2(0.0, h);
    vec2 p2 = vec2(w, -h);
    float dist = sdTriangle(p, p0,p1,p2);

    return dist;
}

vec4 sdfShadow(in float dist, in float borderThickness, in float shadowThickness){
    vec3 shadowColor =  glow ? vec3(1) : vec3(0);
    float shadowAlpha = glow ? 0.75: 0.6;

    float shadowDist = dist - borderThickness;

    float shadow =  pow((1 - clamp(((1 / shadowThickness) * shadowDist), 0.0 , 1.0)) * shadowAlpha, 2.0);
    float exclusion = smoothstep(borderThickness, borderThickness - 1.0, dist); // Inner cutout for shadow

    vec4 shadowPart = vec4(shadowColor,shadow) * (1 - exclusion) * v_Colour;
    return shadowPart;
}


void main(void) {
    vec2 resolution = v_TexRect.zw - v_TexRect.xy;
    vec2 pixelPos = (v_TexCoord - v_TexRect.xy) / resolution;

    vec2 p = pixelPos * size;
    vec2 c = 0.5 * size;

    float shadeRadius = shadowRadius;
    float borderThickness = thickness;
    float paddingAmount = - borderThickness - shadeRadius;

    vec2 newSize=  size + paddingAmount * 2;

    float ringSDF = triangle(p, c, newSize);

    if(shadowOnly)
        o_Colour = sdfShadow(ringSDF, borderThickness, shadeRadius);
    else if(fillTriangle) 
        o_Colour = sdfFill(ringSDF, borderThickness, shadeRadius, glow);
    else
        o_Colour = sdfToShape(ringSDF, borderThickness, shadeRadius, glow);
}
#endif