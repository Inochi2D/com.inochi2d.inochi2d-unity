Shader "PartShader"
{
    Properties
    {
        [Header(Textures)]
        [NoScaleOffset] _MainTex("Albedo", 2D) = "white" {}
        [NoScaleOffset] _Emission("Emission Map", 2D) = "white" {}
        _EmissionStrength("Emission Strength", Float) = 0
        [Normal] _BumpMap("Normal Map", 2D) = "bump" {}

        _Opacity("Opacity", Float) = 1
        _MultColor("Multiply Color", Color) = (1, 1, 1)
        _ScreenColor("Screen Color", Color) = (0, 0, 0)
        [Space(5)]

        [Header(Blending)]
        [Enum(UnityEngine.Rendering.BlendOp)] _BlendOp("Blend Op", Float) = 1
        [Enum(UnityEngine.Rendering.BlendMode)] _BlendSrc("Src Factor", Float) = 1
        [Enum(UnityEngine.Rendering.BlendMode)] _BlendDst("Dst Factor", Float) = 1
        [Enum(UnityEngine.Rendering.BlendMode)] _BlendSrcA("Src Factor (Alpha)", Float) = 1
        [Enum(UnityEngine.Rendering.BlendMode)] _BlendDstA("Dst Factor (Alpha)", Float) = 1
        [Space(5)]
    

        [Header(Stencil)]
        [Enum(None,0,All,15)] _ColorWriteMask("Color Mask", Float) = 15
        [Enum(Equal,3,Always,8)] _StencilFunc("Stencil Func", Float) = 8
        [IntRange] _StencilRef("Stencil Ref", Range(0, 255)) = 0
        [IntRange] _StencilMask("Stencil Mask", Range(0, 255)) = 0
        [Enum(UnityEngine.Rendering.StencilOp)] _StencilOpPass("Stencil Op Pass", Float) = 0
        [Enum(UnityEngine.Rendering.StencilOp)] _StencilOpFail("Stencil Op Fail", Float) = 0
        [Enum(UnityEngine.Rendering.StencilOp)] _StencilOpZFail("Stencil Op Fail (Z)", Float) = 0

    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        LOD 100

        // Turn backface culling off
        Cull Off

        // Turn ZWrite off
        ZWrite Off
        ZTest Off

        Stencil {
            Ref[_StencilRef]
            ReadMask[_StencilMask]
            WriteMask[_StencilMask]
            Comp[_StencilFunc]
            Pass[_StencilOpPass]
            Fail[_StencilOpFail]
            ZFail[_StencilOpZFail]
        }

        ColorMask[_ColorWriteMask]

        BlendOp[_BlendOp]
        Blend[_BlendSrc][_BlendDst],[_BlendSrcA][_BlendDstA]

        Pass
        {

            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            // VERTEX
            float4 _MainTex_ST;

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

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            // FRAGMENT

            
            // Multi-output
            struct fsOutput {
                float4 dest0 : SV_Target0;
                float3 dest1 : SV_Target1;
                float3 dest2 : SV_Target2;
            };

            float _Opacity;
            float3 _MultColor;
            float3 _ScreenColor;
            float _EmissionStrength;

            sampler2D _MainTex;
            sampler2D _Emission;
            sampler2D _BumpMap;

            fsOutput frag(v2f i) : SV_Target
            {
                fsOutput o;
                
                // Albedo Out
                float4 texColor = tex2D(_MainTex, i.uv);
                float3 screenOut = float3(1.0, 1.0, 1.0) - ((float3(1.0, 1.0, 1.0) - (texColor.xyz)) * (float3(1.0, 1.0, 1.0) - (_ScreenColor * texColor.a)));
                o.dest0 = float4(screenOut.xyz, texColor.a) * float4(_MultColor.xyz, 1) * _Opacity;

                // Emission Out
                o.dest1 = float4(tex2D(_Emission, i.uv).xyz * _EmissionStrength, 1) * o.dest0.a;

                // Bumpmap Out
                o.dest2 = float4(tex2D(_BumpMap, i.uv).xyz, 1) * o.dest0.a;
                return o;
            }
            ENDCG
        }
    }
}
