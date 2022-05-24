Shader "Custom/Clouds2"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1, 1, 1, 1)
		_Clouds ("Clouds", Vector) = (0, 0, 0, 0)
        _SDF ("SDF", Vector) = (0, 0, 0, 0)
        _Offset ("Offset", Float) = 0
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
                float2 uv1 : TEXCOORD0;
                float2 uv2 : TEXCOORD1;
				float3 worldPos : TEXCOORD2;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _Color;
            float4 _Clouds;
            float4 _SDF;
            float _Offset;
            float _Range;

            v2f vert (appdata v)
            {
                v2f o;
				
                float4 t = _Time.y * _Clouds * 0.1;

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                o.uv1 = t.xy + v.uv * _SDF.zz;
                o.uv2 = t.zw + v.uv * _SDF.ww;
				
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float3 objPos = unity_ObjectToWorld._m03_m13_m23;

                float dist = length(i.worldPos - objPos) - _Offset + _Range * 0.5;
                dist /= _Range;
                //float d = sqrt(dot(pos, pos));

				fixed4 texCol1 = tex2D(_MainTex, i.uv1);
			    fixed4 texCol2 = tex2D(_MainTex, i.uv2);

                fixed4 res = (texCol1 + texCol2) * 0.5;
                fixed alpha = res.a + dist + (-_SDF.x);
                alpha *= 1 / ((-_SDF.x) + _SDF.y);

                alpha = saturate(alpha);
                fixed mapped = 3 + alpha * -2;

                return fixed4(res.rgb, alpha * alpha * mapped) * _Color;
            }
            ENDCG
        }
    }
}
