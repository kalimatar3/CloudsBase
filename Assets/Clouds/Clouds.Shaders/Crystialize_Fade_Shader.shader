Shader "Custome/Crystialize_Fade_Shader"
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
        _CrystializeTex("Crystialize Texture", 2D) = "white" {}

        _TimeScale("Time Scale",float) = 0
        _AlphaValue("AlphaValue",float) = 0
        _ColorOne("Color One",Color) = (1,1,1,1)
        _ColorTwo("Color Two",Color) = (1,1,1,1)
        _ColorThree("Color Three",Color) = (1,1,1,1)

       _MaskTex("Mask Texture",2D) = "white" {}
       _RevealValue("Reveal Value",float) = 0
       _SmoothFeather("Smooth Feather",float) = 0
       _FeartherValue("Fearther Value",float) = 0
       _EnrobeColor("Enrobe Color",color) = (1,1,1,1)

    }
    SubShader
    {
        Blend [_SrcFactor] [_DstFactor]
        BlendOp [_Opp]
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            sampler2D _MainTex;

            sampler2D _CrystializeTex;

            sampler2D _MaskTex;

            float4 _CrystializeTex_ST;
            float4 _MainTex_ST;
            float4 _MaskTex_ST;

            fixed4 _ColorOne;
            fixed4 _ColorTwo;
            fixed4 _ColorThree;

            float _TimeScale;
            float _AlphaValue;

            float4 _EnrobeColor;
            float _RevealValue;
            float _SmoothFeather;
            float _FeartherValue;

            struct appdata
            {
                float4 vertex : POSITION;
                float4 uv : TEXCOORD0;
                float2 uv2 : TEXCOORD2;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 uv : TEXCOORD0;
                float2 uv2 :TEXCOORD2;
                float3 viewDir : TEXCOORD1;
            };

             float3 Plasma(fixed2 uv) {
                float time = _TimeScale;
                float w1 = sin((uv.x + time) * 13) ;
                float w2 = sin((uv.y + time) *  20);
                float3 w3 = sin( (uv.x  + 0.5 + uv.y + time) * 12);

                fixed2 offset = fixed2(0.3 ,0.3);
                fixed2 normalizeuv = uv - offset;
                float r = sin( (sqrt(normalizeuv.x * normalizeuv.x + normalizeuv.y * normalizeuv.y) + time)  * 9);

                float finalpoint = w1 + w2 + w3 + r;


                float3 col1 = _ColorOne * sin(finalpoint * UNITY_PI);
                float3 col2 = _ColorTwo * cos(finalpoint * UNITY_PI);
                float3 col3  = _ColorThree * finalpoint;
                float3 finalcol = (col1 + col2 + col3)/3 ;
                return finalcol  + 0.5;
            }

           v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv.xy = TRANSFORM_TEX(v.uv.xy, _MainTex);
                o.uv.zw =  TRANSFORM_TEX(v.uv.xy, _MaskTex);
                o.uv2 = TRANSFORM_TEX(v.uv.xy, _CrystializeTex);
                o.viewDir = WorldSpaceViewDir(v.vertex);
                return o;
            }


            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv.xy);
                fixed4 crystalcolor = tex2D(_CrystializeTex,i.uv2);
                float2 newuv = i.viewDir.xy * 0.1 + crystalcolor * 0.9;
                fixed3 plasma = Plasma(newuv) * col.a ;
                col.rgb = col.rgb * 0.7 + plasma * 0.5;


                fixed4 maskcol = tex2D(_MaskTex,i.uv2);
                float RevealAmountTop = smoothstep(maskcol.r - _SmoothFeather,maskcol.r + _SmoothFeather,_RevealValue + _FeartherValue);
                float RevealAmountDown = smoothstep(maskcol.r - _SmoothFeather,maskcol.r + _SmoothFeather,_RevealValue - _FeartherValue);
                float RevealDiff = RevealAmountTop - RevealAmountDown;
                float3 finalcolor = lerp(col.rgb,col.rgb, RevealDiff);
                fixed4 finaltext = fixed4(finalcolor,col.a * RevealAmountTop );
                finaltext.a *= _AlphaValue;
                return finaltext;
            }
            ENDCG
        }
    }
}
