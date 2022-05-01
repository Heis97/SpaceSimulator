#version 460 core
layout (lines, invocations = 1) in;
layout (line_strip, max_vertices = 2) out;

uniform vec3 LightPosition_world;
uniform mat4 VPs[4];
uniform mat4 Vs[4];
uniform mat4 Ps[4];
uniform vec2 MouseLoc;
uniform vec2 MouseLocGL;

in VS_GS_INTERFACE
{
vec3 Position_world;
vec3 Normal_world;
}vs_out[];

out GS_FS_INTERFACE
{
vec3 Position_world;
vec3 Normal_camera;
vec3 EyeDirection_camera;
vec3 LightDirection_camera;
} fs_in;

void main() 
{
   for (int i = 0; i < gl_in.length(); i++)
   { 
	    gl_ViewportIndex = gl_InvocationID;
		gl_Position = VPs[gl_InvocationID] * vec4(vs_out[i].Position_world, 1.0);
	    fs_in.Position_world = gl_Position.xyz;
	    EmitVertex();
	}
}
