#version 460 core
layout (lines, invocations = 1) in;
layout (line_strip, max_vertices = 203) out;

uniform mat4 VPs[4];
uniform vec3 target;
layout (rgba32f, binding = 0) uniform  image2D objData;
layout (rgba32f, binding = 1) uniform  image2D posTimeData;
in VS_GS_INTERFACE
{
	float ind;
}vs_out[];


void main() 
{
	gl_ViewportIndex = gl_InvocationID;

	ivec2 curP1 = ivec2(0,int(vs_out[0].ind));
	
	vec4 curPos =vec4(imageLoad(objData,curP1).rgb, 1.0);
	curPos.a = imageLoad(posTimeData,curP1).a;
	

	ivec2 curP2 = ivec2(1,int(vs_out[0].ind));
	vec4 curPos2= imageLoad(posTimeData,curP2);

	if(curPos2.a >3 )
	{
	   curPos2.a = 0;
	   imageStore(posTimeData, curP2, curPos2);

	   curP1.x = int(curPos.a);
		if(curP1.x >199)
		{
		   curPos.a = 0;
		}
		imageStore(posTimeData, curP1, curPos);
		curPos.a+=1;
		curP1.x = 0;
		imageStore(posTimeData, curP1, curPos);																								
	}
	else
	{
	  curPos2.a+=1;
	  imageStore(posTimeData, curP2, curPos2);
	}

	for (int i = int(curPos.a); i < 199 ; i++)
		{ 		
			ivec2 curP = ivec2(i,int(vs_out[0].ind));
			gl_Position =VPs[0]* vec4(imageLoad(posTimeData,curP).rgb-target, 1.0);
			EmitVertex();		
		}
		for (int i = 1; i < int(curPos.a) ; i++)
		{ 		
			ivec2 curP = ivec2(i,int(vs_out[0].ind));
			gl_Position =VPs[0]* vec4(imageLoad(posTimeData,curP).rgb-target, 1.0);
			EmitVertex();	

		}
		EndPrimitive();	
}
