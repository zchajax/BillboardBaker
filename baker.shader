Shader "Custom/Billboadbaker"
{
	Properties
	{
		_MainTex("Albedo", 2D) = "white" {}
		_NormalMap("Normal Map", 2D) = "bump" {}
		_Cutoff("Cut off", Range(0, 1)) = 0.33
		[Enum(UnityEngine.Rendering.CullMode)]_Culling("Culling", Float) = 0
	}
		SubShader
	{
		Tags { "RenderType" = "Opaque" }
		LOD 100
		Cull [_Culling]
		ZTest On
		ZWrite On

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				float4 tangent : TANGENT;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				
				half3 tspace0 : TEXCOORD1;
				half3 tspace1 : TEXCOORD2;
				half3 tspace2 : TEXCOORD3;
			};

			struct fragOut
			{
				float4 color : SV_Target0;
				float4 normal : SV_Target1;
			};

			sampler2D _MainTex;
			sampler2D _NormalMap;
			float4 _MainTex_ST;
			float _Cutoff;

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);

				half3 normal = normalize(UnityObjectToWorldNormal(v.normal));
				half3 tangent = normalize(UnityObjectToWorldDir(v.tangent.xyz));
				half tangentSign = v.tangent.w * unity_WorldTransformParams.w;
				half3 bitangent = normalize(cross(normal, tangent) * tangentSign);

				/*half3 normal = normalize(mul(UNITY_MATRIX_IT_MV, v.normal.xyzz));
				half3 tangent = normalize(mul(UNITY_MATRIX_IT_MV, v.tangent.xyzz));
				half3 bitangent = normalize(cross(normal, tangent));*/

				normal = normalize(mul(UNITY_MATRIX_V, float4(normal, 0)).xyz);
				tangent = normalize(mul(UNITY_MATRIX_V, float4(tangent, 0)).xyz);
				bitangent = normalize(mul(UNITY_MATRIX_V, float4(bitangent, 0)).xyz);

				o.tspace0 = half3(tangent.x, bitangent.x, normal.x);
				o.tspace1 = half3(tangent.y, bitangent.y, normal.y);
				o.tspace2 = half3(tangent.z, bitangent.z, normal.z);

				return o;
			}

			fragOut frag(v2f i) : SV_Target
			{
				
				fragOut o;

				o.color = tex2Dlod(_MainTex, float4(i.uv, 0, 0));
				clip(o.color.a - _Cutoff);
				o.color.w = 1;

				half3 normal = normalize(UnpackNormal(tex2D(_NormalMap, float2(i.uv))));
				o.normal.x = dot(i.tspace0, normal);
				o.normal.y = dot(i.tspace1, normal);
				o.normal.z = dot(i.tspace2, normal);
				o.normal.xyz = normalize(o.normal.xyz);
				o.normal.xyz = normalize(float3(i.tspace0.z, i.tspace1.z, i.tspace2.z));

				o.normal.w = 1.0;
				o.normal = o.normal * 0.5 + 0.5;
				//o.color.xyz = normalize(float3(i.tspace0.z, i.tspace1.z, i.tspace2.z)) * 0.5 + 0.5;
				//o.color.xyz = o.normal.xyz;

				return o;
			}
			ENDCG
		}
	}
}
