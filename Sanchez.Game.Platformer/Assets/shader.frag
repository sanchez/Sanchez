#version 330

out vec4 outputColor;

in vec2 texCoord;

uniform sampler2D ourTexture;

void main (void) 
{
    outputColor = texture(ourTexture, texCoord);
//    outputColor = vec4(ourColor, 1.0);
}