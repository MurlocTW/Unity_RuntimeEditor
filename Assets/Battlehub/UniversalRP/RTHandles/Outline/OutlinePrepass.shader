Shader "Battlehub/URP/OutlinePrepass"
{
	Properties
	{
	}
	SubShader
	{
		Pass
		{
			HLSLPROGRAM

			#include "Outline.hlsl"
			
			#pragma target 3.5
			#pragma multi_compile_instancing
			#pragma vertex PrepassVertex
			#pragma fragment PrepassFragment


			ENDHLSL
		}
	}
}
