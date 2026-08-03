Shader "Custom/WaterTransmission"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _SubTex("SubTexture",2D) = "white" {}
        _Value("value",float) = 0
        _Offset("Offset",float) = 0 
        _Center ("Center (UV)", Vector) = (0.5, 0.5, 0, 0)
        _Speed ("Speed", float) = 1.0
        _Frequency ("Frequency", float) = 20.0
        _Amplitude ("Amplitude", float) = 0.05
        _Width ("Ripple Width", Range(0.0, 0.5)) = 0.1

        _Border ("Border (L B R T)", Vector) = (0,0,0,0)
        _Size ("UI Size (W H)", Vector) = (100,100,0,0)
        _SpriteSize ("Sprite Size (W H)", Vector) = (100,100,0,0)
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            sampler2D _SubTex;
            float4 _MainTex_ST;
            float4 _SubTex_ST;
            float4 _Center;
            float _Speed;
            float _Frequency;
            float _Amplitude;
            float _Width;
            float _Value;
            float _Offset;
            float4 _Border;
            float2 _Size;
            float2 _SpriteSize;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float4 uv : TEXCOORD0;
                float4 color : COLOR;
                float3 worldPos : TEXCOORD1;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv.xy = TRANSFORM_TEX(v.uv, _MainTex);
                o.uv.zw = TRANSFORM_TEX(v.uv, _SubTex);
                o.worldPos = v.vertex;
                o.color = v.color;
                return o;
            }
            float2 MapUVToSliced(float2 uv, float4 border, float2 size, float2 spriteSize) 
            {
                float2 pixelPos = uv * size; // Chuyển UV về tọa độ pixel trên UI
                float2 resultPixel;

                // --- Xử lý trục X ---
                if (pixelPos.x < border.x) { // Vùng bên trái
                    resultPixel.x = pixelPos.x;
                } 
                else if (pixelPos.x > size.x - border.z) { // Vùng bên phải
                    resultPixel.x = spriteSize.x - (size.x - pixelPos.x);
                } 
                else { // Vùng giữa (Stretch)
                    float normalizedMid = (pixelPos.x - border.x) / (size.x - border.x - border.z);
                    resultPixel.x = border.x + normalizedMid * (spriteSize.x - border.x - border.z);
                }

                // --- Xử lý trục Y ---
                if (pixelPos.y < border.y) { // Vùng dưới
                    resultPixel.y = pixelPos.y;
                } 
                else if (pixelPos.y > size.y - border.w) { // Vùng trên
                    resultPixel.y = spriteSize.y - (size.y - pixelPos.y);
                } 
                else { // Vùng giữa (Stretch)
                    float normalizedMid = (pixelPos.y - border.y) / (size.y - border.y - border.w);
                    resultPixel.y = border.y + normalizedMid * (spriteSize.y - border.y - border.w);
                }

                return resultPixel / spriteSize; // Chuyển ngược về UV (0-1) để sample
            }         
            fixed4 frag (v2f i) : SV_Target
            {
                float2 objectCenter = mul(unity_ObjectToWorld, float4(0,0,0,1)).xy;

                float2 dir = (i.worldPos.xy - objectCenter)/1920;

                float dist = length(dir);

                float timeCycle = frac(_Value * _Speed);

                float rippleMask =
                    smoothstep(timeCycle - _Width, timeCycle, dist) *
                    smoothstep(timeCycle + _Width, timeCycle, dist);

                float distortion =
                    sin(dist * _Frequency - _Value * _Speed * 10.0) *
                    _Amplitude * rippleMask;

                float rippleMask2 =
                    smoothstep(timeCycle - _Width, timeCycle, dist + _Offset) *
                    smoothstep(timeCycle + _Width, timeCycle, dist + _Offset);

                float distortion2 =
                    sin(dist * _Frequency - _Value * _Speed * 10.0) *
                    _Amplitude * rippleMask2;

                float2 distortedUV = i.uv.xy + normalize(dir) * (distortion2 + distortion);
                float2 finalUV = MapUVToSliced(distortedUV, _Border, _Size, _SpriteSize);
    
                fixed4 col = tex2D(_MainTex, finalUV);
                fixed4 col2 = tex2D(_SubTex, finalUV);

                float ratio = smoothstep(0,1,_Value);

                fixed4 finalcol = ratio * col2 + (1 - ratio) * col;

                return finalcol * i.color;
            }   
            ENDCG
        }
    }
}