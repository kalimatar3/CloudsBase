Shader "CustomRenderTexture/GlowShader"
{
    Properties
    {
        _MainTex("InputTex", 2D) = "white" {}
        _ColorBlend("Color Blend",color) = (1,1,1,1)
        _BlendValue("Blend Value",float) = 0

       [Enum(UnityEngine.Rendering.BlendMode)]
       _SrcFactor("Src Factor",float) = 5
       [Enum(UnityEngine.Rendering.BlendMode)]
       _DstFactor("Dst Factor",float) = 10
       [Enum(UnityEngine.Rendering.BlendOp)]
       _Opp("Operation",float) = 0

    }

     SubShader
     {
        ZWrite Off
        ZTest Always
        Tags {"renderType"= "Opaque"}
        Blend [_SrcFactor] [_DstFactor]
        BlendOp [_Opp]
        Pass
        {
            Name "GlowShader"

            CGPROGRAM

           #include "UnityCustomRenderTexture.cginc"

            #pragma vertex vert
            #pragma fragment frag

            sampler2D   _MainTex;
            fixed4 _ColorBlend;
            float _BlendValue;


            struct appdata {
                float4 vertex  : POSITION;
                float2 uv  : TEXCOORD0;
            };
            struct v2f {
                float4 vertex  : SV_POSITION;
                float2 uv  : TEXCOORD0;                
            };
            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }
            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex,i.uv);
                float anim =  _BlendValue;
                fixed4 finalcol = col.a * fixed4(_ColorBlend.xyz,1 * anim);
                return finalcol;
            }
            ENDCG
        }
    }
}
