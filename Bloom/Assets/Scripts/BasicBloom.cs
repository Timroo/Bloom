using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.Rendering;

public class BasicBloom : PostEffectsBase
{
    public Shader bloomShader;
    private Material bloomMaterial = null;

    public Material material{
        get{
            bloomMaterial = CheckShaderAndCreateMaterial(bloomShader, bloomMaterial);
            return bloomMaterial;
        }
    }

    // - 模糊迭代次数
    // - 模糊范围
    // - 下采样
    // - 较亮区域的阈值大小
    [Range(0,4)]
    public int iterations = 3;
    [Range(0.2f, 3.0f)]
    public float blurSpread = 0.6f;
    [Range(1,8)]
    public int downSample = 2;
    [Range(0.0f, 4.0f)]
    public float luminanceThreshold = 0.6f;

    // 获取当前屏幕图像的函数（得到渲染纹理）
    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        // 检查当前材质是否可用
        if(material != null){
            material.SetFloat("_LuminanceThreshold", luminanceThreshold);
            int rtW = src.width/downSample;
            int rtH = src.height/downSample;

            // 创建缓冲区
            RenderTexture buffer0 = RenderTexture.GetTemporary(rtW, rtH, 0);
            // 双线性滤波
            buffer0.filterMode = FilterMode.Bilinear;

            // Pass0 提取图像中较亮区域
            Graphics.Blit(src, buffer0, material, 0);

            for(int i = 0; i < iterations; i++){
                // 高斯模糊的范围
                material.SetFloat("_BlurSize", 1.0f + i * blurSpread);
                RenderTexture buffer1 = RenderTexture.GetTemporary(rtW, rtH, 0);
                
                //Pass1 竖直模糊
                Graphics.Blit(buffer0, buffer1, material, 1);

                RenderTexture.ReleaseTemporary(buffer0);
                buffer0 = buffer1;
                buffer1 = RenderTexture.GetTemporary(rtW, rtH, 0);

                //Pass2 水平模糊
                Graphics.Blit(buffer0, buffer1, material, 2);

                RenderTexture.ReleaseTemporary(buffer0);
                buffer0 = buffer1;
            }

            material.SetTexture("_Bloom", buffer0);
            // Pass3 用于混合
            Graphics.Blit(src, dest, material, 3);

            RenderTexture.ReleaseTemporary(buffer0);
        }else{
            // 没有材质就直接绘制图像
            Graphics.Blit(src, dest);
        }
    }
}
