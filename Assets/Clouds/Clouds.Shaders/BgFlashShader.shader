Shader "UI/BgFlashShader"
{
    Properties
    {
        [Enum(UnityEngine.Rendering.BlendMode)]
       _SrcFactor("Src Factor",float) = 5
       [Enum(UnityEngine.Rendering.BlendMode)]
       _DstFactor("Dst Factor",float) = 10
       [Enum(UnityEngine.Rendering.BlendOp)]
       _Opp("Operation",float) = 0

        _MainTex ("Texture", 2D) = "white" {}
        _MaskTex("MaskTex",2D) = "White" {}
        _MaskColor("Mask Color",color) = (1,1,1,1)
        _RevealValue("Reveal Value",float) = 0
        _MaskAlpha("Mask alpha",Range(0,1)) = 0
        _EdgeThreshold("EdgeThreshold",Range(0,1)) = 0
        _ImageSize("Image Size", Vector) = (1080, 1920, 0, 0)
        _CanvasSize("Canvas Size", Vector) = (1080, 1920, 0, 0)
        _MovingValue("Moving value",Range(-1,1)) = 0
    }
    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        Blend [_SrcFactor] [_DstFactor]
        BlendOp [_Opp]

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float4 uv : TEXCOORD0;
            };
            sampler2D _MainTex;
            sampler2D _MaskTex;
            float4 _MainTex_ST;
            float4 _MaskTex_ST;
            fixed4 _ImageSize;
            fixed4 _CanvasSize;

            fixed4 _MaskColor;
            float _RevealValue;
            float _MaskAlpha;
            float _MovingValue;
            float _EdgeThreshold;

            struct v2f
            {
                float4 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            float EdgeDetect(float2 uv)
            {
                // Lấy texel size
                float2 texel = float2(1.0 / _ImageSize.x, 1.0 / _ImageSize.y);

                // Lấy mẫu 8 hướng
                float3 tl = tex2D(_MainTex, uv + texel * float2(-1,  1)).rgb; 
                float3  l = tex2D(_MainTex, uv + texel * float2(-1,  0)).rgb;
                float3 bl = tex2D(_MainTex, uv + texel * float2(-1, -1)).rgb;

                float3  t = tex2D(_MainTex, uv + texel * float2( 0,  1)).rgb;
                float3  b = tex2D(_MainTex, uv + texel * float2( 0, -1)).rgb;

                float3 tr = tex2D(_MainTex, uv + texel * float2( 1,  1)).rgb; 
                float3  r = tex2D(_MainTex, uv + texel * float2( 1,  0)).rgb;
                float3 br = tex2D(_MainTex, uv + texel * float2( 1, -1)).rgb;

                // Chuyển sang độ sáng để xử lý cạnh nhanh hơn
                float tlL = dot(tl, float3(.299,.587,.114));
                float lL  = dot(l , float3(.299,.587,.114));
                float blL = dot(bl, float3(.299,.587,.114));

                float tL  = dot(t , float3(.299,.587,.114));
                float bL  = dot(b , float3(.299,.587,.114));

                float trL = dot(tr, float3(.299,.587,.114));
                float rL  = dot(r , float3(.299,.587,.114));
                float brL = dot(br, float3(.299,.587,.114));

                // Sobel kernel X
                float gx = (trL + 2*rL + brL) - (tlL + 2*lL + blL);

                // Sobel kernel Y
                float gy = (blL + 2*bL + brL) - (tlL + 2*tL + trL);

                // Độ mạnh cạnh
                float edge = sqrt(gx*gx + gy*gy);

                return edge;
            }
            float ExpandEdge(float2 uv)
            {
                float2 texel = 1.0 / _ImageSize.xy;

                float e = 0.0;
                e = max(e, EdgeDetect(uv));
                e = max(e, EdgeDetect(uv + texel * float2(1, 0)));
                e = max(e, EdgeDetect(uv + texel * float2(-1, 0)));
                e = max(e, EdgeDetect(uv + texel * float2(0, 1)));
                e = max(e, EdgeDetect(uv + texel * float2(0,-1)));

                return saturate(e);
            }
            float BlurEdge(float2 uv)
            {
                float2 texel = 1.0 / _ImageSize.xy;
                float sum = 0;
                float w0 = 0.40;
                float w1 = 0.25;
                float w2 = 0.15;

                sum += ExpandEdge(uv) * w0;
                sum += ExpandEdge(uv + texel * float2(1,0)) * w1;
                sum += ExpandEdge(uv - texel * float2(1,0)) * w1;
                sum += ExpandEdge(uv + texel * float2(0,1)) * w2;
                sum += ExpandEdge(uv + texel * float2(0,-1)) * w2;

                return sum;
            }
            fixed2 rotate(fixed2 uv,float degreeAngle) {
                fixed2 o;
                float rad = radians(degreeAngle);
                float s = sin(rad);
                float c = cos(rad);
                fixed2x2 rotationMatrix = float2x2(c, -s, s, c);
                fixed2 pivot = float2(0.5,1);
                uv -= pivot;
                fixed2 uvImage = mul(rotationMatrix, uv);
                uvImage += pivot;
                o = uvImage;
                return o;
            }
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv.xy = TRANSFORM_TEX(v.uv, _MainTex);
                o.uv.zw = TRANSFORM_TEX(v.uv, _MaskTex);

                float degreeAngle = _MovingValue * 10;
                o.uv.zw = rotate( o.uv.xy,degreeAngle);
                return o;
            }
            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv.xy);
                fixed4 maskcol = tex2D(_MaskTex, i.uv.zw);

                float edge = BlurEdge(i.uv.xy);
                float anim =  sin(_Time.y) *0.5 + 0.5; 
                fixed4 finalcol = maskcol.a;
                finalcol.rgb = lerp(finalcol.rgb, maskcol.a, (1 - edge) * _MaskAlpha);
                return finalcol;
            }
            ENDCG
        }
    }
}
