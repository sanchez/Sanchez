#version 330

out vec4 outputColor;

in vec3 ourColor;

uniform float outlineThickness = .2;
uniform vec3 outlineColor = vec3(0, 0, 0);
uniform float outlineThreshold = .5;

void main(void)
{
    outputColor = vec4(ourColor, 1.0);
}