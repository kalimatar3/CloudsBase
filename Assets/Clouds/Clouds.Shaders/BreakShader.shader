Shader "Custom/BreakShader"
{
    Properties
    {
       [Enum(UnityEngine.Rendering.BlendMode)]
       _SrcFactor("Src Factor",float) = 5
       [Enum(UnityEngine.Rendering.BlendMode)]
       _DstFactor("Dst Factor",float) = 10
       [Enum(UnityEngine.Rendering.BlendOp)]
       _Opp("Operation",float) = 0
       
       _MainTex("Main Textture", 2D) = "White" {}
       _MaskTexture ("Mask Texture",2D) = "white" {}
       _RevealValue("Reveal Value",float) = 0
       _SmoothFeather("Smooth Feather",float) = 0
       _FeartherValue("Fearther Value",float) = 0
       _EnrobeColor("Enrobe Color",color) = (1,1,1,1)

    }
    SubShader
    {
        Tags {"renderType"= "Opaque"}
        LOD 100
        Blend [_SrcFactor] [_DstFactor]
        BlendOp [_Opp]
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            sampler2D _MainTex;
            sampler2D _MaskTexture;

            float4 _MainTex_ST;
            float4 _MaskTexture_ST;
            float4 _EnrobeColor;
            float _RevealValue;
            float _SmoothFeather;
            float _FeartherValue;

            struct appdata
            {
                float4 vertex : POSITION;
                float4 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv.xy = TRANSFORM_TEX(v.uv, _MainTex);
                o.uv.zw = TRANSFORM_TEX(v.uv, _MaskTexture);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 textureColor = tex2D(_MainTex,i.uv.zw);  
                fixed4 maskcol = tex2D(_MaskTexture,i.uv.zw) ;
                float RevealAmountTop = smoothstep(maskcol.r - _SmoothFeather,maskcol.r + _SmoothFeather,_RevealValue + _FeartherValue);
                float RevealAmountDown = smoothstep(maskcol.r - _SmoothFeather,maskcol.r + _SmoothFeather,_RevealValue - _FeartherValue);
                float RevealDiff = RevealAmountTop - RevealAmountDown;
                float3 finalcolor = lerp(textureColor.rgb,_EnrobeColor.rgb, RevealDiff);
                fixed4 finaltext = fixed4(finalcolor,textureColor.a * RevealAmountTop );
                return finaltext;
            }
            ENDCG
        }
    }
}
