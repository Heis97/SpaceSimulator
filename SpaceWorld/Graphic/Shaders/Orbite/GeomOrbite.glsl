#version 460 core
layout (lines, invocations = 1) in;
layout (line_strip, max_vertices = 203) out;

uniform mat4 VPs[4];
uniform int targetCamInd;
layout (rgba32f, binding = 0) uniform  image2D objData;
layout (rgba32f, binding = 1) uniform  image2D posTimeData;
layout (rgba32f, binding = 2) uniform  image2D choosedata;
in VS_GS_INTERFACE
{
	float ind;
}vs_out[];


void main() 
{
	gl_ViewportIndex = gl_InvocationID;
	

	ivec2 curP1 = ivec2(0,int(vs_out[0].ind));
	

	float select = imageLoad(choosedata,curP1).y;
	

	vec4 curPos =vec4(imageLoad(objData,curP1).rgb, 1.0);
	curPos.a = imageLoad(posTimeData,curP1).a;


	int ind_center_obj =int(imageLoad(objData,ivec2(3,int(vs_out[0].ind))).w);

	vec3 pos_center_obj = vec3(0);
	if(ind_center_obj!=int(vs_out[0].ind))
	{
		pos_center_obj = imageLoad(
		objData,ivec2(0,ind_center_obj)
		).xyz;
	}
	

	vec3 targetC = imageLoad(
	objData, ivec2(0, targetCamInd)
	).xyz;


	if(select!=2)
	{
		gl_Position = VPs[0]* vec4(curPos.xyz-targetC, 1.0);


		gl_Position.x+=0.01;
		EmitVertex();

		gl_Position.y+=0.01;
		gl_Position.x-=0.01;
		EmitVertex();

		gl_Position.y-=0.01;
		gl_Position.x-=0.01;
		EmitVertex();

		gl_Position.y-=0.01;
		gl_Position.x+=0.01;
		EmitVertex();

		gl_Position.y+=0.01;
		gl_Position.x+=0.01;
		EmitVertex();
		EndPrimitive();	
	}

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
		ivec2 curP_c = ivec2(i,int(ind_center_obj));
		gl_Position =VPs[0]* vec4(imageLoad(posTimeData,curP).rgb-targetC-imageLoad(posTimeData,curP_c).rgb+pos_center_obj , 1.0);
		EmitVertex();		
	}
	for (int i = 1; i < int(curPos.a) ; i++)
	{ 		
		ivec2 curP = ivec2(i,int(vs_out[0].ind));
		ivec2 curP_c = ivec2(i,int(ind_center_obj));
		gl_Position =VPs[0]* vec4(imageLoad(posTimeData,curP).rgb-targetC-imageLoad(posTimeData,curP_c).rgb+pos_center_obj , 1.0);
		EmitVertex();	
	}	
	EndPrimitive();	
}
