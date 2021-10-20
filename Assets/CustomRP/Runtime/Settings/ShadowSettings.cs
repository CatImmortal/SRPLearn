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
}

/// <summary>
/// 阴影配置
/// </summary>
[Serializable]
public class ShadowSettings
{
   

    [Min(0f)]
    public float MaxDistance = 100f;

    public Directional Directional = new Directional()
    {
        atlasSize = TextureSize._1024
    };
}



