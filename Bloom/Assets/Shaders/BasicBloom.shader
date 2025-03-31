Shader "Unlit/BasicBloom"
{
    Properties
    {
        _MainTex ("Bass(RGB)", 2D) = "white" {}
        _Bloom("Bloom(RGB)",2D) = "black"{}
        _LuminanceThreshold("Luminance Threshold", Float) = 0.5
        _BlurSize("Blur Size", Float) = 1.0
    }
    SubShader
    {
        CGINCLUDE
        
        #include "UnityCG.cginc"

        sampler2D _MainTex;
        half4 _MainTex_TexelSize;
        sampler2D _Bloom;
        float _LuminanceThreshold;
        float _BlurSize;

    // Pass 0
        struct v2f{
            float4 pos : SV_POSITION;
            half2 uv : TEXCOORD0;
        };

        v2f vertExtractBright(appdata_img v){
            v2f o;
            o.pos = UnityObjectToClipPos(v.vertex);
            o.uv = v.texcoord;
            return o;
        }

        // 客观比较亮度
        fixed Luminance(fixed4 color){
            return 0.2125 * color.r + 0.7154 * color.g + 0.0721 * color.b;
        }

        // 提取较亮区域
        fixed4 fragExtractBright(v2f i): SV_Target{
            fixed4 c = tex2D(_MainTex, i.uv);
            fixed val = clamp(Luminance(c) - _LuminanceThreshold, 0.0, 1.0);

            return c * val;
        }

    // Pass1 和 Pass2  高斯模糊
        struct v2fBlur{
            float4 pos : SV_POSITION;
            half2 uv[5] : TEXCOORD0;
        };

        v2fBlur vertBlurVertical(appdata_img v){
            v2fBlur o;
            o.pos = UnityObjectToClipPos(v.vertex);
            half2 uv = v.texcoord;

            // 竖直方向的5个像素
            o.uv[0] = uv;
            o.uv[1] = uv + float2(0.0, _MainTex_TexelSize.y * 1.0) * _BlurSize;
            o.uv[2] = uv - float2(0.0, _MainTex_TexelSize.y * 1.0) * _BlurSize;
            o.uv[3] = uv + float2(0.0, _MainTex_TexelSize.y * 2.0) * _BlurSize;
            o.uv[4] = uv - float2(0.0, _MainTex_TexelSize.y * 2.0) * _BlurSize;

            return o;
        }

        v2fBlur vertBlurHorizontal(appdata_img v){
            v2fBlur o;
            o.pos = UnityObjectToClipPos(v.vertex);
            half2 uv = v.texcoord;

            // 水平方向的5个像素
            o.uv[0] = uv;
            o.uv[1] = uv + float2(_MainTex_TexelSize.x * 1.0, 0.0) * _BlurSize;
            o.uv[2] = uv - float2(_MainTex_TexelSize.x * 1.0, 0.0) * _BlurSize;
            o.uv[3] = uv + float2(_MainTex_TexelSize.x * 2.0, 0.0) * _BlurSize;
            o.uv[4] = uv - float2(_MainTex_TexelSize.x * 2.0, 0.0) * _BlurSize;

            return o;
        }

        fixed4 fragBlur(v2fBlur i) : SV_Target{
            float weight[3] = {0.4026, 0.2442, 0.0545};

            fixed3 sum = tex2D(_MainTex, i.uv[0]).rgb * weight[0];

            // 1和2权重相同，3和4权重相同
            for(int it = 1; it < 3; it++){
                sum += tex2D(_MainTex, i.uv[it*2-1]).rgb * weight[it];
                sum += tex2D(_MainTex, i.uv[it*2]).rgb * weight[it];
            }

            return fixed4(sum, 1.0);
        }

        // Pass3 混合
        struct v2fBloom{
            float4 pos : SV_POSITION;
            half4 uv : TEXCOORD0;
        };

        v2fBloom vertBloom(appdata_img v){
            v2fBloom o;

            o.pos = UnityObjectToClipPos(v.vertex);
            o.uv.xy = v.texcoord;
            o.uv.zw = v.texcoord;

            //平台自适应
            #if UNITY_UV_STARTS_AT_TOP
            if(_MainTex_TexelSize.y < 0.0)
                o.uv.w = 1.0 - o.uv.w;
            #endif
            return o;
        }

        fixed4 fragBloom(v2fBloom i): SV_Target {
            return tex2D(_MainTex, i.uv.xy) + tex2D(_Bloom, i.uv.zw);
        }

        ENDCG


        ZTest Always
        Cull Off
        ZWrite Off

        // Pass0 提取较亮区域
        Pass{
            CGPROGRAM
            #pragma vertex vertExtractBright
            #pragma fragment fragExtractBright
            ENDCG
        }
        // Pass1 竖直方向高斯模糊
        Pass{
            CGPROGRAM
            #pragma vertex vertBlurVertical
            #pragma fragment fragBlur
            ENDCG
        }
        // Pass2 水平方向高斯模糊
        Pass{
            CGPROGRAM
            #pragma vertex vertBlurHorizontal
            #pragma fragment fragBlur
            ENDCG
        }
        // Pass3 混合原图和Bloom效果
        Pass{
            CGPROGRAM
            #pragma vertex vertBloom
            #pragma fragment fragBloom
            ENDCG
        }

    }
}
