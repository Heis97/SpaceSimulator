#version 460 core


layout (local_size_x = 1, local_size_y = 1, local_size_z = 1) in;

layout (rgba32f, binding = 0) uniform  image2D objdata;
layout (rgba32f, binding = 2) uniform  image2D choosedata;
layout (rgba32f, binding = 3) uniform  image2D debugdata;
uniform int targetCamInd;
uniform mat4 VPs[4];
uniform mat4 Vs[4];
uniform vec2 MouseLocGL;
const float deltTime = 100;
const float G = 1.18656E-19;



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

vec4 comp_pos_in_local(int[10] root_to_zero,vec3[10] root_to_zero_offs,int root_len, int ind,int ind_local)
{
	vec4 obj = imageLoad(objdata,ivec2(0,ind));
	vec3 loc_pos = obj.xyz;
	for(int i=0; i<root_len || ind_local != root_to_zero[i]; i++)
	{
		loc_pos-=root_to_zero_offs[i];
	}
	return(vec4(loc_pos,obj.w));
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
	int ind_center_obj = int(velrot1.w);
	int ind_center_obj_old = ind_center_obj;
	float max_omega = 0;
	//------------------------root ind for local-------------------------
	
	int[10] root_to_zero = int[](ind_center_obj,-1,-1,-1,-1,-1,-1,-1,-1,-1);
	vec3[10] root_to_zero_offs = vec3[](pos1.xyz ,vec3(0),vec3(0),vec3(0),vec3(0),vec3(0),vec3(0),vec3(0),vec3(0),vec3(0));
	int root_len = 1;
	imageStore(debugdata,ivec2(0,gl_GlobalInvocationID.y),vec4(ind_center_obj ,pos1.xyz));
	while(root_len<10 && root_to_zero[root_len-1]!=0)
	{
		root_to_zero[root_len] =int(imageLoad(objdata,ivec2(3,root_to_zero[root_len-1])).w);
		root_to_zero_offs[root_len] =imageLoad(objdata,ivec2(0,root_to_zero[root_len-1])).xyz;
		imageStore(debugdata,ivec2(root_len,gl_GlobalInvocationID.y),vec4(root_to_zero[root_len],root_to_zero_offs[root_len]));
		root_len+=1;		
	}
	
	
	//--------------------------------------------------------
	for(int i=0; i< imageSize(objdata).y; i++)
	{
	
		if(ipos1.y!=i)      
		{

			int ind_local =int( imageLoad(objdata,ivec2(3,i)).w);
			vec4 obj = comp_pos_in_local(root_to_zero,root_to_zero_offs,root_len, i,ind_local);

			vec3 moment_1_i = vec3(0,0,0);
			float omega_2 = 0;

			acs3 += compGravit(pos1.xyz,pos1.w,obj.xyz,obj.w,size1,moment_1_i,omega_2);
			moment1+=moment_1_i;

			if(pos1.a<obj.a)
			{
				if(omega_2 > max_omega)
				{
					max_omega = omega_2;
					ind_center_obj = i;
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
	pos1.xyz += vel1*deltTime + (acs3*deltTime*deltTime)/2;
	vel1 += acs3*deltTime;

	float J;
	vec3 eps = moment1/J;
	rot1.xyz+=velrot1.xyz*deltTime ;


	//velrot1.w = ind_center_obj;
	
	imageStore(objdata, ipos1, pos1);
	imageStore(objdata, ipos2, vec4(vel1, size1));

	imageStore(objdata, ipos3,  vec4(rot1.xyz,true_size));
	imageStore(objdata, ipos4, velrot1);



	vec3 targetC = imageLoad(objdata,ivec2(0, targetCamInd)).xyz;
	setModelMatr(true_size,pos1.xyz-targetC,rot1);	
	vec4 choose = draw(size1,pos1.xyz-targetC);
	imageStore(choosedata, ipos1, choose);


}	