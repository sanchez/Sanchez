﻿#version 330

in vec3 aPos;
in vec2 aTexCoord;

out vec2 texCoord;

uniform mat4 transform;

void main(void)
{
    gl_Position = transform * vec4(aPos, 1.0);
    texCoord = aTexCoord;
}