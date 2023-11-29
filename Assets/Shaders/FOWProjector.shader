Shader "Custom/FOWProjector" {
    Properties {
        _DynamicFog("Dynamic", 2D) = "white" {}
        _StaticFog("Static", 2D) = "white" {}
        _Color ("Color", Color) = (0,0,0,0.7)
        _Darkness ("Unexplored Darkness", Float) = 0.3
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
            sampler2D _StaticFog;
            float4x4 unity_Projector;
            float4 _Color;
            float _Darkness;

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

                half4 tex2 = tex2Dproj(_StaticFog, i.uv);

                tex *= tex2;

                if (tex.a <= _Darkness) {
                    tex.a = _Darkness;
                }

                tex.a = tex.a - _Color.a;

                if (i.uv.w < 0)
                {
                    tex = float4(0,0,0,1);
                }
                return tex;
            }
            ENDCG
        }
    }
}