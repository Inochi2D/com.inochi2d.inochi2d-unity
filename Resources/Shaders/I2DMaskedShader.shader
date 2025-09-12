// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Inochi2D/I2DMaskedShader"
{
    Properties
    {
        _Mask ("Mask", 2D) = "white" {}
        _Albedo ("Albedo", 2D) = "white" {}
        _Emission ("Emission", 2D) = "white" {}
        _Bumpmap ("Bumpmap", 2D) = "white" {}
        _BlendSrc ("Source RGB", Integer) = 1
        _BlendDst ("Destination RGB", Integer) = 1
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        Pass
        {
            ZWrite Off
            Cull Off
            Blend [_BlendSrc] [_BlendDst]

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct VtxData {
                float2 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct VtxOut {
                float4 vtx : SV_POSITION;
                float2 uv : TEXCOORD0;
                float2 ndcCoords : COLOR0;
            };
            
            sampler2D _MaskTex;
            sampler2D _AlbedoTex;
            sampler2D _EmissionTex;
            sampler2D _BumpmapTex;

            VtxOut vert (VtxData IN) {
                VtxOut OUT;
                OUT.vtx = mul(UNITY_MATRIX_VP, float4(IN.vertex.x, -IN.vertex.y, 0, 1));
                OUT.uv = IN.uv;
                OUT.ndcCoords = (OUT.vtx.xy * 0.5 + OUT.vtx.w * 0.5) / OUT.vtx.w;
                return OUT;
            }

            struct FragOut {
                float4 _AlbedoOut : COLOR0;
                float4 _EmissionOut : COLOR1;
                float4 _BumpmapOut : COLOR2;
            };

            FragOut frag (VtxOut IN) {
                FragOut OUT;

                // Clamp to Border Color (Transparent)
                if (min(IN.uv.x, IN.uv.y) < 0.0 || max(IN.uv.x, IN.uv.y) > 1.0) {
                    OUT._AlbedoOut = float4(0, 0, 0, 0);
                    OUT._EmissionOut = float4(0, 0, 0, 0);
                    OUT._BumpmapOut = float4(0, 0, 0, 0);
                    return OUT;
                }
                
                // NOTE:    Unity handles sRGB weirdly, as such we handle it here instead.
                //          This is technically suboptimal but I have no idea how otherwise
                //          to do it sensibly right now.
                float4 albedo = pow(tex2D(_AlbedoTex, IN.uv), 2.2);
                float4 mask   = tex2D(_MaskTex, IN.ndcCoords).rrrr;
                
                OUT._AlbedoOut = albedo.rgba * mask;
                OUT._EmissionOut = tex2D(_EmissionTex,  IN.uv)*mask;
                OUT._BumpmapOut = tex2D(_BumpmapTex,    IN.uv)*mask;
                return OUT;
            }
            ENDCG
        }
    }
}
