#ifndef SENTAKKI_CHEVRON_FS
#define SENTAKKI_CHEVRON_FS

#include "sh_SDFUtils.fs"

layout(std140, set = 0, binding = 0) uniform m_shapeParameters
{
    float thickness;
    vec2 size;
    float shadowRadius;
    bool glow;
    bool fanChevron;
};


float sdPolygon( in vec2 p, in vec2 origin, in vec2[6] v )
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

float chev(in vec2 p, in vec2 centre, in vec2 hexSize, in float thickness){    
    float h = hexSize.y * 0.5;
    float w = hexSize.x * 0.5;

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
const float deg2rad_factor = (PI / 180.0);

float fanChev(in vec2 p, in vec2 centre, in vec2 hexSize, in float thickness){    
    float h = hexSize.y * 0.5;
    float w = hexSize.x * 0.5;

    vec2 v0 = vec2(0, -h);
	vec2 v1 = vec2(w, h - thickness);

    // We want to make the chevron edges angled inwards a bit, so they line up better when in a fan formation
    float rad1 = (180 + 22.5) * deg2rad_factor;
    float rad2 = (180 - 22.5) * deg2rad_factor;

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

    vec4 r = sdfFill(ringSDF, borderThickness, shadeRadius, glow);
    o_Colour = r;
}
#endif