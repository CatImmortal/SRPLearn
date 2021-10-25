using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum TextureSize
{
    _256 = 256,
    _512 = 512,
    _1024 = 1024,
    _2048 = 2048,
    _4096 = 4096,
    _8192 = 8192,
}

/// <summary>
/// 方向光的阴影配置
/// </summary>
[Serializable]
public struct Directional
{
    public TextureSize atlasSize;

    /// <summary>
    /// 阴影级联数量滑块
    /// </summary>
    [Range(1,4)]
    public int CascadeCount;

    [Range(0f,1f)]
    public float cascadeRatio1;

    [Range(0f, 1f)]
    public float cascadeRatio2;

    [Range(0f, 1f)]
    public float cascadeRatio3;

    /// <summary>
    /// 级联淡入值
    /// </summary>
    [Range(0.001f, 1f)]
    public float cascadeFade;

    public Vector3 CascadeRatios
    {
        get
        {
            return new Vector3(cascadeRatio1, cascadeRatio2, cascadeRatio3);
        }
    }

}

/// <summary>
/// 阴影配置
/// </summary>
[Serializable]
public class ShadowSettings
{
   
    /// <summary>
    /// 阴影
    /// </summary>
    [Min(0.001f)]
    public float MaxDistance = 100f;

    /// <summary>
    /// 阴影过渡距离
    /// </summary>
    [Range(0.001f,1f)]
    public float distanceFade = 0.1f;



    public Directional Directional = new Directional()
    {
        atlasSize = TextureSize._1024,
        CascadeCount = 4,
        cascadeRatio1 = 0.1f,
        cascadeRatio2 = 0.25f,
        cascadeRatio3 = 0.5f,
        cascadeFade = 0.1f,
    };
}




