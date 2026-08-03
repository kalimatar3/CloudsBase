Shader "Custom/AuroraShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Offset ("Offset",float) = 0

        [Enum(UnityEngine.Rendering.BlendMode)]
       _SrcFactor("Src Factor",float) = 5
       [Enum(UnityEngine.Rendering.BlendMode)]
       _DstFactor("Dst Factor",float) = 10
       [Enum(UnityEngine.Rendering.BlendOp)]
       _Opp("Operation",float) = 0


    }
    SubShader
    {
        Tags {"renderType"= "Opaque"}
        Blend [_SrcFactor] [_DstFactor]
        BlendOp [_Opp]

        Cull Off 
        ZWrite Off 
        ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            sampler2D _MainTex;
            float _Offset;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }


            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
               // float2 newuv = float2(,cos(i.uv.x -_Offset));
                return col;
            }
            ENDCG
        }
    }
}
