#version 330 core

// Input from vertex shader
in vec3 vFragPos;       // Fragment position in world space
in vec3 vNormal;        // Normal in world space
in vec4 vColor;         // Vertex color
in vec2 vTexCoord;      // Texture coordinate

// Uniforms
uniform vec3 uLightDir;          // Directional light direction (normalized)
uniform vec3 uLightColor;        // Light color
uniform vec3 uAmbientLight;      // Ambient light color
uniform vec3 uCameraPos;         // Camera position in world space
uniform bool uUseTexture;        // Whether to use texture
uniform sampler2D uTexture;      // Texture sampler

// Output
out vec4 FragColor;

void main()
{
    // Base color from vertex color or texture
    vec4 baseColor = vColor;
    if (uUseTexture)
    {
        baseColor = texture(uTexture, vTexCoord) * vColor;
    }

    // Normalize the normal (interpolation may have changed its length)
    vec3 normal = normalize(vNormal);

    // Ambient lighting
    vec3 ambient = uAmbientLight * baseColor.rgb;

    // Diffuse lighting (Lambertian)
    float diff = max(dot(normal, uLightDir), 0.0);
    vec3 diffuse = diff * uLightColor * baseColor.rgb;

    // Specular lighting (Blinn-Phong)
    vec3 viewDir = normalize(uCameraPos - vFragPos);
    vec3 halfwayDir = normalize(uLightDir + viewDir);
    float spec = pow(max(dot(normal, halfwayDir), 0.0), 32.0); // Shininess = 32
    vec3 specular = spec * uLightColor * 0.3; // Specular intensity = 0.3

    // Combine lighting components
    vec3 result = ambient + diffuse + specular;

    // Output final color with alpha
    FragColor = vec4(result, baseColor.a);
}
