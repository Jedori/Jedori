Shader "Custom/MoonShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _EmissionMap ("Emission Map", 2D) = "black" {}
        _EmissionIntensity ("Emission Intensity", Range(0, 2)) = 1.0
        _EmissionColor ("Emission Color", Color) = (1,1,1,1)
        _Thickness ("Thickness", Range(0.1, 1.0)) = 0.1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
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
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 worldNormal : TEXCOORD1;
                float3 viewDir : TEXCOORD2;
            };

            sampler2D _MainTex;
            sampler2D _EmissionMap;
            float4 _MainTex_ST;
            float _EmissionIntensity;
            float4 _EmissionColor;
            float _Thickness;

            v2f vert (appdata v)
            {
                v2f o;
                // 얇은 구체를 위한 버텍스 위치 조정
                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                float3 worldNormal = UnityObjectToWorldNormal(v.normal);
                
                // 얇은 구체의 두께를 고려한 UV 매핑
                float2 uv = v.uv;
                uv.y = uv.y * _Thickness;  // Y축 UV를 얇게 조정
                
                o.vertex = UnityWorldToClipPos(worldPos);
                o.uv = TRANSFORM_TEX(uv, _MainTex);
                o.worldNormal = worldNormal;
                o.viewDir = normalize(UnityWorldSpaceViewDir(worldPos));
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // 기본 텍스처 색상
                fixed4 col = tex2D(_MainTex, i.uv);
                
                // 발광 맵 색상
                fixed4 emission = tex2D(_EmissionMap, i.uv);
                
                // 발광 강도 적용
                emission *= _EmissionIntensity * _EmissionColor;
                
                // 얇은 구체의 가장자리에서 발광 강도 감소
                float rim = 1.0 - saturate(dot(i.viewDir, i.worldNormal));
                emission *= 1.0 - rim * 0.5;  // 가장자리에서 발광이 약간 감소
                
                // 최종 색상 계산 (기본 색상 + 발광)
                return col + emission;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
} 