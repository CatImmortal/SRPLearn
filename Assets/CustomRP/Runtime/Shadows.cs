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
    /// 最大级联数量
    /// </summary>
    private const int maxCascades = 4;

    /// <summary>
    /// 阴影图集的属性id
    /// </summary>
    private static int dirShadowAtlasId = Shader.PropertyToID("_DirectionalShadowAtlas");

    /// <summary>
    /// 阴影转换矩阵的属性id
    /// </summary>
    private static int dirShadowMatricesId = Shader.PropertyToID("_DirShadowMatricesId");

    /// <summary>
    /// 阴影转换矩阵数组
    /// </summary>
    private static Matrix4x4[] dirShadowMatrices = new Matrix4x4[maxShadowedDirectionalLightCount * maxCascades];

    /// <summary>
    /// 级联数量的属性id
    /// </summary>
    private static int cascadeCountId = Shader.PropertyToID("_CascadeCount");

    /// <summary>
    /// 级联保卫球的属性id
    /// </summary>
    private static int cascadeCullingSpheresId = Shader.PropertyToID("_CascadeCullingSpheresId");

    /// <summary>
    /// 级联包围球数据
    /// </summary>
    private static Vector4[] cascadeCullingSpheres = new Vector4[maxCascades];

    /// <summary>
    /// 阴影距离的属性Id
    /// </summary>
    private static int shadowDistanceId = Shader.PropertyToID("_ShadowDistance");


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
    /// 渲染阴影到阴影图集中
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
    public Vector2 ReserveDirectionalShadows(Light light,int visibleLightIndex)
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
            
            //用一个vector2 存储阴影强度和阴影图块的偏移
            Vector2 result = new Vector2(light.shadowStrength,settings.Directional.CascadeCount * shadowedDirectionalLightCount);
           
            shadowedDirectionalLightCount++;

            return result;
        }

        return Vector2.zero;
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
    /// 渲染所有方向光阴影
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
        int tiles = shadowedDirectionalLightCount * settings.Directional.CascadeCount;

        int split;
        if (tiles <= 1)
        {
            split = 1;
        }
        else
        {
            if (tiles <= 4)
            {
                split = 2;
            }
            else
            {
                split = 4;
            }
        }

        int tileSize = atlasSize / split;

        //遍历所有方向光进行逐光源的阴影渲染
        for (int i = 0; i < shadowedDirectionalLightCount; i++)
        {
            RenderDirectionalShadows(i, split,tileSize);
        }

        //将级联数量和包围球数据发给shader
        buffer.SetGlobalInt(cascadeCountId, settings.Directional.CascadeCount);
        buffer.SetGlobalVectorArray(cascadeCullingSpheresId, cascadeCullingSpheres);

        //将阴影转换矩阵发给Shader
        buffer.SetGlobalMatrixArray(dirShadowMatricesId, dirShadowMatrices);

        //将最大阴影距离发给shader
        buffer.SetGlobalFloat(shadowDistanceId, settings.MaxDistance);

        buffer.EndSample(bufferName);
        ExecuteBuffer();
    }

    /// <summary>
    /// 渲染单个方向光阴影
    /// </summary>
    private void RenderDirectionalShadows(int index,int split,int tileSize)
    {
        //取出对应索引的数据
        shadowedDirectionalLight light = shadowedDirectionalLights[index];

        //创建 阴影绘制设置 对象
        ShadowDrawingSettings shadowDrawingSettings = new ShadowDrawingSettings(cullingResults, light.VisibleLightIndex);

       

        //得到级联阴影贴图需要的参数
        int cascadeCount = settings.Directional.CascadeCount;
        int tileOffset = index * cascadeCount;
        Vector3 ratios = settings.Directional.CascadeRatios;

        for (int i = 0; i < cascadeCount; i++)
        {
            //获取视图 投影矩阵和裁剪投影对象信息
            cullingResults.ComputeDirectionalShadowMatricesAndCullingPrimitives(
                light.VisibleLightIndex,
                i,
                cascadeCount,
                ratios,
                tileSize,
                0f,
                out Matrix4x4 viewMatrix,
                out Matrix4x4 projectionMatrix,
                out ShadowSplitData splitData
                );

            //得到第一个光源的包围球数据
            if (index == 0)
            {
                Vector4 cullingSphere = splitData.cullingSphere;
                cullingSphere.w *= cullingSphere.w;

                cascadeCullingSpheres[i] = cullingSphere;
            }

            shadowDrawingSettings.splitData = splitData;

            //调整图块索引，它等于光源的图块偏移加上级联的索引
            int tileIndex = tileOffset + i;

            //设置图集分块
            Vector2 offset = SetTileViewport(tileIndex, split, tileSize);

            //获取从世界空间到阴影图块空间的转换矩阵
            dirShadowMatrices[tileIndex] = ConvertToAtlasMatrix(projectionMatrix * viewMatrix, offset, split);

            //设置视图 投影矩阵
            buffer.SetViewProjectionMatrices(viewMatrix, projectionMatrix);
            ExecuteBuffer();

            //渲染阴影到阴影图集中
            context.DrawShadows(ref shadowDrawingSettings);
        }



       
    }

    /// <summary>
    /// 调整渲染视口来渲染单个图块
    /// </summary>
    private Vector2 SetTileViewport(int index, int split, int tileSize)
    {
        //计算图块偏移
        Vector2 offset = new Vector2(index % split, index / split);

        //设置渲染视口 拆分为多个图块
        buffer.SetViewport(new Rect(offset.x * tileSize, offset.y * tileSize, tileSize, tileSize));

        return offset;
    }

    /// <summary>
    /// 获取从世界空间到阴影图块空间的转换矩阵
    /// </summary>
    private Matrix4x4 ConvertToAtlasMatrix(Matrix4x4 m,Vector2 offset,int split)
    {
        //如果使用了反向Zbuffer
        if (SystemInfo.usesReversedZBuffer)
        {
            m.m20 = -m.m20;
            m.m21 = -m.m21;
            m.m22 = -m.m22;
            m.m23 = -m.m23;
        }
        //设置矩阵坐标
        float scale = 1f / split;
        m.m00 = (0.5f * (m.m00 + m.m30) + offset.x * m.m30) * scale;
        m.m01 = (0.5f * (m.m01 + m.m31) + offset.x * m.m31) * scale;
        m.m02 = (0.5f * (m.m02 + m.m32) + offset.x * m.m32) * scale;
        m.m03 = (0.5f * (m.m03 + m.m33) + offset.x * m.m33) * scale;
        m.m10 = (0.5f * (m.m10 + m.m30) + offset.y * m.m30) * scale;
        m.m11 = (0.5f * (m.m11 + m.m31) + offset.y * m.m31) * scale;
        m.m12 = (0.5f * (m.m12 + m.m32) + offset.y * m.m32) * scale;
        m.m13 = (0.5f * (m.m13 + m.m33) + offset.y * m.m33) * scale;
        m.m20 = 0.5f * (m.m20 + m.m30);
        m.m21 = 0.5f * (m.m21 + m.m31);
        m.m22 = 0.5f * (m.m22 + m.m32);
        m.m23 = 0.5f * (m.m23 + m.m33);
        return m;
    }
}
