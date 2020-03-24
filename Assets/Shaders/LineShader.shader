Shader "Hidden/LineShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_MinWidth ("Minimum Width", Range(0, 0.2)) = 0.01
		_MaxWidth ("Maximum Width", Range(0, 0.2)) = 0.05
		_NegativeColor("Negative Color", Color) = (0.0, 0.0, 0.0, 0.0)
		_PositiveColor("Positive Color", Color) = (1.0, 1.0, 1.0, 1.0)
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

			v2f VertexOutput(float2 wpos, float v, v2f input) {
				v2f o = input;
				o.uv.x = o.vertex.x; // Save the weight of the connection before matrix multiplying 
				o.uv.y = v;
				o.vertex = UnityObjectToClipPos(float4(wpos, 0.0, 1.0));
				return o;
			}

			float _MinWidth;
			float _MaxWidth;

			float Width(float weight)
			{
				float t = abs(weight) / 5.0;
				return lerp(_MinWidth, _MaxWidth, t);
			}

			[maxvertexcount(6)]
			void geo(point v2f input[1], inout TriangleStream<v2f> outStream)
			{
				float2 wp0 = input[0].color.xy;
				float2 wp1 = input[0].color.zw;

				float2 lineVec = normalize(wp1 - wp0);
				float2 normalVec = normalize(lineVec.yx * float2(1.0, -1.0));
				float weight = input[0].vertex.x;
				float width = Width(weight);

				outStream.Append(VertexOutput(wp0 + width * normalVec, 0.0, input[0]));
				outStream.Append(VertexOutput(wp1 + width * normalVec, 1.0, input[0]));
				outStream.Append(VertexOutput(wp1 - width * normalVec, 1.0, input[0]));
				outStream.RestartStrip();

				outStream.Append(VertexOutput(wp0 + width * normalVec, 0.0, input[0]));
				outStream.Append(VertexOutput(wp1 - width * normalVec, 1.0, input[0]));
				outStream.Append(VertexOutput(wp0 - width * normalVec, 0.0, input[0]));
				outStream.RestartStrip();
			}

			float4 _NegativeColor;
			float4 _PositiveColor;

			float3 colLookup(float value) 
			{
				float3 negative = _NegativeColor.xyz;
				float3 positive = _PositiveColor.xyz;
				float3 white = float3(1.0, 1.0, 1.0);

				if (value > 0.0) {
					return lerp(white, positive, sqrt(value / 5.0));
				}
				else {
					return lerp(white, negative, sqrt(-value / 5.0));
				}
			}

			float4 color(float2 uv) 
			{
				// Don't uncomment
				//bright = pow(bright, _line_gradient_pow);
				//bright = lerp(_dark, _bright, bright);

				//float cdist = abs(uv.y-0.5);
				//float bright = pow(cdist * 2.0, 2);//(cdist < 0.25 ? 0.0 : 1.0);
				//bright = clamp(1.0-bright, 0.0, 1.0);
				//bright = pow(bright, 5.0);
				//bright = lerp(0.5, 0.9, bright);
				float bright = 1.0;
				float3 col = colLookup(uv.x) * bright;
				float4 c = float4(col, 1.0);
				return c;
			}
			
            fixed4 frag (v2f i) : SV_Target
            {
				return color(i.uv);
            }
            ENDCG
        }
    }
}
