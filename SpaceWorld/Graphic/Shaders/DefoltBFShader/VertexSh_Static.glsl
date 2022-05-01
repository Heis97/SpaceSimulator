#version 460 core

layout(location = 0) in vec3 _Position_model;
layout(location = 2) in vec3 _Color;

uniform mat4 VPs[4];
uniform mat4 Vs[4];
uniform mat4 Ps[4];
uniform vec2 MouseLoc;
uniform vec2 MouseLocGL;
uniform vec3 targetCam;

out VS_FS_INTERFACE
{
vec3 Color;
} vs_out;

mat4 modelMatr()
{
	return(mat4( 
	1,0,0,0,
	0,1,0,0,
	0,0,1,0,
	vec4(-targetCam,1)));
}

void main() 
{
	gl_Position = VPs[0] *modelMatr()*  vec4(_Position_model,1);
	vs_out.Color = _Color;
}