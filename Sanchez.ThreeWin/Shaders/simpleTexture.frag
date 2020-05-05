#version 330

out vec4 outputColor;

in vec2 ourTex;

uniform sampler2D ourTexture;

void main(void)
{
    outputColor = texture(ourTexture, ourTex);
}