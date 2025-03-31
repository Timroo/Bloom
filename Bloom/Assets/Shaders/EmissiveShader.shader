Shader "Unlit/EmissiveShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _EmissiveColor("Emissive Color", Color) = (1,1,1,1)
        _EmissiveIntensity("Emissive Intensity", Range(0, 20)) = 1.0
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
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _EmissiveColor;
            float _EmissiveIntensity;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                return fixed4(1,1,1,1) * _EmissiveColor * _EmissiveIntensity;
            }
            ENDCG
        }
    }
}
