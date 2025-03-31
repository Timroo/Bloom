// shader的名字不重要，路径也不重要，具体调用得看里面写的这个
// - 因为shader会预编译
Shader "HighQualityBloom/post"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Cull Off ZWrite Off ZTest Always

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

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }
            

            float3 ACESToneMapping(float3 color, float adapted_lum){
                const float A = 2.51f;
                const float B = 0.03f;
                const float C = 2.43f;
                const float D = 0.59f;
                const float E = 0.14f;

                color *= adapted_lum;
                return (color * (A * color + B)) / (color * (C * color + D) + E);
            }

            sampler2D _MainTex;
            sampler2D _BloomTex;
            float4 _MainTex_TexelSize;
            float _bloomIntensity;

            fixed4 frag (v2f i) : SV_Target
            {
                float4 col = tex2D(_MainTex, i.uv);
                
                // bloom
                float3 bloom = tex2D(_BloomTex, i.uv) * _bloomIntensity;
                bloom = ACESToneMapping(bloom, 1.0);

                float g = 1.0/2.2;
                bloom = saturate(pow(bloom, float3(g,g,g)));

                col.rgb += bloom;
                return col;
            }
            ENDCG
        }
    }
}
