#version 330

in vec3 aPos;
in vec2 aTexCoord;

out vec2 ourTex;

uniform mat4 transform;
uniform mat4 view;
uniform mat4 projection;

void main(void)
{
    gl_Position = vec4(aPos, 1.0) * transform * view * projection;
    ourTex = aTexCoord;
}