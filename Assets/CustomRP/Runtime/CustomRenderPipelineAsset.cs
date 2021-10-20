using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// 自定义渲染管线的Asset
/// </summary>
[CreateAssetMenu(menuName = "Rendering/CreateCustomRenderPipeline")]
public class CustomRenderPipelineAsset : RenderPipelineAsset
{
    /// <summary>
    /// 是否使用动态合批
    /// </summary>
    [SerializeField]
    private bool useDynamicBatching = true;

    /// <summary>
    /// 是否使用GPU实例化
    /// </summary>
    [SerializeField]
    private bool useGPUInstancing = true;

    /// <summary>
    /// 是否使用SRPBatcher
    /// </summary>
    [SerializeField]
    private bool useSRPBatcher = true;

    /// <summary>
    /// 阴影设置
    /// </summary>
    [SerializeField]
    private ShadowSettings shadows = default;

    protected override RenderPipeline CreatePipeline()
    {
        return new CustomRenderPipeline(useDynamicBatching,useGPUInstancing,useSRPBatcher,shadows);
    }
}
