#version 460 core

in GS_FS_INTERFACE
{
vec3 Position_world;
vec3 Normal_camera;
vec3 EyeDirection_camera;
vec3 LightDirection_camera;
}fs_in;
out vec4 color;
void main() 
{
	color = vec4 (0.5,0.5,0.5,1);
}