// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Inochi2D/I2DMaskShader"
{
    Properties
    {
        _Mask ("Albedo", 2D) = "white" {}
    }
    SubShader
    {
        Pass
        {
            ZWrite Off
            Cull Off
            Blend One One

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct VtxData {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct VtxOut {
                float4 vtx : SV_POSITION;
                float2 uv : TEXCOORD0;
            };
            
            sampler2D _MaskTex;

            VtxOut vert (VtxData IN) {
                VtxOut OUT;
                OUT.vtx = mul(UNITY_MATRIX_VP, float4(IN.vertex.x, -IN.vertex.y, 0, 1));
                OUT.uv = IN.uv;
                return OUT;
            }

            struct FragOut {
                float _MaskOut : COLOR0;
            };

            FragOut frag (VtxOut IN) {
                // sample the texture
                FragOut OUT;
                OUT._MaskOut = tex2D(_MaskTex, IN.uv).a;
                return OUT;
            }
            ENDCG
        }
    }
}
