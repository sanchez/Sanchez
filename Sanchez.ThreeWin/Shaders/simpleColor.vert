#version 330

in vec3 aPos;
in vec3 aColor;

out vec3 ourColor;

uniform mat4 transform;
uniform mat4 view;
uniform mat4 projection;

void main(void)
{
//    gl_Position = projection * view * transform * vec4(aPos, 1.0);
    gl_Position = vec4(aPos, 1.0) * transform * view * projection;
    ourColor = aColor;
}