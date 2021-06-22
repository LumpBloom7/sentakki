#include "sh_Utils.h"
#include "sh_Masking.h"

varying mediump vec2 v_TexCoord;

void main(void)
{
    float DistanceToLine = mod((v_TexCoord.x+v_TexCoord.y) / (v_TexRect[2] - v_TexRect[0]), 0.3);
    bool pixelLit = DistanceToLine < 0.15;
    gl_FragColor = getRoundedColor( vec4(1,1,1,pixelLit ? 1 : 0), v_TexCoord);
}
