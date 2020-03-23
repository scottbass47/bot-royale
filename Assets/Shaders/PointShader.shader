Shader "Hidden/PointShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_Size ("Size", Range(0, 1)) = 0.5 
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
                o.uv = v.uv;
				o.color = v.color;
                return o;
            }

			v2f VertexOutput(float2 pos, float2 uv, v2f input) 
			{
				v2f o = input;
				o.uv = uv;
				o.vertex = UnityObjectToClipPos(float4(pos, 0.0, 0.0));
				return o;
			}

			float _Size;

			[maxvertexcount(6)]
			void geo(point v2f input[1], inout TriangleStream<v2f> outStream)
			{
				float2 wp = input[0].vertex.xy;
				float2 wp1 = float2(wp.x - _Size, wp.y - _Size);
				float2 wp2 = float2(wp.x - _Size, wp.y + _Size);
				float2 wp3 = float2(wp.x + _Size, wp.y + _Size);
				float2 wp4 = float2(wp.x + _Size, wp.y - _Size);

				outStream.Append(VertexOutput(wp1, float2(0,0), input[0]));
				outStream.Append(VertexOutput(wp2, float2(0,1), input[0]));
				outStream.Append(VertexOutput(wp3, float2(1,1), input[0]));
				outStream.RestartStrip();

				outStream.Append(VertexOutput(wp1, float2(0,0), input[0]));
				outStream.Append(VertexOutput(wp3, float2(1,1), input[0]));
				outStream.Append(VertexOutput(wp4, float2(1,0), input[0]));
				outStream.RestartStrip();
			}

            fixed4 frag (v2f i) : SV_Target
            {
				float2 dist = i.uv - float2(0.5, 0.5);
				clip(dot(dist,dist) < _Size * _Size ? 1.0 : -1.0);
                return i.color;
            }
            ENDCG
        }
    }
}
