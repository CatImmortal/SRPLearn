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

    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        base.OnGUI(materialEditor, properties);

        matEditor = materialEditor;
        mats = matEditor.targets;
        matProps = properties;
    }

    /// <summary>
    /// 设置材质属性
    /// </summary>
    private void SetProp(string name,float value)
    {
        FindProperty(name, matProps).floatValue = value;
    }

    /// <summary>
    /// 设置材质属性
    /// </summary>
    private void SetProp(string name,string keyword, bool value)
    {
        SetProp(name, value ? 1f : 0f);
        SetKeyword(keyword, value);
    }

    /// <summary>
    /// 设置关键字
    /// </summary>
    private void SetKeyword(string keyword,bool enabled)
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
}
