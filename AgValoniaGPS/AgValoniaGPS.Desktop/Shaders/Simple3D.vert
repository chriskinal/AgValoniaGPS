#version 300 es
precision highp float;

layout(location = 0) in vec3 aPosition;
layout(location = 1) in vec3 aNormal;    // Must use to prevent compiler optimization
layout(location = 2) in vec4 aColor;
layout(location = 3) in vec2 aTexCoord;

uniform mat4 uMVP;  // Combined model-view-projection matrix

out vec4 vColor;
out vec2 vTexCoord;

void main()
{
    // Use normal in a way that doesn't affect output but prevents optimization
    // Add a tiny fraction of normal.z to prevent it from being optimized away
    float dummy = aNormal.z * 0.0;
    vColor = aColor + vec4(dummy, dummy, dummy, 0.0);
    vTexCoord = aTexCoord;
    gl_Position = uMVP * vec4(aPosition, 1.0);
}
