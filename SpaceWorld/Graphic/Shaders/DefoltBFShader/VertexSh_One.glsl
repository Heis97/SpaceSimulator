#version 460 core

layout(location = 0) in vec3 _Position_model;
layout(location = 1) in vec3 _Normal_model;

layout (rgba32f, binding = 0) uniform  image2D objdata;
layout (rgba32f, binding = 2) uniform  image2D choosedata;

uniform vec3 LightPosition_world;
uniform mat4 VPs[4];
uniform mat4 Vs[4];
uniform mat4 Ps[4];
uniform vec2 MouseLoc;
uniform vec2 MouseLocGL;
uniform float lightPower;
uniform sampler2D textureSample;
uniform int textureVis;
uniform vec3 colorOne;
uniform int stind;

out VS_FS_INTERFACE
{
vec3 Color;
} vs_out;

mat4 modelMatr(in int  _ind)
{
	return(mat4( 
	imageLoad(objdata,ivec2(4,_ind)),
	imageLoad(objdata,ivec2(5,_ind)),
	imageLoad(objdata,ivec2(6,_ind)),
	imageLoad(objdata,ivec2(7,_ind))
	));
}
vec3 select(in int _ind)
{
     if(imageLoad(choosedata,ivec2(0,_ind)).y==1)
	 {
		return (vec3(1,0,0));	
	 }
	 else
	 {
		return (vec3(0.5));
	 }
}
void main() 
{
	int ind = stind + gl_InstanceID;
	ind = int(imageLoad(choosedata,ivec2(1,ind)).x);

	vec4 Position_world = modelMatr(ind)*vec4(_Position_model,1);
	vec3 Normal_world = normalize((modelMatr(ind)*vec4(_Normal_model,0)).xyz);
	gl_Position = VPs[0] * Position_world;

	vec3 Position_camera = (Vs[0] * Position_world).xyz;
	vec3 EyeDirection_camera = vec3(0,0,0) - Position_camera;
	vec3 LightPosition_camera = ( Vs[0] * vec4(LightPosition_world,1)).xyz;
		
	vec3 LightDirection_world = Position_world.xyz - LightPosition_world;
	vec3 LightDirection_camera = LightPosition_camera + EyeDirection_camera;

	vec3 Normal_camera =(Vs[0] * vec4(Normal_world, 1.0)).xyz;

	vec3 LightColor = vec3(1.0, 1.0, 1.0);
	float LightPower = lightPower;
	float distance = length( LightPosition_world - Position_world.xyz );
	vec3 n = normalize( Normal_world );
	vec3 l = normalize( LightDirection_world );
	float cosTheta = clamp( dot( n,l ), 0,1 );

	vec3 lc = normalize( LightDirection_camera );
	vec3 nc = normalize( Normal_camera);
	vec3 E = normalize(EyeDirection_camera);
	vec3 R = reflect(-lc,nc);

	float cosAlpha = clamp(dot( E,R ) , 0,1 );
	


	vec3 MaterialDiffuseColor = colorOne;
	vec3 MaterialAmbientColor = vec3(0.05);
	vec3 MaterialSpecularColor = vec3(0.01);
	vs_out.Color  = MaterialAmbientColor + MaterialDiffuseColor * LightColor * LightPower * cosTheta / (distance*distance) +MaterialSpecularColor * LightColor * LightPower * pow(cosAlpha,5) / (distance*distance);
	//vs_out.Color = select(ind);
	
}