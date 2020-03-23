﻿Shader "Hidden/LineShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_Width ("Width", Range(0, 0.1)) = 0.025
    }
    SubShader
    {
		Tags { "Queue"="Transparent" "RenderType"="Opaque"}
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma geometry geo
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
				float4 color : COLOR;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
				float4 color : COLOR;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = v.vertex;
				o.color = v.color;
                o.uv = v.uv;
                return o;
            }

			v2f VertexOutput(float3 wpos, v2f input) {
				v2f o = input;
				o.vertex = UnityObjectToClipPos(float4(wpos, 1.0));
				return o;
			}

			float _Width;

			[maxvertexcount(6)]
			void geo(line v2f input[2], inout TriangleStream<v2f> outStream)
			{
				float4 wp0 = input[0].vertex;
				float4 wp1 = input[1].vertex;

				float2 lineVec = normalize(wp1.xy - wp0.xy);
				float2 normalVec = normalize(lineVec.yx * float2(1.0, -1.0));

				outStream.Append(VertexOutput(wp0 + _Width * float3(normalVec, 0.0), input[0]));
				outStream.Append(VertexOutput(wp1 + _Width * float3(normalVec, 0.0), input[1]));
				outStream.Append(VertexOutput(wp1 - _Width * float3(normalVec, 0.0), input[1]));
				outStream.RestartStrip();

				outStream.Append(VertexOutput(wp0 + _Width * float3(normalVec, 0.0), input[0]));
				outStream.Append(VertexOutput(wp1 - _Width * float3(normalVec, 0.0), input[1]));
				outStream.Append(VertexOutput(wp0 - _Width * float3(normalVec, 0.0), input[0]));
				outStream.RestartStrip();
			}
			
            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = i.color;
                return col;
            }
            ENDCG
        }
    }
}