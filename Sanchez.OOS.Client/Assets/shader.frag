﻿#version 330

out vec4 outputColor;
in vec3 ourColor;

void main (void)
{
    outputColor = vec4(ourColor, 1.0);
}