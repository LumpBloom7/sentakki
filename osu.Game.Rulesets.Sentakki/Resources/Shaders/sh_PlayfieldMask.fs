varying highp vec2 v_Position;
varying lowp vec4 v_Colour;

uniform highp vec2 aperturePos;
uniform highp vec2 apertureSize;

const mediump float smoothness = 1.5;

// highp precision is necessary for vertex positions to prevent catastrophic failure on GL_ES platforms
lowp vec4 getColourAt(highp vec2 diff, highp vec2 size, lowp vec4 originalColour)
{
    highp float dist = length(diff);
    highp float flashlightRadius = length(size);

    return originalColour * vec4(1.0, 1.0, 1.0, smoothstep(flashlightRadius, flashlightRadius * smoothness, dist));
}

void main(void)
{
    gl_FragColor = mix(getColourAt(aperturePos - v_Position, apertureSize, v_Colour), vec4(0, 0.0, 0, 1.0), 0);
}
