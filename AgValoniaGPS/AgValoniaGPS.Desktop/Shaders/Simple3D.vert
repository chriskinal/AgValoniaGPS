#version 300 es
precision highp float;

layout(location = 0) in vec3 aPosition;
layout(location = 2) in vec4 aColor;

uniform mat4 uMVP;  // Combined model-view-projection matrix

out vec4 vColor;

void main()
{
    vColor = aColor;
    gl_Position = uMVP * vec4(aPosition, 1.0);
}
