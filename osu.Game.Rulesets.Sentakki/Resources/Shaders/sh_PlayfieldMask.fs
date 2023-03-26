layout(location = 0) in highp vec2 v_Position;
layout(location = 1) in lowp vec4 v_Colour;

layout(std140, set = 0, binding = 0) uniform m_maskParameters
{
    highp vec2 maskPosition;
    highp vec2 maskRadius;
};

const mediump float smoothness = 2.0;

layout(location = 0) out vec4 o_Colour;

// highp precision is necessary for vertex positions to prevent catastrophic failure on GL_ES platforms
lowp vec4 getColourAt(highp vec2 diff, highp vec2 size, lowp vec4 originalColour)
{
    highp float dist = length(diff);
    highp float fullVisibilityRadius = length(size);

    return originalColour * vec4(1.0, 1.0, 1.0, smoothstep(fullVisibilityRadius, fullVisibilityRadius * smoothness, dist));
}

void main(void)
{
    o_Colour = mix(getColourAt(maskPosition - v_Position, maskRadius, v_Colour), vec4(0.0, 0.0, 0.0, 1.0), 0.0);
}
