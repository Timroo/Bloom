using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent (typeof(Camera))]
public class PostEffectsBase : MonoBehaviour
{
    // protected void Start(){
    //     CheckResources();
    // }

    // protected void CheckResources(){
    //     bool isSupported = CheckSupport();

    //     if(isSupported == false){
    //         NotSupported();
    //     }
    // }


    // // 几乎所有设备都支持 图像特效和渲染纹理，所以这个判断已经没必要了
    // protected bool CheckSupport(){
    //     if(SystemInfo.supportsImageEffects == false || SystemInfo.supportsRenderTextures == false){
    //         Debug.LogWarning("This platform does not support image effects or render textures.");
    //         return false;
    //     }
    //     return true;
    // }

    protected void NotSupported(){
        // 内置变量，用于开启or关闭脚本
        enabled = false;
    }
    
    // 检查给定的着色器（Shader）是否可用 : 根据需要创建或返回一个与之关联的材质
        // 只要shader可用，就一定会返回一个合适的材质
    protected Material CheckShaderAndCreateMaterial(Shader shader, Material material){

        if(shader == null){
            return null;
        }

        if(shader.isSupported && material && material.shader == shader)
            return material;
        
        if(!shader.isSupported){
            return null;
        }
        else{
            material = new Material(shader);
            material.hideFlags = HideFlags.DontSave;      //临时创建，不保存
            if(material)
                return material;
            else
                return null;
        }
    }

    
}
