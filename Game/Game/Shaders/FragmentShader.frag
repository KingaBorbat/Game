#version 330 core
out vec4 FragColor;

uniform vec3 uLightColor;
uniform vec3 uLightPos;
uniform vec3 uViewPos;

uniform vec3 uAmbientStrength; 
uniform vec3 uDiffuseStrength; 
uniform vec3 uSpecularStrength;

uniform float uShininess;

uniform sampler2D uTexture;
uniform bool uUseTexture;
uniform vec3 uEmissiveColor;
uniform bool uUseEmissive;
		
in vec4 outCol;
in vec3 outNormal;
in vec3 outWorldPosition;
in vec2 outTex;

void main()
{
    vec3 ambient = uAmbientStrength * uLightColor;

    vec3 norm = normalize(outNormal);
    vec3 lightDir = normalize(uLightPos - outWorldPosition);
    float diff = max(dot(norm, lightDir), 0.0);
    vec3 diffuse = diff * uLightColor * uDiffuseStrength;

    vec3 viewDir = normalize(uViewPos - outWorldPosition);
    vec3 reflectDir = reflect(-lightDir, norm);
    float spec = pow(max(dot(viewDir, reflectDir), 0.0), uShininess);
    vec3 specular = spec * uSpecularStrength * uLightColor;

    
    vec4 baseColor = uUseTexture ? texture(uTexture, outTex) : outCol;
    vec3 lighting = (ambient + diffuse + specular) * baseColor.rgb;
    vec3 emissive = uUseEmissive ? uEmissiveColor : vec3(0.0);
    vec3 result = lighting + emissive;
    FragColor = vec4(result, baseColor.w);
}