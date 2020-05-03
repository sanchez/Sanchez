#version 330

in vec3 aPos;

uniform mat4 transform;

void main(void)
{
    gl_Position = transform * vec4(aPos, 1.0);
}