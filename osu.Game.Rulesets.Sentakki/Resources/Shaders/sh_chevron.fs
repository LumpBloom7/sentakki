#ifndef SENTAKKI_CHEVRON_FS
#define SENTAKKI_CHEVRON_FS

#include "sh_noteBase.fs"

layout(std140, set = 1, binding = 0) uniform m_chevParameters
{
    bool fanChevron;
};

const int N = 6;

float sdPolygon( in vec2 p, in vec2 origin, in vec2[N] v )
{
    vec2 P = p-origin;

    const int num = v.length();
    float d = dot(P-v[0],P-v[0]);
    float s = 1.0;
    for( int i=0, j=num-1; i<num; j=i, i++ )
    {
        // distance
        vec2 e = v[j] - v[i];
        vec2 w =    P - v[i];
        vec2 b = w - e*clamp( dot(w,e)/dot(e,e), 0.0, 1.0 );
        d = min( d, dot(b,b) );

        // winding number from http://geomalgorithms.com/a03-_inclusion.html
        bvec3 cond = bvec3( P.y>=v[i].y, 
                            P.y <v[j].y, 
                            e.x*w.y>e.y*w.x );
        if( all(cond) || all(not(cond)) ) s=-s;  
    }
    
    return s*sqrt(d);
}

vec4 sdfFill(in float dist, in float borderThickness, in float shadowThickness){
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

float chev(in vec2 p, in vec2 centre, in vec2 hexSize, in float thickness){    
    float h = hexSize.y*0.5;
    float w = hexSize.x*0.5;

    vec2 v0 = vec2(0, -h);
	vec2 v1 = vec2(w, h - thickness);
	vec2 v2 = vec2(w,  h );
	vec2 v3 = vec2(0.0, -h + thickness);
	vec2 v4 = vec2(-w, h);
	vec2 v5 = vec2(-w, h - thickness);

    // add more points
    vec2[] polygon = vec2[](v0,v1,v2, v3, v4, v5);

    float dist = sdPolygon(p, centre, polygon);

    return dist;
}

#define PI 3.1415926538

float fanChev(in vec2 p, in vec2 centre, in vec2 hexSize, in float thickness){    
    float h = hexSize.y*0.5;
    float w = hexSize.x*0.5;

    vec2 v0 = vec2(0, -h);
	vec2 v1 = vec2(w, h - thickness);

    float rad1 = ((200+2.5)/180.0) * PI;
    float rad2 = ((160-2.5)/180.0) * PI;

	vec2 v2 = vec2(sin(rad1) * thickness, -cos(rad1) * thickness) + v1;
	vec2 v3 = vec2(0.0, -h + thickness);

	vec2 v5 = vec2(-w, h - thickness);
    vec2 v4 = vec2(sin(rad2) * thickness, -cos(rad2) * thickness) + v5;

    // add more points
    vec2[] polygon = vec2[](v0,v1,v2, v3, v4, v5);

    float dist = sdPolygon(p, centre, polygon);

    return dist;
}


void main(void) {
    vec2 resolution = v_TexRect.zw - v_TexRect.xy;
    vec2 pixelPos = (v_TexCoord - v_TexRect.xy) / resolution;

    vec2 p = pixelPos * size;
    vec2 c = 0.5 * size;

    float shadeRadius = shadowRadius;
    float borderThickness = thickness;
    float paddingAmount = - borderThickness - shadeRadius;

    float ringSDF = 0;
    if(fanChevron)
        ringSDF = fanChev(p, c,size - vec2(shadeRadius + borderThickness) * 2, borderThickness);
    else ringSDF = chev(p, c,size - vec2(shadeRadius + borderThickness) * 2, borderThickness);

    vec4 r = sdfFill(ringSDF, borderThickness, shadeRadius);
    o_Colour = r;
}
#endif