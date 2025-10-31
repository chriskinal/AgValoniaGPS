#version 300 es
precision highp float;

out vec4 FragColor;

void main()
{
    // Always output bright red
    FragColor = vec4(1.0, 0.0, 0.0, 1.0);
}
