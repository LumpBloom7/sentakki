#include "sh_Utils.h"

varying mediump vec2 v_TexCoord;
varying mediump vec4 v_TexRect;

void main(void)
{
    float DistanceToLine = mod((v_TexCoord.x+v_TexCoord.y) / (v_TexRect[2] - v_TexRect[0]), 0.3);
    bool pixelLit = DistanceToLine < 0.15;
    gl_FragColor = vec4(1,1,1,pixelLit ? 1 : 0);
}
