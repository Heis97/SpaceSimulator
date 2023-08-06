#version 460 core
layout (lines, invocations = 1) in;
layout (line_strip, max_vertices = 203) out;

uniform mat4 VPs[4];
uniform int targetCamInd;
layout (rgba32f, binding = 0) uniform  image2D objdata;
layout (rgba32f, binding = 1) uniform  image2D posTimeData;
layout (rgba32f, binding = 2) uniform  image2D choosedata;
layout (rgba32f, binding = 3) uniform  image2D debugdata;
in VS_GS_INTERFACE
{
	float ind;
}vs_out[];

out GS_FS_INTERFACE
{
	vec3 _color;
};
struct Root
{
	int[10] root_to_zero;
	vec3[10] root_to_zero_offs;
	int root_len;

};
vec4 comp_pos_in_local(Root root, int ind,int ind_local)
{
	vec4 obj = imageLoad(objdata,ivec2(0,ind));
	vec3 loc_pos = obj.xyz;
	int i = 0;
	while( i<root.root_len && ind_local != root.root_to_zero[i] )
	{
		loc_pos-=root.root_to_zero_offs[i];
		i++;
	}
	loc_pos-=root.root_to_zero_offs[i];
	return(vec4(loc_pos,obj.w));
}

vec4 comp_pos_in_local_relat(Root root_dest, int ind_dest, Root root_rel, int ind_rel)
{
	int i_st = 0;
	while(root_dest.root_to_zero[i_st]!=root_rel.root_to_zero[i_st])
	{
		i_st++;
	}
	i_st = 0;
	int ind_local = 0;
	//int ind_local =int(imageLoad(objdata,ivec2(3,i_st)).w);
	vec4 pos_dest_com = comp_pos_in_local(root_dest,i_st,ind_local);
	vec4 pos_rel_com = comp_pos_in_local(root_rel,i_st,ind_local);

	return(pos_rel_com-pos_dest_com);
}

Root comp_root(vec3 pos, int local_ind)
{
	int[10] _root_to_zero = int[](local_ind,-1,-1,-1,-1,-1,-1,-1,-1,-1);
	vec3[10] _root_to_zero_offs = vec3[](pos ,vec3(0),vec3(0),vec3(0),vec3(0),vec3(0),vec3(0),vec3(0),vec3(0),vec3(0));
	int _root_len = 1;
	//imageStore(debugdata,ivec2(0,gl_GlobalInvocationID.y),vec4(local_ind ,pos));
	while(_root_len<10 && _root_to_zero[_root_len-1]!=0)
	{
		_root_to_zero[_root_len] =int(imageLoad(objdata,ivec2(3,_root_to_zero[_root_len-1])).w);
		_root_to_zero_offs[_root_len] = imageLoad(objdata,ivec2(0,_root_to_zero[_root_len-1])).xyz;
		//imageStore(debugdata,ivec2(_root_len,gl_GlobalInvocationID.y),vec4(_root_to_zero[_root_len],_root_to_zero_offs[_root_len]));
		_root_len+=1;		
	}
	//imageStore(debugdata,ivec2(3,gl_GlobalInvocationID.y),vec4(_root_len,_root_len,_root_len,_root_len));
	return (Root(_root_to_zero,_root_to_zero_offs,_root_len));
}

void main() 
{
	gl_ViewportIndex = gl_InvocationID;
	

	ivec2 curP1 = ivec2(0,int(vs_out[0].ind));
	ivec2 curP_cam = ivec2(7,int(vs_out[0].ind));
	

	float select = imageLoad(choosedata,curP1).y;
	

	vec4 curPos =vec4(imageLoad(objdata,curP1).xyz, 1.0);
	curPos.a = imageLoad(posTimeData,curP1).a;

	vec4 curPos_cam =vec4(imageLoad(objdata,curP_cam).rgb, 1.0);
	

	
	_color = vec3(0.8,0,0);
	
	if(select==1)
	{
		_color = vec3(0,0.8,0);
	}


	if(select!=2)
	{
		gl_Position = VPs[0]* vec4(curPos_cam.xyz, 1.0);


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

	//for local orbit
	int ind_center_obj =int(imageLoad(objdata,ivec2(3,int(vs_out[0].ind))).w);
	Root root_cur = comp_root(curPos.xyz,ind_center_obj);

	vec4 pos_cam = imageLoad(objdata,ivec2(0, targetCamInd));
	int ind_loc_cam = int(imageLoad(objdata,ivec2(3, targetCamInd)).w);
	Root root_cam = comp_root(pos_cam.xyz,ind_loc_cam);

	vec3 pos_cur_in_cam = comp_pos_in_local_relat(root_cur, int( vs_out[0].ind), root_cam, targetCamInd).xyz;

	//imageStore(debugdata,ivec2(0,vs_out[0].ind),vec4(pos_cur_in_cam,777));
	//imageStore(debugdata,ivec2(1,vs_out[0].ind),vec4(curPos_cam.xyz,888));
	//------------------------------------------------------------------------------------------------------------------
	for (int i = int(curPos.a); i < 199 ; i++)
	{ 		
		ivec2 curP_c = ivec2(i,int(vs_out[0].ind));
		vec3 pos_cam_off = imageLoad(posTimeData,curP_c).xyz - curPos.xyz;
		gl_Position =VPs[0]* vec4(pos_cur_in_cam+pos_cam_off , 1.0);
		EmitVertex();		
	}
	for (int i = 1; i < int(curPos.a) ; i++)
	{ 		
		ivec2 curP_c = ivec2(i,int(vs_out[0].ind));
		vec3 pos_cam_off = imageLoad(posTimeData,curP_c).xyz - curPos.xyz;
		gl_Position =VPs[0]* vec4(pos_cur_in_cam+pos_cam_off , 1.0);
		EmitVertex();	
	}	
	EndPrimitive();	
}
