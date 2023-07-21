#version 460 core

in GS_FS_INTERFACE
{
	vec3 _color;
};
out vec4 color;
void main() 
	{
	    color.xyz = _color;
		color.w = 1.0;
	}
