#version 460 core

layout (local_size_x = 1, local_size_y = 1, local_size_z = 1) in;

layout (rgba32f, binding = 0) uniform  image2D objdata;

const float deltTime = 1;
const float G = 1.18656E-19;
//1 - объект расчёта, 2 - влияющие на него другие объекты
//масса в массах земли, расстояние в астрономических единицах
vec3 compGravit(in vec3 pos1, in float mass1,in vec3 pos2,in float mass2)
{
	float dist = distance(pos1,pos2);
	if(dist<1.0E-9)
	{
		dist = 1.0E-9;
	}
	float a = (G*mass2)/(dist*dist);
	vec3 a3 = ((pos2 - pos1)/dist)*a;
	return(a3);
}

void main() 
{
	ivec2 ipos1 = ivec2(0, gl_GlobalInvocationID.y );
	ivec2 ipos2 =  ivec2(1,gl_GlobalInvocationID.y);
	vec3 acs3 = vec3(0,0,0);
	vec4 pos1 = imageLoad(objdata,ipos1);
	vec3 vel1 = imageLoad(objdata,ipos2).rgb;
	float size1 = imageLoad(objdata,ipos2).a;

	for(int i=0; i< imageSize(objdata).y; i++)
	{

		if(ipos1.y!=i)      
		{
			ivec2 curP1 = ivec2(0,i);
			vec4 obj = imageLoad(objdata,curP1);
			acs3 += compGravit(pos1.xyz,pos1.a,obj.rgb,obj.a);
		}
	}


	
	pos1.xyz += vel1*deltTime + (acs3*deltTime*deltTime)/2;
	vel1 += acs3*deltTime;
	//vel1 = vec3( imageSize(objdata).x,imageSize(objdata).y,);
	imageStore(objdata, ipos1, pos1);
	imageStore(objdata, ipos2, vec4(vel1, size1));
	//imageStore(objdata, ipos1, vec4(gl_GlobalInvocationID.x,1,2,3));
	//imageStore(objdata, ipos2, vec4(gl_GlobalInvocationID.y,4,5,6));
}	