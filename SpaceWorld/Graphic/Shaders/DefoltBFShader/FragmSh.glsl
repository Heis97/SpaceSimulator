#version 460 core

in VS_FS_INTERFACE
{
vec3 Color;
}fs_in;

out vec4 color;
void main() 
{
	    color.xyz = fs_in.Color;
		color.w = 1.0;
}