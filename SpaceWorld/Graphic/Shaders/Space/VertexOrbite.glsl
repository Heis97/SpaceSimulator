#version 460 core

layout(location = 0) in vec3 pos1;
layout(location = 1) in vec3 pos2;
layout(location = 2) in vec3 pos3;
layout(location = 3) in vec3 pos4;
//layout(location = 4) in mat4 _ModelMatrix;
uniform mat4 VPs[4];

out VS_GS_INTERFACE
{
vec3 vertexPosition_world[4];

} vs_out;

void main() 
{
	vs_out.vertexPosition_world[0] = (VPs[0]* vec4(pos1,1)).xyz;
	vs_out.vertexPosition_world[1] = (VPs[0]* vec4(pos2,1)).xyz;
	vs_out.vertexPosition_world[2] = (VPs[0]* vec4(pos3,1)).xyz;
	vs_out.vertexPosition_world[3] = (VPs[0]* vec4(pos4,1)).xyz;
	gl_Position = vec4(vs_out.vertexPosition_world[0],1);

}