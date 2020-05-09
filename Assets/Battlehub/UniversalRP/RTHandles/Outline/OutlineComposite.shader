Shader "Battlehub/URP/OutlineComposite"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
	}
	SubShader
	{
		Pass
		{
			HLSLPROGRAM

			#include "Outline.hlsl"
			
			#pragma target 3.5
			#pragma multi_compile_instancing
			#pragma vertex CompositePassVertex
			#pragma fragment CompositePassFragment


			ENDHLSL
		}
	}
}
