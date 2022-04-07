// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Projector/BlobShadow" {
	Properties{
		_Color("Color", Color) = (1, 1, 1, 1)
		_ShadowTex("Cookie", 2D) = "gray" {}
		_FalloffTex("FallOff", 2D) = "white" {}
		_Angle("Angle", float) = 90
		_Radius("Radius", float) = 1
		_Thickness("Thickness", Range(0, 1)) = 0.2
		[Toggle]_CircleActive("Circle Active", int) = 1
		[Toggle(DRAW_CIRCLE)]_DrawCircle("Draw Circle", int) = 0
		[Toggle(IS_FILLED)]_IsFilled("Is Filled", int) = 1
	}
		Subshader{
			Tags {"Queue" = "Transparent"}
			Pass {
				ZWrite Off
				ColorMask RGB
				//Blend DstColor Zero
				Blend SrcAlpha OneMinusSrcAlpha
				Offset -1, -1

				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma multi_compile_fog
				#pragma shader_feature_local DRAW_CIRCLE
				#pragma shader_feature_local IS_FILLED
				#include "UnityCG.cginc"

				struct vertex_out {
					float4 uvShadow : TEXCOORD0;
					float4 uvFalloff : TEXCOORD1;
					UNITY_FOG_COORDS(2) // TEXCOORD2
					float4 pos : SV_POSITION;
					float intensity : TEXCOORD3; // additional intensity, based on normal orientation
				};

				float4x4 unity_Projector;
				float4x4 unity_ProjectorClip;

				vertex_out vert(float4 vertex : POSITION, float3 normal : NORMAL)
				{
					vertex_out o;
					o.intensity = sign(dot(float3(0.0, 1.0, 0.0), UnityObjectToWorldNormal(normal))); // 1.0 if pointing UP
					o.pos = UnityObjectToClipPos(vertex);
					o.uvShadow = mul(unity_Projector, vertex);
					o.uvFalloff = mul(unity_ProjectorClip, vertex);
					UNITY_TRANSFER_FOG(o,o.pos);
					return o;
				}

				sampler2D _ShadowTex;
				sampler2D _FalloffTex;
				bool _CircleActive = 1;

#if DRAW_CIRCLE
				float4 _Color;
				float _Angle;
				float _Radius;
				float _Thickness;

				float cross(float2 a, float2 b)
				{
					return a.x * b.y - a.y * b.x;
				}

				float2 rotate(float2 p, float a)
				{
					float2 result = float2(0, 0);
					result.x = cos(a) * p.x + sin(a) * p.y;
					result.y = sin(a) * p.x + cos(a) * p.y;

					return result;
				}

				float4 draw(float2 uv)
				{
					float thickness = _Thickness / 10;
					//float halfThickness = thickness / 2;

					float mag = length(uv);

					float2 rot1 = rotate(float2(0, -1), -radians(_Angle / 2));
					float2 rot2 = rotate(float2(0, -1), radians(_Angle / 2));

					float ld = cross(rot1, uv);
					float rd = cross(rot2, uv);

					//float radius = _Radius / 4.75;
					float radius = _Radius / 4;
					bool isInsideCircle = mag <= radius;
					bool c1 = mag >= radius - thickness;

	#if IS_FILLED
					bool c2 = ld <= 0;
					bool c3 = rd >= 0;

					return _Color * float(isInsideCircle && (c1 || c2 && c3));
	#else
					//bool c2 = ld - halfThickness <= 0 && ld > -halfThickness && rd + halfThickness >= 0;
					//bool c3 = rd + halfThickness >= 0 && rd < halfThickness&& ld - halfThickness <= 0;
					bool c2 = ld <= 0 && ld > -thickness && rd >= 0;
					bool c3 = rd >= 0 && rd < thickness && ld <= 0;

					return _Color * float(isInsideCircle && (c1 || c2 || c3));
	#endif
				}
#else
				float4 draw(float2 uv)
				{
					return 0;
				}
#endif

				fixed4 frag(vertex_out i) : SV_Target
				{
					fixed4 texS = tex2Dproj(_ShadowTex, i.uvShadow);
					texS.a = 1.0 - texS.a;

					fixed4 texF = tex2Dproj(_FalloffTex, i.uvFalloff);
					fixed4 res = lerp(fixed4(0, 0, 0, 0), texS, texF.a * i.intensity);
					//res += draw((i.uvShadow.xy - 12.5) / 10) * i.intensity;
					float4 result = draw(i.uvShadow.xy / i.uvShadow.w - 0.5) * i.intensity * _CircleActive;
					res.rgb = result.rgb;
					res.a = max(res.a, result.a);

					//res = fwidth((i.uvShadow - 1.5) / 10);
					//res.a = 1;

					UNITY_APPLY_FOG_COLOR(i.fogCoord, res, fixed4(1,1,1,1));
					return res;
				}
				ENDCG
			}
	}
}