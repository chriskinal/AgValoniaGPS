#version 300 es
precision highp float;

in vec4 vColor;
in vec2 vTexCoord;
out vec4 FragColor;

uniform bool uUseTexture;
uniform sampler2D uTexture;

void main()
{
    if (uUseTexture)
    {
        // Sample texture and multiply by vertex color for tinting
        vec4 texColor = texture(uTexture, vTexCoord);
        FragColor = texColor * vColor;
    }
    else
    {
        // Use vertex color only
        FragColor = vColor;
    }
}
