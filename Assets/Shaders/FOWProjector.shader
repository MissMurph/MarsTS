Shader "Custom/FOWProjector" {
    Properties {
        _DynamicFog("Dynamic", 2D) = "white" {}
        _Color ("Color", Color) = (0,0,0,0.7)
        _Darkness ("Unexplored Darkness", Float) = 0.3
        _Smoothness ("Feather", Range(0,1)) = 0.005
    }

    Subshader {
        Tags {
            "RenderType" = "Transparent"
            "Queue" = "Transparent+100"
        }
        Pass {
            ZWrite Off
            Offset -1, -1

            Fog{ Mode Off }

            Blend OneMinusSrcAlpha SrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma fragmentoption ARB_fog_exp2
            #pragma fragmentoption ARB_precision_hint_fastest
            #include "UnityCG.cginc"

            struct v2f
            {
                float4 pos : SV_POSITION;
                float4 uv : TEXCOORD0;
            };

            sampler2D _DynamicFog;
            float4x4 unity_Projector;
            float4 _Color;
            float _Darkness;
            float _Smoothness;

            v2f vert(appdata_tan v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = mul(unity_Projector, v.vertex);
                return o;
            }

            half4 frag(v2f i) : COLOR
            {
                half4 tex = tex2Dproj(_DynamicFog, i.uv);

                tex.a = tex.r;
                tex.r = 0;

                if (tex.a <= _Darkness) {
                    tex.a = _Darkness;
                }

                if (i.uv.w < 0)
                {
                    tex = float4(0,0,0,1);
                }

                half4 gaussianH   = tex2Dproj (_DynamicFog, i.uv + float4(-_Smoothness,0,-_Smoothness,0))*0.25;
                gaussianH  += tex2Dproj (_DynamicFog, i.uv                         )*0.5  ;
                gaussianH  += tex2Dproj (_DynamicFog, i.uv + float4( _Smoothness,0,_Smoothness,0))*0.25;
 
                half4 gaussianV   = tex2Dproj (_DynamicFog, i.uv + float4(0,-_Smoothness,0,-_Smoothness))*0.25;
                gaussianV  += tex2Dproj (_DynamicFog, i.uv                        ) *0.5  ;
                gaussianV  += tex2Dproj (_DynamicFog, i.uv + float4(0, _Smoothness,0, _Smoothness))*0.25;
 
                half4 blurred    = (gaussianH+ gaussianV)*0.5;

                tex.a = tex.a - blurred.g;

                
                return tex;
            }
            ENDCG
        }
    }
}