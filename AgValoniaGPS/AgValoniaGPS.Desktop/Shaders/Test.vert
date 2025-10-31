#version 300 es
precision highp float;

layout(location = 0) in vec2 aPosition;

void main()
{
    // Directly output to clip space - no matrices!
    gl_Position = vec4(aPosition, 0.0, 1.0);
}
