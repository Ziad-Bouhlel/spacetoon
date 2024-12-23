Shader "Custom/SpriteContour"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _OutlineColor ("Outline Color", Color) = (0,0,0,1)
        _OutlineThickness ("Outline Thickness", Range(0,0.1)) = 0.05
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
        LOD 200

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off
            Lighting Off
            ZWrite Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _OutlineColor;
            float _OutlineThickness;

            v2f vert (appdata_t v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                float2 uv = i.texcoord;
                float2 offset = float2(_OutlineThickness, _OutlineThickness);

                // Sample texture in 9 surrounding pixels to detect edges
                fixed4 center = tex2D(_MainTex, uv);
                fixed4 edgeSample = 
                    tex2D(_MainTex, uv + offset * float2(-1, -1)) +
                    tex2D(_MainTex, uv + offset * float2(0, -1)) +
                    tex2D(_MainTex, uv + offset * float2(1, -1)) +
                    tex2D(_MainTex, uv + offset * float2(-1, 0)) +
                    tex2D(_MainTex, uv + offset * float2(1, 0)) +
                    tex2D(_MainTex, uv + offset * float2(-1, 1)) +
                    tex2D(_MainTex, uv + offset * float2(0, 1)) +
                    tex2D(_MainTex, uv + offset * float2(1, 1));

                // If surrounding pixels are transparent, show the outline
                if (edgeSample.a > 0 && center.a == 0) {
                    return _OutlineColor;
                }
                return center;
            }
            ENDCG
        }
    }
}
