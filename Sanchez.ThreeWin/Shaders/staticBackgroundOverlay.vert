#version 330

in vec2 aPos;
in vec3 aColor;

out vec3 ourColor;

void main(void)
{
    gl_Position = vec4(aPos, 0.0, 1.0);
    ourColor = aColor;
}