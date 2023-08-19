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
	vec4[10] root_to_zero_offs;
	int root_len;

};
Root load_root(int i,vec4 pos)
{
	vec4 root_inf1 = imageLoad(objdata, ivec2(8,i));
	vec4 root_inf2 = imageLoad(objdata, ivec2(9,i));
	int[10] _root_to_zero = int[](int(root_inf1.z),int(root_inf1.w),int(root_inf2.x),int(root_inf2.y),int(root_inf2.z),int(root_inf2.w),-1,-1,-1,-1);
	vec4[10] _root_to_zero_offs = vec4[](pos ,vec4(0),vec4(0),vec4(0),vec4(0),vec4(0),vec4(0),vec4(0),vec4(0),vec4(0));
	int _root_len = int(root_inf1.y);
	//int ir = 1;
	//imageStore(debugdata,ivec2(0,gl_GlobalInvocationID.y),vec4(local_ind ,pos));
	for(int ir=1;ir<=_root_len&& _root_to_zero[ir-1]!=0;ir++)
	{
		_root_to_zero_offs[ir] = imageLoad(objdata,ivec2(0,_root_to_zero[ir-1]));
		//imageStore(debugdata,ivec2(ir-1,gl_GlobalInvocationID.y),vec4(_root_to_zero_offs[_root_len],777));
		//imageStore(debugdata,ivec2(_root_len,gl_GlobalInvocationID.y),vec4(_root_to_zero[_root_len],_root_to_zero_offs[_root_len]));		
	}
	//imageStore(debugdata,ivec2(3,gl_GlobalInvocationID.y),vec4(_root_len,_root_len,_root_len,_root_len));
	return (Root(_root_to_zero,_root_to_zero_offs,_root_len));

}

vec4 comp_pos_in_local(Root root, int ind,int ind_local)
{
	vec4 obj = imageLoad(objdata,ivec2(0,ind));
	float units_root = root.root_to_zero_offs[0].w;
	vec3 loc_pos = obj.xyz*pow(1e6,obj.w-units_root);
	int i = 0;
	while( i<root.root_len && ind_local != root.root_to_zero[i] )
	{

		loc_pos-=root.root_to_zero_offs[i].xyz*pow(1e6,root.root_to_zero_offs[i].w-units_root);
		i++;
	}
	loc_pos-=root.root_to_zero_offs[i].xyz*pow(1e6,root.root_to_zero_offs[i].w-units_root);;
	return(vec4(loc_pos,obj.w));
}
/*
vec4 comp_pos_in_local(Root root, int ind,int ind_local)
{
	vec4 obj = imageLoad(objdata,ivec2(0,ind));
	float units_root = 1;
	vec3 loc_pos = obj.xyz*pow(1e6,obj.w-units_root);
	int i = 0;
	while( i<root.root_len && ind_local != root.root_to_zero[i] )
	{

		loc_pos-=root.root_to_zero_offs[i].xyz*pow(1e6,root.root_to_zero_offs[i].w-units_root);
		i++;
	}
	loc_pos-=root.root_to_zero_offs[i].xyz*pow(1e6,root.root_to_zero_offs[i].w-units_root);;
	return(vec4(loc_pos,obj.w));
}*/
Root change_pos_unit(Root root, int new_unit)
{
	vec4 old_pos = root.root_to_zero_offs[0];
	root.root_to_zero_offs[0] = vec4(old_pos.xyz*pow(1e6,old_pos.w- new_unit), new_unit);
	return(root);
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
	root_dest = change_pos_unit(root_dest,int(root_rel.root_to_zero_offs[0].w));
	vec4 pos_dest_com = comp_pos_in_local(root_dest,i_st,ind_local);
	vec4 pos_rel_com = comp_pos_in_local(root_rel,i_st,ind_local);

	return(pos_rel_com-pos_dest_com);
}

void main() 
{
	gl_ViewportIndex = gl_InvocationID;
	

	ivec2 curP1 = ivec2(0,int(vs_out[0].ind));
	ivec2 curP_cam = ivec2(7,int(vs_out[0].ind));
	

	float select = imageLoad(choosedata,curP1).y;
	
	vec4 pos_cur = imageLoad(objdata,curP1);
	vec4 curPos =vec4(pos_cur.xyz, 1.0);
	curPos.a = imageLoad(posTimeData,curP1).a;

	vec4 curPos_cam =vec4(imageLoad(objdata,curP_cam).rgb, 1.0);
	

	
	_color = vec3(0.8,0,0);
	
	if(select==1)
	{
		_color = vec3(0,0.8,0);
	}

	float dim_cont = 1;
	if(select!=2)
	{
		gl_Position = VPs[0]* vec4(curPos_cam.xyz, 1.0);


		gl_Position.x+=dim_cont;
		EmitVertex();

		gl_Position.y+=dim_cont;
		gl_Position.x-=dim_cont;
		EmitVertex();

		gl_Position.y-=dim_cont;
		gl_Position.x-=dim_cont;
		EmitVertex();

		gl_Position.y-=dim_cont;
		gl_Position.x+=dim_cont;
		EmitVertex();

		gl_Position.y+=dim_cont;
		gl_Position.x+=dim_cont;
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
	//int ind_center_obj =int(imageLoad(objdata,ivec2(8,int(vs_out[0].ind))).x);
	Root root_cur = load_root(int(vs_out[0].ind),pos_cur ); 

	vec4 pos_cam = imageLoad(objdata,ivec2(0, targetCamInd));
	//int ind_loc_cam = int(imageLoad(objdata,ivec2(8, targetCamInd)).x);
	Root root_cam = load_root(targetCamInd,pos_cam);

	vec3 pos_cur_in_cam = (comp_pos_in_local_relat(root_cur, int( vs_out[0].ind), root_cam, targetCamInd).xyz);
	imageStore(debugdata,ivec2(5,vs_out[0].ind),vec4(pos_cur_in_cam,123));
	imageStore(debugdata,ivec2(6,vs_out[0].ind),vec4(pos_cur.xyw,111));
	imageStore(debugdata,ivec2(7,vs_out[0].ind),vec4(pos_cam.xyw,222));
	//imageStore(debugdata,ivec2(8,vs_out[0].ind),vec4(pos_cam.xyw,333));
	//------------------------------------------------------------------------------------------------------------------
	for (int i = int(curPos.a); i < 199 ; i++)
	{ 		
		ivec2 curP_c = ivec2(i,int(vs_out[0].ind));
		vec3 pos_cam_off = imageLoad(posTimeData,curP_c).xyz - pos_cur.xyz;
		gl_Position =VPs[0]* vec4((pos_cur_in_cam +pos_cam_off*pow(1e6,pos_cur.w-pos_cam.w)), 1.0);
		EmitVertex();		
	}
	for (int i = 1; i < int(curPos.a) ; i++)
	{ 		
		ivec2 curP_c = ivec2(i,int(vs_out[0].ind));
		vec3 pos_cam_off = imageLoad(posTimeData,curP_c).xyz - curPos.xyz;
		gl_Position =VPs[0]* vec4((pos_cur_in_cam +pos_cam_off*pow(1e6,pos_cur.w-pos_cam.w)) , 1.0);
		EmitVertex();	
	}	
	EndPrimitive();	
}
