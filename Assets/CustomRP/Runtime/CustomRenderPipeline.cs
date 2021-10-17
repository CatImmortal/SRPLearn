using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// 自定义渲染管线
/// </summary>
public class CustomRenderPipeline : RenderPipeline
{
    private CameraRenderer renderer = new CameraRenderer();

    /// <summary>
    /// Unity每帧都会调用此方法进行渲染，是SRP的入口
    /// </summary>
    protected override void Render(ScriptableRenderContext context, Camera[] cameras)
    {
        foreach (Camera camera in cameras)
        {
            renderer.Render(context, camera);
        }
    }
}
