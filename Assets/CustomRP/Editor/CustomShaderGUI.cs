using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;
public class CustomShaderGUI : ShaderGUI
{
    private MaterialEditor matEditor;
    private Object[] mats;
    private MaterialProperty[] matProps;

    private bool shoPresets;

    private bool Clipping
    {
        set
        {
            SetProp("_Clipping", "_CLIPPING", value);
        }
    }

    private bool PremultiplyAlpha
    {
        set
        {
            SetProp("_PremulAlpha", "_PREMULTIPLY_ALPHA", value);
        }
    }

    private BlendMode SrcBlend
    {
        set
        {
            SetProp("_SrcBlend",(float)value);
        }
    }

    private BlendMode DstBlend
    {
        set
        {
            SetProp("_DstBlend", (float)value);
        }
    }

    private bool ZWrite
    {
        set
        {
            SetProp("_ZWrite", value ? 1f : 0f);
        }
    }

    private RenderQueue RenderQueue
    {
        set
        {
            foreach (Material mat in mats)
            {
                mat.renderQueue = (int)value;
            }
        }
    }

  

    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        base.OnGUI(materialEditor, properties);

        matEditor = materialEditor;
        mats = matEditor.targets;
        matProps = properties;

        EditorGUILayout.Space();
        shoPresets = EditorGUILayout.Foldout(shoPresets, "Presets", true);
        if (shoPresets)
        {
            OpaquePreset();
            ClipPreset();
            FadePreset();
            TransparentPreset();
        }
    }

    /// <summary>
    /// 是否有此属性
    /// </summary>
    private bool HasProp(string name)
    {
        return FindProperty(name, matProps, false) != null;
    }

    /// <summary>
    /// 设置材质属性
    /// </summary>
    private bool SetProp(string name,float value)
    {
        MaterialProperty prop = FindProperty(name, matProps, false);
        if (prop != null)
        {
            prop.floatValue = value;
            return true;
        }

        return false;
    }


    /// <summary>
    /// 设置关键字是否开启
    /// </summary>
    private void SetKeyword(string keyword, bool enabled)
    {
        foreach (Material mat in mats)
        {
            if (enabled)
            {
                mat.EnableKeyword(keyword);
            }
            else
            {
                mat.DisableKeyword(keyword);
            }
        }
    }

    /// <summary>
    /// 设置材质属性
    /// </summary>
    private void SetProp(string name,string keyword, bool value)
    {
        if (SetProp(name, value ? 1f : 0f))
        {
            SetKeyword(keyword, value);
        }
       
    }

    private bool PresetButton(string name)
    {
        if (GUILayout.Button(name))
        {
            matEditor.RegisterPropertyChangeUndo(name);
            return true;
        }

        return false;
    }

    /// <summary>
    /// 设置为不透明渲染模式
    /// </summary>
    private void OpaquePreset()
    {
        if (PresetButton("Opaque"))
        {
            Clipping = false;
            PremultiplyAlpha = false;
            SrcBlend = BlendMode.One;
            DstBlend = BlendMode.Zero;
            ZWrite = true;
            RenderQueue = RenderQueue.Geometry;
        }
    }

    /// <summary>
    /// 设置为裁剪模式
    /// </summary>
    private void ClipPreset()
    {
        if (PresetButton("Clip"))
        {
            Clipping = true;
            PremultiplyAlpha = false;
            SrcBlend = BlendMode.One;
            DstBlend = BlendMode.Zero;
            ZWrite = true;
            RenderQueue = RenderQueue.AlphaTest;
        }
    }

    /// <summary>
    /// 设置为透明渲染模式
    /// </summary>
    private void FadePreset()
    {
        if (PresetButton("Fade"))
        {
            Clipping = false;
            PremultiplyAlpha = false;
            SrcBlend = BlendMode.SrcAlpha;
            DstBlend = BlendMode.OneMinusSrcAlpha;
            ZWrite = false;
            RenderQueue = RenderQueue.Transparent;
        }
    }

    /// <summary>
    /// 设置为预乘透明度的透明渲染模式
    /// </summary>
    private void TransparentPreset()
    {
        if (HasProp("_PremulAlpha") &&  PresetButton("Transparent"))
        {
            Clipping = false;
            PremultiplyAlpha = true;
            SrcBlend = BlendMode.One;
            DstBlend = BlendMode.OneMinusSrcAlpha;
            ZWrite = false;
            RenderQueue = RenderQueue.Transparent;
        }
    }
}
