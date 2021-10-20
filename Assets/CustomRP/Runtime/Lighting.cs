using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class Lighting
{
    //private static int dirLightColorId = Shader.PropertyToID("_DirectionalLightColor");
    //private static int dirLightDirectionId = Shader.PropertyToID("_DirectionalLightDirection");

    private static int dirLightCountId = Shader.PropertyToID("_DirectionalLightCount");
    private static int dirLightColorsId = Shader.PropertyToID("_DirectionalLightColors");
    private static int dirLightDirectionsId = Shader.PropertyToID("_DirectionalLightDirections");

    /// <summary>
    /// 最大可见方向光数量
    /// </summary>
    private const int maxDirLightCount = 4;

    /// <summary>
    /// 可见光的颜色
    /// </summary>
    private static Vector4[] dirLightColors = new Vector4[maxDirLightCount];

    /// <summary>
    /// 可见光的方向
    /// </summary>
    private static Vector4[] dirLightDirections = new Vector4[maxDirLightCount];

    /// <summary>
    /// 相机剔除结果
    /// </summary>
    private CullingResults cullingResults;

    private const string bufferName = "Lighting";

    private CommandBuffer buffer = new CommandBuffer()
    {
        name = bufferName
    };

    



    public void SetUp(ScriptableRenderContext context,CullingResults cullingResults, ShadowSettings shadowSetting)
    {
        this.cullingResults = cullingResults;

        buffer.BeginSample(bufferName);

        //发送光源数据给shader
        SetupLights();

        buffer.EndSample(bufferName);
        context.ExecuteCommandBuffer(buffer);
        buffer.Clear();
    }

    /// <summary>
    /// 将多个光源数据传给Shader
    /// </summary>
    private void SetupLights()
    {
        //所有可见光
        NativeArray<VisibleLight> visibleLights = cullingResults.visibleLights;

        int dirLightCount = 0;

        for (int i = 0; i < visibleLights.Length; i++)
        {
            VisibleLight visibleLight = visibleLights[i];

            if (visibleLight.lightType == LightType.Directional)
            {
                //只处理方向光
                SetupDirectionalLight(dirLightCount, ref visibleLight);

                dirLightCount++;
                if (dirLightCount >= maxDirLightCount)
                {
                    break;
                }
            }
        }

        //将数据传递给shader
        buffer.SetGlobalInt(dirLightCountId, dirLightCount);
        buffer.SetGlobalVectorArray(dirLightColorsId, dirLightColors);
        buffer.SetGlobalVectorArray(dirLightDirectionsId, dirLightDirections);
    }

    /// <summary>
    /// 将可见的方向光光源数据存储到数组中
    /// </summary>
    private void SetupDirectionalLight(int index,ref VisibleLight visibleLight)
    {
        dirLightColors[index] = visibleLight.finalColor;

        //从localToWorld矩阵的第三列获取光源的forward方向 然后取反
        dirLightDirections[index] = -visibleLight.localToWorldMatrix.GetColumn(2);
    }

   
}
