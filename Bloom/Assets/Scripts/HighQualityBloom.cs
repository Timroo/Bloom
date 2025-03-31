using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

// 编辑器下执行脚本+Scene视图下看到结果
[ExecuteAlways, ImageEffectAllowedInSceneView]
public class HighQualityBloom : MonoBehaviour
{
#region 参数定义

    // Debug：控制Bloom各阶段输出
    public enum BloomDebugFlag{
        None = 0,
        DownSample = 1,
        UpSample = 2
    }
    [Space(20)]

    // 采样参数： 层数；模糊核大小；模糊强度
    public int downSampleStep = 7;
    [Range(3, 15)] public int downSampleBlurSize = 5;
    [Range(0.01f, 10.0f)]public float downSampleBlurSigma = 1.0f;
    [Range(3, 15)] public int upSampleBlurSize = 5;
    [Range(0.01f, 10.0f)]public float upSampleBlurSigma = 1.0f;
    [Space(20)]

    // 亮度筛选和强度调节
    public bool useKarisAverage = false;
    [Range(0.001f, 10.0f)] public float luminanceThreshold = 1.0f;
    [Range(0.001f, 10.0f)] public float bloomIntensity = 1.0f;
    [Space(20)]

    // 调试参数
    // - 调试输出；查看第几层Mipmap
    public BloomDebugFlag debugFlag;
    [Range(0,6)] public int mipDebugIndex = 0;

#endregion

#region 生命周期函数
    void Start()
    {
        Camera.main.allowHDR = true;
    }

    #endregion


    #region Bloom执行主流程

    // source是当前屏幕
    // destination 是最终渲染到屏幕的内容 
    // - 函数中一定要有一句是针对destination做Graphics.Blit
    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        // 设置shader参数
        Shader.SetGlobalInt("_downSampleBlurSize", downSampleBlurSize);
        Shader.SetGlobalFloat("_downSampleBlurSigma", downSampleBlurSigma);
        Shader.SetGlobalInt("_upSampleBlurSize", upSampleBlurSize);
        Shader.SetGlobalFloat("_upSampleBlurSigma", upSampleBlurSigma);

        Shader.SetGlobalFloat("_luminanceThreshold", luminanceThreshold);
        Shader.SetGlobalFloat("_bloomIntensity", bloomIntensity);

        // 【高亮区域提取】
        RenderTexture RT_threshold = RenderTexture.GetTemporary(Screen.width, Screen.height, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
        RT_threshold.filterMode = FilterMode.Bilinear;
        Graphics.Blit(source, RT_threshold, new Material(Shader.Find("HighQualityBloom/threshold")));

        // 【Debug】渲染到destination
        // Graphics.Blit(RT_threshold, destination, new Material(Shader.Find("HighQualityBloom/postDebug")));
        

        // 【降采样】
        int N = downSampleStep;
        int downSize = 2;
        RenderTexture[] RT_BloomDown = new RenderTexture[N];
        for(int i=0; i<N; i++){
            int w = Screen.width / downSize;
            int h = Screen.height / downSize;
            // 创建一个临时的 RenderTexture 对象
            RT_BloomDown[i] = RenderTexture.GetTemporary(w, h, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
            RT_BloomDown[i].filterMode = FilterMode.Bilinear;
            downSize *= 2;
        }
        
        // - 解决闪烁问题
        Graphics.Blit(RT_threshold, RT_BloomDown[0], new Material(Shader.Find(useKarisAverage ? "HighQualityBloom/firstDownSample" :  "HighQualityBloom/downSample")));
    //    Graphics.Blit(RT_threshold, RT_BloomDown[0], new Material(Shader.Find("HighQualityBloom/downSample")));
        for(int i=1; i<N; i++){
            Graphics.Blit(RT_BloomDown[i-1], RT_BloomDown[i], new Material(Shader.Find("HighQualityBloom/downSample")));
        }

        // 【上采样】
        RenderTexture[] RT_BloomUp = new RenderTexture[N];
        for(int i=0; i<N-1; i++){
            int w = RT_BloomDown[N-2-i].width;
            int h = RT_BloomDown[N-2-i].height;
            RT_BloomUp[i] = RenderTexture.GetTemporary(w, h, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
            RT_BloomUp[i].filterMode = FilterMode.Bilinear;
        }

        Shader.SetGlobalTexture("_PrevMip", RT_BloomDown[N-1]);
        Graphics.Blit(RT_BloomDown[N-2], RT_BloomUp[0], new Material(Shader.Find("HighQualityBloom/upSample")));
        for(int i=1; i<N-1; i++){
            RenderTexture prev_mip = RT_BloomUp[i-1];
            RenderTexture curr_mip = RT_BloomDown[N-2-i];
            Shader.SetGlobalTexture("_PrevMip", prev_mip);
            Graphics.Blit(curr_mip, RT_BloomUp[i], new Material(Shader.Find("HighQualityBloom/upSample")));
        }

        // Bloom的最终结果
        Shader.SetGlobalTexture("_BloomTex", RT_BloomUp[N-2]);


        // 【合成原图与Debug】

        if(debugFlag == BloomDebugFlag.None){
            // 【Debug】渲染到destination
            // Graphics.Blit(RT_threshold, destination, new Material(Shader.Find("HighQualityBloom/postDebug")));
            Graphics.Blit(source, destination, new Material(Shader.Find("HighQualityBloom/post")));
        }
        else if(debugFlag == BloomDebugFlag.DownSample){
            Graphics.Blit(RT_BloomDown[mipDebugIndex], destination, new Material(Shader.Find("HighQualityBloom/postDebug")));
        }
        else if(debugFlag == BloomDebugFlag.UpSample){
            Graphics.Blit(RT_BloomUp[mipDebugIndex], destination, new Material(Shader.Find("HighQualityBloom/postDebug")));
        }

        // 清理临时纹理（释放内存
        for(int i=0; i<N; i++){
            RenderTexture.ReleaseTemporary(RT_BloomDown[i]);
            RenderTexture.ReleaseTemporary(RT_BloomUp[i]);
        }
        RenderTexture.ReleaseTemporary(RT_threshold);
 
    }

    #endregion
}