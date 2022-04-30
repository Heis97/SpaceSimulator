#version 460 core
layout (triangles, invocations = 1) in;
layout (triangle_strip, max_vertices = 3) out;

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
vec3 Color;
vec2 Texture;
}vs_out[];

out GS_FS_INTERFACE
{
vec3 Position_world;
vec3 Color;
vec3 Normal_camera;
vec3 Normal_world;
vec3 EyeDirection_camera;
vec3 LightDirection_camera;
vec3 LightDirection_world;
vec2 TextureUV;
} fs_in;

void main() 
{
   for (int i = 0; i < gl_in.length(); i++)
   { 
	    gl_ViewportIndex = gl_InvocationID;
        gl_Position = VPs[gl_InvocationID] * vec4(vs_out[i].Position_world, 1.0);

	    fs_in.Position_world = vs_out[i].Position_world;
	    vec3 Position_camera = (Vs[gl_InvocationID] * vec4(vs_out[i].Position_world, 1.0)).xyz;
	    fs_in.EyeDirection_camera = vec3(0,0,0) - Position_camera;
	    vec3 LightPosition_camera = ( Vs[gl_InvocationID] * vec4(LightPosition_world,1)).xyz;
		
		fs_in.LightDirection_world = vs_out[i].Position_world-LightPosition_world;
		fs_in.LightDirection_camera = LightPosition_camera + fs_in.EyeDirection_camera;


		fs_in.Normal_world = vs_out[i].Normal_world;
		fs_in.Normal_camera =(Vs[gl_InvocationID] * vec4(vs_out[i].Normal_world, 1.0)).xyz;
	    fs_in.Color = vs_out[i].Color;
		fs_in.TextureUV = vs_out[i].Texture;
	    EmitVertex();
	}
}
