#version 460 core

layout (local_size_x = 1, local_size_y = 1, local_size_z = 1) in;

layout (rgba32f, binding = 0) uniform  image2D objdata;
layout (rgba32f, binding = 2) uniform  image2D choosedata;
layout (rgba32f, binding = 3) uniform  image2D debugdata;
uniform int targetCamInd;
uniform mat4 VPs[4];
uniform mat4 Vs[4];
uniform vec2 MouseLocGL;
const float deltTime = 1000;
const float G = 1.18656E-19;
const float G_2 = 6.6743e-11;

struct Root
{
	int[10] root_to_zero;
	vec3[10] root_to_zero_offs;
	int root_len;

};

Root load_root(int i,vec3 pos)
{
	vec4 root_inf1 = imageLoad(objdata, ivec2(8,i));
	vec4 root_inf2 = imageLoad(objdata, ivec2(9,i));
	int[10] _root_to_zero = int[](int(root_inf1.z),int(root_inf1.w),int(root_inf2.x),int(root_inf2.y),int(root_inf2.z),int(root_inf2.w),-1,-1,-1,-1);
	vec3[10] _root_to_zero_offs = vec3[](pos ,vec3(0),vec3(0),vec3(0),vec3(0),vec3(0),vec3(0),vec3(0),vec3(0),vec3(0));
	int _root_len = int(root_inf1.y);
	//int ir = 1;
	//imageStore(debugdata,ivec2(0,gl_GlobalInvocationID.y),vec4(local_ind ,pos));
	for(int ir=1;ir<=_root_len&& _root_to_zero[ir-1]!=0;ir++)
	{
		_root_to_zero_offs[ir] = imageLoad(objdata,ivec2(0,_root_to_zero[ir-1])).xyz;
		//imageStore(debugdata,ivec2(ir-1,gl_GlobalInvocationID.y),vec4(_root_to_zero_offs[_root_len],777));
		//imageStore(debugdata,ivec2(_root_len,gl_GlobalInvocationID.y),vec4(_root_to_zero[_root_len],_root_to_zero_offs[_root_len]));		
	}
	//imageStore(debugdata,ivec2(3,gl_GlobalInvocationID.y),vec4(_root_len,_root_len,_root_len,_root_len));
	return (Root(_root_to_zero,_root_to_zero_offs,_root_len));

}

void save_root(Root root, int i,float mass_i)
{
	vec4 root_inf1 = vec4(mass_i,root.root_len,root.root_to_zero[0],root.root_to_zero[1]);
	vec4 root_inf2 = vec4(root.root_to_zero[2],root.root_to_zero[3],root.root_to_zero[4],root.root_to_zero[5]);
	imageStore(objdata, ivec2(8,i), root_inf1);
	imageStore(objdata, ivec2(9,i), root_inf2);
}

void setModelMatr(in float size,in vec3 pos,in vec4 rot)
{

	float A = cos(rot.x);
    float B = sin(rot.x);
    float C = cos(rot.y);
    float D = sin(rot.y);
    float E = cos(rot.z);
    float F = sin(rot.z);

	float AD = A * D;
    float BD = B * D;

	imageStore(objdata, ivec2(4,gl_GlobalInvocationID.y), size*vec4(C * E,-C * F,-D,0));
	imageStore(objdata, ivec2(5,gl_GlobalInvocationID.y), size*vec4(-BD * E + A * F,BD * F + A * E,-B * C,0));
	imageStore(objdata, ivec2(6,gl_GlobalInvocationID.y), size*vec4(AD * E + B * F,-AD * F + B * E,A * C,0));
	imageStore(objdata, ivec2(7,gl_GlobalInvocationID.y), vec4(pos,1));
}
vec4 draw(in float size,in vec3 pos)
{
	ivec2 ipos4 =  ivec2(3,gl_GlobalInvocationID.y);

    vec4 pos2d= VPs[0]* vec4(pos,1);
	vec4 pos3d= Vs[0]* vec4(pos,1);
	vec3 pos2dh = pos2d.xyz/pos2d.w;

	float board = 1.2;

	//imageStore(objdata, ipos4,vec4(size,length(pos3d.xyz)/10,length(pos3d.xyz) ,size/length(pos3d.xyz)));
	
	bool vis;
	if(size/length(pos3d.xyz)> 2)
	{
		vis = true;	
	}
	else
	{
		if(size/length(pos3d.xyz)< 1e-3)
		{
			vis = false;	
		}
		else
		{
			if(pos2dh.x<board  && pos2dh.x>-board
			&& pos2dh.y<board && pos2dh.y>-board)
			{
				vis = true;	
			}
			else
			{
				vis = false;	
			}
		}
	}

	
	vec4 cho = vec4(0,0,0,0);
	vis =true;
	if(vis)
	{
		 cho.x = 1;//need for vis(1.in camera 2.size is same)
	}
	if(length(MouseLocGL-pos2dh.xy)<(size/length(pos3d.xyz))+0.01)
	{
		cho.y = 1;//selected cursor
	}
	cho.x = 1;

	//imageStore(objdata, ipos4,vec4(cho.y,length(MouseLocGL-pos2dh.xy),(size/length(pos3d.xyz))+0.01 ,size/abs(pos3d.z)));
	return(cho);
}

//1 - объект расчёта, 2 - влияющие на него другие объекты
//масса в массах земли, расстояние в астрономических единицах
vec3 compGravit(in vec3 pos1, in float mass1,in vec3 pos2,in float mass2,in float size1, out vec3 moment1,out float omega_2)
{
	float dist = distance(pos1,pos2);
	if(dist<1.0E-9)
	{
		dist = 1.0E-9;
	}
	float a = (G*mass2)/(dist*dist);
	vec3 a3 = ((pos2 - pos1)/dist)*a;
	omega_2 =length( a3)/dist;
	//центр масс полукруга y = 4*r/(3*pi)




	return(a3);
}

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

bool check_in_root(Root root, int ind)
{
	for(int i=0; i< 10;i++)//len_arr
	{
		if(ind == root.root_to_zero[i])
		{
			return(true);
		}
	}

	return(false);
}



Root comp_root(vec3 pos, int local_ind)
{
	int[10] _root_to_zero = int[](local_ind,-1,-1,-1,-1,-1,-1,-1,-1,-1);
	vec3[10] _root_to_zero_offs = vec3[](pos ,vec3(0),vec3(0),vec3(0),vec3(0),vec3(0),vec3(0),vec3(0),vec3(0),vec3(0));
	int _root_len = 1;
	//imageStore(debugdata,ivec2(0,gl_GlobalInvocationID.y),vec4(local_ind ,pos));
	while(_root_len<10 && _root_to_zero[_root_len-1]!=0)
	{
		_root_to_zero[_root_len] =int(imageLoad(objdata,ivec2(8,_root_to_zero[_root_len-1])).z);
		_root_to_zero_offs[_root_len] = imageLoad(objdata,ivec2(0,_root_to_zero[_root_len-1])).xyz;
		//imageStore(debugdata,ivec2(_root_len,gl_GlobalInvocationID.y),vec4(_root_to_zero[_root_len],_root_to_zero_offs[_root_len]));
		_root_len+=1;		
	}
	//imageStore(debugdata,ivec2(3,gl_GlobalInvocationID.y),vec4(_root_len,_root_len,_root_len,_root_len));
	return (Root(_root_to_zero,_root_to_zero_offs,_root_len));
}


void main() 
{
	ivec2 ipos1 = ivec2(0, gl_GlobalInvocationID.y );
	ivec2 ipos2 =  ivec2(1,gl_GlobalInvocationID.y);
	ivec2 ipos3 =  ivec2(2,gl_GlobalInvocationID.y);
	ivec2 ipos4 =  ivec2(3,gl_GlobalInvocationID.y);

	vec3 acs3 = vec3(0,0,0);
	vec4 pos1 = imageLoad(objdata,ipos1);
	vec3 vel1 = imageLoad(objdata,ipos2).xyz;
	float size1 = imageLoad(objdata,ipos2).w;

	vec4 rot1 = imageLoad(objdata,ipos3);
	vec4 velrot1 = imageLoad(objdata,ipos4);
	vec3 moment1 = vec3(0,0,0);
	float true_size = rot1.w;
	//--------------------------------------------------------
	vec4 pos_inf_cur = imageLoad(objdata,ivec2(8,gl_GlobalInvocationID.y));
	float mass_cur = pos_inf_cur.x;
	int ind_center_obj =int(pos_inf_cur.z);
	//int ind_center_obj = int(imageLoad(objdata,ivec2(8, gl_GlobalInvocationID.y)).z);
	int ind_center_obj_old = ind_center_obj;
	float max_omega = 0;
	//------------------------root ind for local-------------------------
	Root root_cur;
	if(velrot1.w!=1)
	{
		root_cur = comp_root(pos1.xyz,ind_center_obj);
		save_root(root_cur,int(gl_GlobalInvocationID.y),mass_cur);
		velrot1.w = 1;
	}
	else
	{
		//root_cur = comp_root(pos1.xyz,ind_center_obj);
		root_cur = load_root(int(gl_GlobalInvocationID.y),pos1.xyz);
	}

	//--------------------------------------------------------
	for(int i=0; i< imageSize(objdata).y; i++)
	{
		vec4 pos_inf = imageLoad(objdata,ivec2(8,i));
		int ind_local =int(pos_inf .z);
		float mass_i = pos_inf.x;
		//if(ipos1.y!=i && check_in_root(root_cur,ind_local))
		if(ipos1.y!=i && i==ind_center_obj)
		{

			vec4 obj = comp_pos_in_local(root_cur, i,ind_local);
			
			//imageStore(debugdata,ivec2(i,gl_GlobalInvocationID.y),vec4(obj.xyz,999));
			vec3 moment_1_i = vec3(0,0,0);
			float omega_2 = 0;
			vec3 acs = compGravit(vec3(0),mass_cur,obj.xyz,mass_i,size1,moment_1_i,omega_2);
			acs3 += acs;
			//imageStore(debugdata,ivec2(i,gl_GlobalInvocationID.y),vec4(acs,999));

			moment1+=moment_1_i;

			if(pos1.a<obj.a)
			{
				if(omega_2 > max_omega)
				{
					max_omega = omega_2;
					//ind_center_obj = i;
				}
			}
			//imageStore(debugdata,ivec2(ipos1.y,i),vec4(ipos1.y,i,length(grav),ind_center_obj));
				
		}
		else
		{
			//imageStore(debugdata,ivec2(ipos1.y,i),vec4(0,0,0,0));
		}
	}
	if(ind_center_obj_old != ind_center_obj)
	{
		//пересчёт из одной системы координат в другую
	}
	vec4 pos_cam = imageLoad(objdata,ivec2(0, targetCamInd));
	//int ind_loc_cam = int(imageLoad(objdata,ivec2(8, targetCamInd)).x);
	Root root_cam = load_root(targetCamInd,pos_cam.xyz);

	//vec3 pos_cur_in_cam = comp_pos_in_local(root_cam,int( gl_GlobalInvocationID.y), ind_center_obj).xyz;
	vec3 pos_cur_in_cam = comp_pos_in_local_relat(root_cur, int( gl_GlobalInvocationID.y), root_cam, targetCamInd).xyz;

	//imageStore(debugdata,ivec2(0,gl_GlobalInvocationID.y),vec4(pos_cur_in_cam,999));
	setModelMatr(true_size,pos_cur_in_cam ,rot1);	
	vec4 choose = draw(size1,pos_cur_in_cam);
	imageStore(choosedata, ipos1, choose);
	//------------------------------------------------------
	pos1.xyz += vel1*deltTime + (acs3*deltTime*deltTime)/2;
	vel1 += acs3*deltTime;

	float J;
	vec3 eps = moment1/J;
	rot1.xyz+=velrot1.xyz*deltTime ;
	
	imageStore(objdata, ipos1, pos1);
	imageStore(objdata, ipos2, vec4(vel1, size1));

	imageStore(objdata, ipos3,  vec4(rot1.xyz,true_size));
	imageStore(objdata, ipos4, velrot1);

	


}	