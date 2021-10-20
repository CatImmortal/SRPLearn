using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// 可投射阴影的定向光
/// </summary>
public struct shadowedDirectionalLight
{
    /// <summary>
    /// 可见光的索引
    /// </summary>
    public int VisibleLightIndex;
}

/// <summary>
/// 阴影类
/// </summary>
public class Shadows
{
    /// <summary>
    /// 最大可投射阴影的定向光数量
    /// </summary>
    private const int maxShadowedDirectionalLightCount = 4;

    /// <summary>
    /// 阴影图集的属性id
    /// </summary>
    private static int dirShadowAtlasId = Shader.PropertyToID("_DirectionalShadowAtlas");

    private const string bufferName = "Shadow";

    private CommandBuffer buffer = new CommandBuffer()
    {
        name = bufferName
    };

    private ScriptableRenderContext context;
    private CullingResults cullingResults;
    private ShadowSettings settings;

    /// <summary>
    /// 可投射阴影的定向光数组
    /// </summary>
    private shadowedDirectionalLight[] shadowedDirectionalLights = new shadowedDirectionalLight[maxShadowedDirectionalLightCount];

    /// <summary>
    /// 已存储的可投射阴影的定向光数量
    /// </summary>
    private int shadowedDirectionalLightCount;

    public void Setup(ScriptableRenderContext context, CullingResults cullingResults, ShadowSettings settings)
    {
        this.context = context;
        this.cullingResults = cullingResults;
        this.settings = settings;

        shadowedDirectionalLightCount = 0;
    }

    /// <summary>
    /// 阴影渲染
    /// </summary>
    public void Render()
    {
        if (shadowedDirectionalLightCount > 0)
        {
            RenderDirectionalShadows();
        }
    }

    public void Cleanup()
    {
        buffer.ReleaseTemporaryRT(dirShadowAtlasId);
    }



    /// <summary>
    /// 存储可见光的阴影数据
    /// </summary>
    public void ReserveDirectionalShadows(Light light,int visibleLightIndex)
    {
        if (shadowedDirectionalLightCount < maxShadowedDirectionalLightCount
            &&light.shadows != LightShadows.None
            && light.shadowStrength > 0f
            && cullingResults.GetShadowCasterBounds(visibleLightIndex,out Bounds b)
            )
        {
            //光源开启了阴影投射 并且 强度>0
            //并且 在阴影最大投射距离内有被该光源影响且需要投影的物体

            shadowedDirectionalLights[shadowedDirectionalLightCount] = new shadowedDirectionalLight() { VisibleLightIndex = visibleLightIndex };
            shadowedDirectionalLightCount++;
        }
    }


    /// <summary>
    /// 执行缓冲区命令并清除缓冲区
    /// </summary>
    private void ExecuteBuffer()
    {
        context.ExecuteCommandBuffer(buffer);
        buffer.Clear();
    }

    /// <summary>
    /// 渲染定向光阴影
    /// </summary>
    private void RenderDirectionalShadows()
    {
        //创建一张RenderTexture
        int atlasSize = (int)settings.Directional.atlasSize;
        buffer.GetTemporaryRT(dirShadowAtlasId, atlasSize, atlasSize, 32, FilterMode.Bilinear, RenderTextureFormat.Shadowmap);

        //创建一张阴影图集 指定其为阴影渲染的纹理
        buffer.SetRenderTarget(dirShadowAtlasId, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);

        //清除深度缓冲区
        buffer.ClearRenderTarget(true, false, Color.clear);

        buffer.BeginSample(bufferName);
        ExecuteBuffer();

        //要分隔的图块大小和数量
        int split = shadowedDirectionalLightCount <= 1 ? 1 : 2;
        int tileSize = atlasSize / split;

        //遍历所有方向光进行逐光源的阴影渲染
        for (int i = 0; i < shadowedDirectionalLightCount; i++)
        {
            RenderDirectionalShadows(i, split,tileSize);
        }

        buffer.EndSample(bufferName);
        ExecuteBuffer();
    }

    private void RenderDirectionalShadows(int index,int split,int tileSize)
    {
        //取出对应索引的数据
        shadowedDirectionalLight light = shadowedDirectionalLights[index];

        //创建 阴影绘制设置 对象
        ShadowDrawingSettings shadowDrawingSettings = new ShadowDrawingSettings(cullingResults, light.VisibleLightIndex);

        //获取视图 投影矩阵和裁剪投影对象信息
        cullingResults.ComputeDirectionalShadowMatricesAndCullingPrimitives(
            light.VisibleLightIndex,
            0,
            1,
            Vector3.zero,
            tileSize,
            0f,
            out Matrix4x4 viewMatrix,
            out Matrix4x4 projectionMatrix,
            out ShadowSplitData splitData
            );

        shadowDrawingSettings.splitData = splitData;

        SetTileViewport(index, split, tileSize);

        //设置视图 投影矩阵
        buffer.SetViewProjectionMatrices(viewMatrix, projectionMatrix);

        ExecuteBuffer();

        //渲染阴影到阴影图集中
        context.DrawShadows(ref shadowDrawingSettings);
    }

    /// <summary>
    /// 调整渲染视口来渲染单个图块
    /// </summary>
    private void SetTileViewport(int index, int split, int tileSize)
    {
        //计算图块偏移
        Vector2 offset = new Vector2(index % split, index / split);

        //设置渲染视口 拆分为多个图块
        buffer.SetViewport(new Rect(offset.x * tileSize, offset.y * tileSize, tileSize, tileSize));
    }
}
