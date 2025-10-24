// UI/SynergySplitter.shader
Shader "UI/SynergySplitter"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        
        // C#에서 제어할 변수들
        _Count ("Image Count", Range(1, 3)) = 1
        _RectAspect ("Rect Aspect", Float) = 1.0 // RawImage의 가로/세로 비율

        // 1번 슬롯
        _Tex1 ("Texture 1", 2D) = "white" {}
        _Aspect1 ("Aspect 1", Float) = 1.0
        [NoScaleOffset] _Tex1_UVRect ("UV Rect 1", Vector) = (0,0,1,1)

        // 2번 슬롯
        _Tex2 ("Texture 2", 2D) = "white" {}
        _Aspect2 ("Aspect 2", Float) = 1.0
        [NoScaleOffset] _Tex2_UVRect ("UV Rect 2", Vector) = (0,0,1,1)

        // 3번 슬롯
        _Tex3 ("Texture 3", 2D) = "white" {}
        _Aspect3 ("Aspect 3", Float) = 1.0
        [NoScaleOffset] _Tex3_UVRect ("UV Rect 3", Vector) = (0,0,1,1)
        
        // UI 마스킹 및 스텐실을 위한 기본 프로퍼티
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use UI Alpha Clip", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord : TEXCOORD0; // RawImage의 기본 UV (0~1)
                float4 worldPosition : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };
            
            // Properties에서 선언한 변수들
            fixed4 _Color;
            float _Count;
            float _RectAspect; 
            float4 _ClipRect; // RectMask2D용

            sampler2D _Tex1;
            sampler2D _Tex2;
            sampler2D _Tex3;
            
            float _Aspect1;
            float _Aspect2;
            float _Aspect3;

            float4 _Tex1_UVRect; // (x, y, width, height)
            float4 _Tex2_UVRect;
            float4 _Tex3_UVRect;


            // UV를 1/n 영역에 맞게 비율 유지 크롭(Center Crop)하는 함수
            float2 CropUV(float2 uv, float sliverAspect, float texAspect)
            {
                float2 resultUV = uv;
                
                if (texAspect > sliverAspect) // 텍스처가 슬라이스보다 '넓음' -> 좌우를 잘라냄
                {
                    float scale = sliverAspect / texAspect;
                    resultUV.x = (uv.x - 0.5) * scale + 0.5;
                }
                else // 텍스처가 슬라이스보다 '김' -> 위아래를 잘라냄
                {
                    float scale = texAspect / sliverAspect;
                    resultUV.y = (uv.y - 0.5) * scale + 0.5;
                }
                return resultUV;
            }

            // 크롭된 UV(0~1)를 아틀라스 UV(0~1)로 변환하는 함수
            float2 RemapToAtlasUV(float2 croppedUV, float4 uvRect)
            {
                // croppedUV (0~1) -> (uvRect.x) ~ (uvRect.x + uvRect.z)
                croppedUV.x = uvRect.x + croppedUV.x * uvRect.z;
                croppedUV.y = uvRect.y + croppedUV.y * uvRect.w;
                return croppedUV;
            }

            v2f vert(appdata_t v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.worldPosition = v.vertex;
                o.vertex = UnityObjectToClipPos(o.worldPosition);
                o.texcoord = v.texcoord;
                o.color = v.color * _Color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // [수정된 부분]
                // _Count가 0 (0.5 미만)이면 즉시 투명(0,0,0,0)을 반환합니다.
                if (_Count < 0.5)
                {
                    return fixed4(0,0,0,0);
                }
                // --- 수정 끝 ---

                float rectAspect = _RectAspect; 
                fixed4 col = fixed4(0,0,0,0);
                float2 uv = i.texcoord;
                
                // 이제 이 if문은 _Count가 1일 때만 실행됩니다.
                if (_Count <= 1.0)
                {
                    float2 croppedUV = CropUV(uv, rectAspect, _Aspect1);
                    float2 finalUV = RemapToAtlasUV(croppedUV, _Tex1_UVRect);
                    col = tex2D(_Tex1, finalUV);
                }
                else if (_Count <= 2.0)
                {
                    float sliverAspect = rectAspect * 0.5; // 1/2 영역의 비율
                    if (uv.x < 0.5)
                    {
                        float2 uv1 = float2(uv.x * 2.0, uv.y); // 0~0.5 -> 0~1
                        float2 croppedUV = CropUV(uv1, sliverAspect, _Aspect1);
                        float2 finalUV = RemapToAtlasUV(croppedUV, _Tex1_UVRect);
                        col = tex2D(_Tex1, finalUV);
                    }
                    else
                    {
                        float2 uv2 = float2((uv.x - 0.5) * 2.0, uv.y); // 0.5~1 -> 0~1
                        float2 croppedUV = CropUV(uv2, sliverAspect, _Aspect2);
                        float2 finalUV = RemapToAtlasUV(croppedUV, _Tex2_UVRect);
                        col = tex2D(_Tex2, finalUV);
                    }
                }
                else // Count 3
                {
                    float sliverAspect = rectAspect / 3.0; // 1/3 영역의 비율
                    if (uv.x < 0.333)
                    {
                        float2 uv1 = float2(uv.x * 3.0, uv.y);
                        float2 croppedUV = CropUV(uv1, sliverAspect, _Aspect1);
                        float2 finalUV = RemapToAtlasUV(croppedUV, _Tex1_UVRect);
                        col = tex2D(_Tex1, finalUV);
                    }
                    else if (uv.x < 0.666)
                    {
                        float2 uv2 = float2((uv.x - 0.333) * 3.0, uv.y);
                        float2 croppedUV = CropUV(uv2, sliverAspect, _Aspect2);
                        float2 finalUV = RemapToAtlasUV(croppedUV, _Tex2_UVRect);
                        col = tex2D(_Tex2, finalUV);
                    }
                    else
                    {
                        float2 uv3 = float2((uv.x - 0.666) * 3.0, uv.y);
                        float2 croppedUV = CropUV(uv3, sliverAspect, _Aspect3);
                        float2 finalUV = RemapToAtlasUV(croppedUV, _Tex3_UVRect);
                        col = tex2D(_Tex3, finalUV);
                    }
                }

                // UI 마스킹 적용
                col.a *= UnityGet2DClipping(i.worldPosition.xy, _ClipRect);
                
                #ifdef UNITY_UI_ALPHACLIP
                clip (col.a - 0.001);
                #endif

                return col;
            }
            ENDCG
        }
    }
}