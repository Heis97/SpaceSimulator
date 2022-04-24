#version 460 core
layout (lines, invocations = 1) in;
layout (line_strip, max_vertices = 202) out;
layout (rgba32f, binding = 0) uniform  image2D posData;
layout (r32f, binding = 2) uniform  image2D massData;
uniform mat4 VPs[4];
in VS_GS_INTERFACE
{
	float ind;
}vs_out[];

const float G = 1.18656E-19;
//1 - объект расчёта, 2 - влияющие на него другие объекты
//масса в массах земли, расстояние в астрономических единицах

vec3 compGravit(in vec3 pos1, in vec3 pos2,in float mass2)
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

vec3 acsInPoint(in vec3 pos1)
{
	vec3 acs3 = vec3(0,0,0);
	for(int i=0; i< imageSize(massData).x; i++)
	{
		ivec2 curP = ivec2(i,0);
		acs3 += compGravit(pos1,imageLoad(posData,curP).rgb,imageLoad(massData,curP).r);
	}
	return(acs3);
}

void main() 
{
	vec3 pos_area=imageLoad(posData,ivec2(1,0)).rgb;
	float scale =0.00003;
	for(int i=-100; i< 100; i++)
	{
		vec3 pos = vec3(scale*(vs_out[0].ind-100)+pos_area.x,scale*i+pos_area.y,0);
		vec3 a = acsInPoint(pos);
		float lena =1e+3 *sqrt(length(a));
		gl_Position =VPs[0]* vec4(pos.xy,lena,1);
		EmitVertex();
	}
}	
