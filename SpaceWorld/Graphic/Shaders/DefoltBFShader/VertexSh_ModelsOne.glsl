#version 460 core

layout(location = 0) in vec3 _vertexPosition_model;
layout(location = 1) in vec3 _vertexNormal_model;
layout(location = 2) in vec3 _vertexColor;
layout(location = 3) in vec2 _vertexTexture;
uniform mat4 ModelMatrix;

uniform vec3 LightPosition_world;
uniform mat4 VPs[4];
uniform mat4 Vs[4];
uniform mat4 Ps[4];
uniform vec2 MouseLoc;
uniform vec2 MouseLocGL;
uniform vec3 MaterialDiffuse;
uniform vec3 MaterialAmbient;
uniform vec3 MaterialSpecular;
uniform float lightPower;
uniform sampler2D textureSample;
uniform int textureVis;

out VS_FS_INTERFACE
{
vec3 Color;
} vs_out;

void main() 
{
	vec4 vertexPosition_world= ModelMatrix*vec4(_vertexPosition_model,1);
	gl_Position = VPs[0] * vec4(vertexPosition_world);
	vec3 vertexPosition_camera = (Vs[0] * vertexPosition_world).xyz;
	vec3 EyeDirection_camera = vec3(0,0,0) - vertexPosition_camera;
	vec3 LightPosition_camera = ( Vs[0] * vec4(LightPosition_world,1)).xyz;
	vec3 LightDirection_camera = LightPosition_camera + EyeDirection_camera;
	vec3 Normal_camera = ( Vs[0] * vec4(_vertexNormal_model, 1.0)).xyz;

	vec3 LightColor = vec3(1.0, 1.0, 1.0);
	float LightPower = lightPower;
	
	vec3 MaterialAmbientColor = MaterialAmbient;
	vec3 MaterialSpecularColor = MaterialSpecular;
	float distance = length( LightPosition_world - vertexPosition_world.xyz );
	vec3 n = normalize( Normal_camera );
	vec3 l = normalize( LightDirection_camera );
	float cosTheta = clamp( dot( n,l ), 0,1 );
	vec3 E = normalize(EyeDirection_camera);
	vec3 R = reflect(-l,n);
	float cosAlpha = clamp( dot( E,R ), 0,1 );
	vec3 MaterialDiffuseColor;
	if(textureVis == 1)
	{
		MaterialDiffuseColor = texture( textureSample,  _vertexTexture ).xyz;
	}
	else
	{
		MaterialDiffuseColor =_vertexColor;
	}

	vs_out.Color = MaterialAmbientColor + MaterialDiffuseColor * LightColor * LightPower * cosTheta / (distance*distance) +MaterialSpecularColor * LightColor * LightPower * pow(cosAlpha,5) / (distance*distance);
	  
}