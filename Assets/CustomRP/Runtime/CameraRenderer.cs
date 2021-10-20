using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// 单个相机的渲染封装类
/// </summary>
public partial class CameraRenderer
{
    private const string bufferName = "Render Camera";

    private static ShaderTagId unlitShaderTagId = new ShaderTagId("SRPDefaultUnlit");
    private static ShaderTagId litShaderTagId = new ShaderTagId("CustomLit");

#if UNITY_EDITOR
    private string sampleName;
#else
    private const sampleName = bufferName;
#endif

    private ScriptableRenderContext context;
    private Camera camera;
    private Lighting lighting = new Lighting();

    /// <summary>
    /// 渲染命令缓冲区
    /// </summary>
    private CommandBuffer buffer = new CommandBuffer()
    {
        name = bufferName
    };

    /// <summary>
    /// 存储剔除后的结果数据
    /// </summary>
    private CullingResults cullingResults;

    /// <summary>
    /// 相机渲染
    /// </summary>
    public void Render(ScriptableRenderContext context,Camera camera, bool useDynamicBatching, bool useGPUInstancing, ShadowSettings shadowSetting)
    {
        this.context = context;
        this.camera = camera;

#if UNITY_EDITOR
        
        //在Game视图绘制的几何体也绘制到Scene视图
        PrepareForSceneWindow();

        PrepareBuffer();
#endif

        //剔除 将阴影最大距离传入
        if (!Cull(shadowSetting.MaxDistance))
        {
            return;
        }

        buffer.BeginSample(sampleName);
        ExecuteBuffer();

        //设置光源与阴影数据
        lighting.SetUp(context, cullingResults, shadowSetting);

        buffer.EndSample(sampleName);

        //初始设置
        Setup();


        //绘制几何体
        DrawVisibleGeometry(useDynamicBatching,useGPUInstancing);

#if UNITY_EDITOR
        //绘制SRP不支持的Shader 将其显示为粉色
        DrawUnsupportedShaders();

        //绘制辅助线框
        DrawGizmos();
#endif

        //清理阴影
        lighting.Cleanup();

        //提交渲染命令
        Submit();
    }

    /// <summary>
    /// 剔除
    /// </summary>
    private bool Cull(float maxShadowDistance)
    {
        if (camera.TryGetCullingParameters(out ScriptableCullingParameters p))
        {
            //最大阴影距离和相机远平面比较，取最小者作为阴影距离
            p.shadowDistance = Mathf.Min(maxShadowDistance, camera.farClipPlane);

            cullingResults = context.Cull(ref p);
            return true;
        }

        return false;
    }

    /// <summary>
    /// 初始设置
    /// </summary>
    private void Setup()
    {
        //在清除渲染目标前设置相机属性 实现快速清除
        context.SetupCameraProperties(camera);

        CameraClearFlags flag = camera.clearFlags;

        //清除旧渲染数据
        //CameraClearFlags枚举的清除量是递减的
        buffer.ClearRenderTarget(flag <= CameraClearFlags.Depth, flag == CameraClearFlags.Color,
            flag == CameraClearFlags.Color?camera.backgroundColor.linear:Color.clear);

        buffer.BeginSample(sampleName);
        ExecuteBuffer();

    }

    /// <summary>
    /// 执行缓冲区命令
    /// </summary>
    private void ExecuteBuffer()
    {
        context.ExecuteCommandBuffer(buffer);
        buffer.Clear();
    }

    /// <summary>
    /// 绘制几何体
    /// </summary>
    private void DrawVisibleGeometry(bool useDynamicBatching,bool useGPUInstancing)
    {
        //设置绘制顺序和指定渲染相机
        SortingSettings ss = new SortingSettings(camera);


        //设置渲染的Pass和排序模式
        DrawingSettings ds = new DrawingSettings(unlitShaderTagId,ss);
        ds.enableDynamicBatching = useDynamicBatching;
        ds.enableInstancing = useGPUInstancing;
        ds.SetShaderPassName(1, litShaderTagId);

        //设置要渲染的渲染队列
        FilteringSettings fs = new FilteringSettings(RenderQueueRange.opaque);

        //先绘制不透明物体和天空盒
        ss.criteria = SortingCriteria.CommonOpaque;
        ds.sortingSettings = ss;
        context.DrawRenderers(cullingResults,ref ds,ref fs);

        context.DrawSkybox(camera);

        //最后绘制半透明物体
        ss.criteria = SortingCriteria.CommonTransparent;
        ds.sortingSettings = ss;
        fs.renderQueueRange = RenderQueueRange.transparent;
        context.DrawRenderers(cullingResults, ref ds, ref fs);
    }

  

    /// <summary>
    /// 提交缓冲区渲染命令
    /// </summary>
    private void Submit()
    {
        buffer.EndSample(sampleName);
        ExecuteBuffer();
        context.Submit();
    }
}
