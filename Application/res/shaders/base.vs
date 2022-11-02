#version 330

in vec3 vertexPosition;
in vec4 vertexColor;

uniform mat4 matModel;
uniform mat4 matView;
uniform mat4 matProjection;

out vec4 fragColor;

void main()
{
    fragColor = vertexColor;
    
    mat4 mvp = matProjection * matView * matModel;
    gl_Position = mvp*vec4(vertexPosition, 1.0);
}