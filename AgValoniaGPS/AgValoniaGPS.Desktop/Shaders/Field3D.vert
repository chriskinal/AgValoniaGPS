#version 300 es

precision highp float;

// Input vertex attributes
layout(location = 0) in vec3 aPosition;   // 3D position
layout(location = 1) in vec3 aNormal;     // Surface normal for lighting
layout(location = 2) in vec4 aColor;      // Vertex color
layout(location = 3) in vec2 aTexCoord;   // Texture coordinate (optional)

// Uniforms
uniform mat4 uModel;           // Model matrix (object space -> world space)
uniform mat4 uView;            // View matrix (world space -> camera space)
uniform mat4 uProjection;      // Projection matrix (camera space -> clip space)
uniform mat3 uNormalMatrix;    // Normal transformation matrix (transpose(inverse(model)))

// Output to fragment shader
out vec3 vFragPos;       // Fragment position in world space
out vec3 vNormal;        // Normal in world space
out vec4 vColor;         // Vertex color
out vec2 vTexCoord;      // Texture coordinate

void main()
{
    // Transform vertex position to world space
    vec4 worldPos = uModel * vec4(aPosition, 1.0);
    vFragPos = worldPos.xyz;

    // Transform normal to world space
    vNormal = normalize(uNormalMatrix * aNormal);

    // Pass through color and texture coordinate
    vColor = aColor;
    vTexCoord = aTexCoord;

    // Transform to clip space for rendering
    gl_Position = uProjection * uView * worldPos;
}
