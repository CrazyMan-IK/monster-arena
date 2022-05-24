Shader "Custom/Clouds"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1, 1, 1, 1)
        _ShadowColor ("Shadow Color", Color) = (0, 0, 0, 1)
		_Offset ("Offset", Float) = 0
        _Scale ("Scale", Float) = 0.1
        _Range ("Range", Float) = 0
    }
    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent+1" }
        //Tags { "RenderType" = "AlphaTest" "Queue" = "AlphaTest" }
		
        Blend SrcAlpha OneMinusSrcAlpha
        ZTest Always
        ZWrite Off
	    Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "PerlinNoise3D.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
				float3 worldPos : TEXCOORD1;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _ShadowColor;
            float4 _Color;
            float _Offset;
            float _Scale;
            float _Range;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float3 objPos = unity_ObjectToWorld._m03_m13_m23;

                half4 texcol = tex2D(_MainTex, i.uv);
				
                float dist = length(i.worldPos - objPos) - _Offset;
                dist /= _Range;
                //dist = dist * 2.0 - 1.0;

                //return float4(dist, dist, dist, 1.0);
				
                //float PerlinNoise_Octaves(float3 inCoord, float scale, float3 speed, uint octaveNumber, float octaveScale, float octaveAttenuation, float time)
                float noise1 = PerlinNoise_Octaves(i.worldPos, _Scale, float3(0, 1, 0), 1, 2.0, 1.5, _Time.y * 0) + dist;
                //noise1 = floor(noise1 * 16) / 16;
                //float noise = PerlinNoise_Octaves(i.worldPos, 0.1, float3(0, 1, 0), 4, 0.5, 0.5, _Time.y * 0);
                //return float4(noise.xxx, 1.0) + float4(_Offset, _Offset, _Offset, 0);
                return float4(texcol.rgb, clamp(noise1, 0, 1));

                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                return col * _Color;
            }
            ENDCG
        }
    }
}
