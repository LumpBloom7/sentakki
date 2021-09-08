varying highp vec2 v_Position;
varying lowp vec4 v_Colour;

uniform highp vec2 maskPosition;
uniform highp vec2 maskRadius;

const mediump float smoothness = 2.0;

// highp precision is necessary for vertex positions to prevent catastrophic failure on GL_ES platforms
lowp vec4 getColourAt(highp vec2 diff, highp vec2 size, lowp vec4 originalColour)
{
    highp float dist = length(diff);
    highp float fullVisibilityRadius = length(size);

    return originalColour * vec4(1.0, 1.0, 1.0, smoothstep(fullVisibilityRadius, fullVisibilityRadius * smoothness, dist));
}

void main(void)
{
    gl_FragColor = mix(getColourAt(maskPosition - v_Position, maskRadius, v_Colour), vec4(0.0, 0.0, 0.0, 1.0), 0.0);
}
