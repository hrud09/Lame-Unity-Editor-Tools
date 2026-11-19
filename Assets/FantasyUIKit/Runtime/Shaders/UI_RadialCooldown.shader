Shader "FantasyUI/RadialCooldown"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (0.08, 0.05, 0.10, 0.72)
        _Progress ("Cooldown Remaining (0-1)", Range(0, 1)) = 0.0
        _EdgeGlow ("Sweep Edge Glow", Color) = (1, 0.84, 0.28, 0.85)
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

            struct appdata_t { float4 vertex : POSITION; float4 color : COLOR; float2 texcoord : TEXCOORD0; };
            struct v2f { float4 vertex : SV_POSITION; fixed4 color : COLOR; float2 texcoord : TEXCOORD0; float4 worldPosition : TEXCOORD1; };

            sampler2D _MainTex;
            fixed4 _Color;
            float _Progress;
            fixed4 _EdgeGlow;
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
                float2 d = i.texcoord - float2(0.5, 0.5);
                // Clockwise angle starting from 12 o'clock [0..1]
                float angle = atan2(d.x, d.y);
                float norm = frac(angle / 6.2831853);
                float mask = step(norm, _Progress);
                float edge = smoothstep(0.04, 0.0, abs(norm - _Progress)) * step(0.005, _Progress) * step(_Progress, 0.995);
                col.rgb = lerp(col.rgb, _EdgeGlow.rgb, edge * _EdgeGlow.a);
                col.a *= max(mask, edge) * UnityGet2DClipping(i.worldPosition.xy, _ClipRect);
                return col;
            }
            ENDCG
        }
    }
}
