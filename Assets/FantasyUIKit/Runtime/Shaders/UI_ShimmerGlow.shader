Shader "FantasyUI/ShimmerGlow"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _ShimmerColor ("Shimmer Color", Color) = (1, 0.92, 0.55, 0.55)
        _ShimmerSpeed ("Shimmer Speed", Range(0, 5)) = 1.2
        _ShimmerWidth ("Shimmer Width", Range(0.02, 0.6)) = 0.16
        _ShimmerAngle ("Shimmer Angle", Range(-3.14, 3.14)) = 0.55
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" "CanUseSpriteAtlas"="True" }
        Stencil { Ref [_Stencil] Comp [_StencilComp] Pass [_StencilOp] ReadMask [_StencilReadMask] WriteMask [_StencilWriteMask] }
        Cull Off Lighting Off ZWrite Off ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            struct appdata_t {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 texcoord : TEXCOORD0;
            };
            struct v2f {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
            };

            sampler2D _MainTex;
            fixed4 _Color;
            fixed4 _ShimmerColor;
            float _ShimmerSpeed;
            float _ShimmerWidth;
            float _ShimmerAngle;
            float4 _ClipRect;

            v2f vert(appdata_t v) {
                v2f o;
                o.worldPosition = v.vertex;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = v.texcoord;
                o.color = v.color * _Color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target {
                half4 col = tex2D(_MainTex, i.texcoord) * i.color;
                float s = sin(_ShimmerAngle);
                float c = cos(_ShimmerAngle);
                float proj = i.texcoord.x * c + i.texcoord.y * s;
                float cycle = frac(_Time.y * _ShimmerSpeed * 0.35) * 2.6 - 0.8;
                float band = smoothstep(_ShimmerWidth, 0.0, abs(proj - cycle));
                col.rgb += _ShimmerColor.rgb * (band * _ShimmerColor.a);
                col.a *= UnityGet2DClipping(i.worldPosition.xy, _ClipRect);
                return col;
            }
            ENDCG
        }
    }
}
