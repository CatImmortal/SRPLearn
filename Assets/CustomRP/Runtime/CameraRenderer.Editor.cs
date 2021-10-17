#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;


public partial class CameraRenderer
{
    /// <summary>
    /// SRP不支持的Shader Pass
    /// </summary>
    private static ShaderTagId[] legacyShaderTagIds =
    {
        new ShaderTagId("Always"),
        new ShaderTagId("ForwardBase"),
        new ShaderTagId("PrepassBase"),
        new ShaderTagId("Vertex"),
        new ShaderTagId("VertexLMRGBM"),
        new ShaderTagId("VertexLM"),
    };

    /// <summary>
    /// 表示绘制错误的粉红色材质
    /// </summary>
    private static Material errorMat;

    /// <summary>
    /// 在Game视图绘制的几何体也绘制到Scene视图
    /// </summary>
    private void PrepareForSceneWindow()
    {
        if (camera.cameraType == CameraType.SceneView)
        {
            ScriptableRenderContext.EmitWorldGeometryForSceneView(camera);
        }
    }

    /// <summary>
    /// 在缓冲区执行前调用
    /// </summary>
    private void PrepareBuffer()
    {
        Profiler.BeginSample("Editor Only");
        buffer.name = sampleName = camera.name;
        Profiler.EndSample();
    }

    /// <summary>
    /// 绘制SRP不支持的Shader
    /// </summary>
    private void DrawUnsupportedShaders()
    {
        if (errorMat == null)
        {
            errorMat = new Material(Shader.Find("Hidden/InternalErrorShader"));
        }

        DrawingSettings ds = new DrawingSettings(legacyShaderTagIds[0], new SortingSettings(camera));
        ds.overrideMaterial = errorMat;  //不支持的Shader绘制成粉色

        for (int i = 1; i < legacyShaderTagIds.Length; i++)
        {
            ds.SetShaderPassName(i, legacyShaderTagIds[i]);
        }

        FilteringSettings fs = FilteringSettings.defaultValue;

        context.DrawRenderers(cullingResults, ref ds, ref fs);
    }

    /// <summary>
    /// 绘制辅助线框
    /// </summary>
    private void DrawGizmos()
    {
        if (Handles.ShouldRenderGizmos())
        {
            context.DrawGizmos(camera, GizmoSubset.PreImageEffects);
            context.DrawGizmos(camera, GizmoSubset.PostImageEffects);
        }
    }


}
#endif


