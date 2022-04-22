#version 460 core
layout (lines, invocations = 1) in;
layout (line_strip, max_vertices = 4) out;


in VS_GS_INTERFACE
{
vec3 vertexPosition_world[4];
}vs_out[];


void main() 
{
	gl_ViewportIndex = gl_InvocationID;
   for (int i = 0; i < gl_in.length(); i++)
   { 	
			
		gl_Position = vec4(vs_out[0].vertexPosition_world[i], 1.0);
		EmitVertex();
		
	}
}
