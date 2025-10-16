Shader "lit/SimpleOutline"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _OutlineColor ("Outline Color", Color) = (1,1,1,1)
        _OutlineThickness("Outline Thickness", Range(0, 50)) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

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

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            fixed4 _OutlineColor;
            float _OutlineThickness;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float4 original = tex2D(_MainTex, i.uv);

                if (original.a > 0)
                {
                    return original; // 원본 픽셀이 보이면 그대로 출력
                }

                float2 offset = _MainTex_TexelSize.xy * _OutlineThickness;

                float4 top = tex2D(_MainTex, i.uv + float2(0, offset.y));
                float4 bottom = tex2D(_MainTex, i.uv - float2(0, offset.y));
                float4 left = tex2D(_MainTex, i.uv - float2(offset.x, 0));
                float4 right = tex2D(_MainTex, i.uv + float2(offset.x, 0));

                if (top.a > 0 || bottom.a > 0 || left.a > 0 || right.a > 0)
                {
                    return _OutlineColor; // 주변에 원본 픽셀이 있으면 아웃라인 색상 출력
                }

                return fixed4(0,0,0,0); // 투명
            }
            ENDCG
        }
    }
}
