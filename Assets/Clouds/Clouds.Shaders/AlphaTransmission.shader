Shader "Custom/TextureBlend"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}
        _SubTex ("Sub Texture", 2D) = "white" {}
        _BlendValue ("Blend Value", Range(0, 1)) = 0
        _ShakeAmplitude ("Shake Amplitude", Range(0, 1)) = 0.1
        _ShakeSpeed ("Shake Speed", float) = 1.0
        _ShakeAngle ("Shake Angle", Range(0, 360)) = 0
        _WaveCount ("Wave Count", Range(1, 10)) = 3
        _WaveAmplitude ("Wave Amplitude", Range(0, 0.1)) = 0.01
        _WaveFrequency ("Wave Frequency", float) = 10.0
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
            float _BlendValue;
            float _ShakeAmplitude;
            float _ShakeSpeed;
            float _ShakeAngle;
            int _WaveCount;
            float _WaveAmplitude;
            float _WaveFrequency;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color;

                // Calculate shake offset
                float shake = sin(_BlendValue) * _ShakeAmplitude;
                float rad = radians(_ShakeAngle);
                float2 shakeOffset = float2(shake * cos(rad), shake * sin(rad));

                // Calculate wave offset
                float waveAngle = _BlendValue * 2 * 3.14159; // angle based on blend
                float2 waveDir = float2(cos(waveAngle), sin(waveAngle));
                float2 perpDir = float2(-sin(waveAngle), cos(waveAngle));
                float position = dot(o.uv, waveDir);
                float distortion = 0;
                for(int j = 0; j < _WaveCount; j++){
                    distortion += sin(_WaveFrequency * position + j * 2 * 3.14159 / _WaveCount) * _WaveAmplitude;
                }
                float2 waveOffset = distortion * perpDir;

                float2 totalOffset = shakeOffset + waveOffset;
                v.vertex.xy += totalOffset;

                o.pos = UnityObjectToClipPos(v.vertex);

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 mainCol = tex2D(_MainTex, i.uv);
                fixed4 subCol = tex2D(_SubTex, i.uv);

                fixed4 finalCol = lerp(mainCol, subCol, _BlendValue);

                return finalCol * i.color;
            }
            ENDCG
        }
    }
}