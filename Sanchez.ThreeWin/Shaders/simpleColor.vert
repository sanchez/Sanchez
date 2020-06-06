#version 330

in vec3 aPos;
in vec3 aColor;
in vec3 aNormal;

out vec3 ourColor;
out vec3 ourNormal;
out vec3 ourPos;

uniform mat4 transform;
uniform mat4 view;
uniform mat4 projection;

void main(void)
{
//    gl_Position = projection * view * transform * vec4(aPos, 1.0);
    gl_Position = vec4(aPos, 1.0) * transform * view * projection;
    ourColor = aColor;
    ourNormal = aNormal;
    ourPos = vec3(vec4(aPos, 1.0) * transform);
}