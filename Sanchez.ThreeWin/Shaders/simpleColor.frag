#version 330

struct Material {
    vec3 ambient;
    vec3 diffuse;
    vec3 specular;
    float shininess;
};
uniform Material material;

struct PointLight {
    vec3 position;

    float constant;
    float linear;
    float quadratic;

    vec3 ambient;
    vec3 diffuse;
    vec3 specular;
};
#define NR_POINT_LIGHTS 4
uniform PointLight pointLights[NR_POINT_LIGHTS];
uniform vec3 viewPos;

out vec4 outputColor;

in vec3 ourColor;
in vec3 ourNormal;
in vec3 ourPos;

vec3 CalcPointLight(PointLight light, vec3 normal, vec3 fragPos, vec3 viewDir)
{
    vec3 lightDir = normalize(light.position - fragPos);
    float diff = max(dot(normal, lightDir), 0.0);
    vec3 reflectDir = reflect(-lightDir, normal);
    float spec = pow(max(dot(viewDir, reflectDir), 0.0), material.shininess);
    float distance = length(light.position - fragPos);
    float attenuation = 1.0 / (light.constant + light.linear * distance + light.quadratic * (distance * distance));

    vec3 ambient = light.ambient * ourColor;
    vec3 diffuse = light.diffuse * diff * ourColor;
    vec3 specular = light.specular * spec * ourColor;
    ambient *= attenuation;
    diffuse *= attenuation;
    specular *= attenuation;

    return (ambient + diffuse + specular);
}

vec3 calcLight(PointLight light, vec3 normal, vec3 fragPos, vec3 viewDir, float viewDist)
{
    vec3 lightDir = normalize(light.position - fragPos);
    float diff = 1.0 - max(dot(viewDir, lightDir), 0.0);
    float camDiff = max(dot(viewDir, normal), dot(viewDir, -normal));
    float attenuation = light.constant + (light.linear * viewDist) + (light.quadratic * viewDist * viewDist);
    
    vec3 ambient = light.ambient * ourColor * attenuation;
    vec3 reflection = light.diffuse * diff * ourColor * attenuation;
    vec3 directShine = light.specular * pow(camDiff, 2) * attenuation;
    
    return ambient + reflection + directShine;
}

void main(void)
{
    vec3 norm = normalize(ourNormal);
    vec3 viewDir = normalize(viewPos - ourPos);
    float viewDist = 1.0 - (length(viewPos - ourPos) / 1000.0);
    
    vec3 result = vec3(0., 0., 0.);
    for (int i = 0; i < NR_POINT_LIGHTS; i++)
        result += calcLight(pointLights[i], norm, ourPos, viewDir, viewDist);
    
    outputColor = vec4(result, 1.0);
}