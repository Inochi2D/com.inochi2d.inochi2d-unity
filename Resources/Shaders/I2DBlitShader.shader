// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'
Shader "Inochi2D/I2DBlitShader"
{
    Properties
    {
        _MaskTex ("Mask", 2D) = "white" {}
        _MainTex ("Albedo", 2D) = "white" {}
        _BlendSrc ("Source RGB", Integer) = 1
        _BlendDst ("Destination RGB", Integer) = 1
    }
    SubShader
    {
        Pass
        {
            ZWrite Off
            ZTest Always
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
            sampler2D _MainTex;

            VtxOut vert (VtxData IN) {
                VtxOut OUT;
                OUT.vtx = UnityObjectToClipPos(float4(IN.vertex.x, IN.vertex.y, 0, 1));
                OUT.uv = IN.uv;
                OUT.ndcCoords = (OUT.vtx.xy * 0.5 + OUT.vtx.w * 0.5) / OUT.vtx.w;
                return OUT;
            }

            struct FragOut {
                float4 _MainOut : COLOR0;
            };

            FragOut frag (VtxOut IN) {
                // sample the texture
                FragOut OUT;
                
                // NOTE:    Unity handles sRGB weirdly, as such we handle it here instead.
                //          This is technically suboptimal but I have no idea how otherwise
                //          to do it sensibly right now.
                float4 albedo = tex2D(_MainTex, IN.uv);
                float4 mask   = tex2D(_MaskTex, IN.ndcCoords).rrrr;

                OUT._MainOut = albedo*mask;
                return OUT;
            }
            ENDCG
        }
    }
}
