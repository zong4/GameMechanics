Shader "Mirror/ScreenSpaceUV"
{
    Properties
    {
        _MainTex ("Mirror Texture", 2D) = "black" {}
        _Color ("Color", Color) = (1, 1, 1, 1)
        _FlipX ("Flip X", Float) = 1
        _FlipY ("Flip Y", Float) = 0
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 screenPos : TEXCOORD0;
            };

            sampler2D _MainTex;
            fixed4 _Color;
            float _FlipX;
            float _FlipY;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.screenPos = ComputeScreenPos(o.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = i.screenPos.xy / i.screenPos.w;
                uv.x = lerp(uv.x, 1.0 - uv.x, saturate(_FlipX));
                uv.y = lerp(uv.y, 1.0 - uv.y, saturate(_FlipY));
                return tex2D(_MainTex, uv) * _Color;
            }
            ENDCG
        }
    }
}
