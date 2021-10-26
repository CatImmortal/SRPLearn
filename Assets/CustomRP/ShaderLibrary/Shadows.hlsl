//阴影采样

#ifndef CUSTOM_SHADOWS_INCLUDED
#define CUSTOM_SHADOWS_INCLUDED

#define MAX_SHADOWED_DIRECTIONAL_LIGHT_COUNT 4
#define MAX_CASCADE_COUNT 4

//阴影图集
TEXTURE2D_SHADOW(_DirectionalShadowAtlas);
#define SHADOW_SAMPLER sampler_linear_clamp_compare
SAMPLER_CMP(SHADOW_SAMPLER);

CBUFFER_START(_CustomShadows)
//级联数量与包围球
int _CascadeCount;
float4 _CascadeCullingSpheres[MAX_CASCADE_COUNT];

//级联数据
float4 _CascadeData[MAX_CASCADE_COUNT];

//阴影转换矩阵
float4x4 _DirectionalShadowMatrices[MAX_SHADOWED_DIRECTIONAL_LIGHT_COUNT * MAX_CASCADE_COUNT];


//float _ShadowDistance;

//阴影过渡距离
float4 _ShadowDistanceFade;

CBUFFER_END

//方向光阴影数据
struct DirectionalShadowData
{
    float strength;
    int tileIndex;
};

//阴影数据
struct ShadowData
{
    int cascadeIndex;

    //是否采样阴影的标识符
    float strength;
};

//计算阴影过渡时的强度
float FadedShadowStrength(float distance,float scale,float fade)
{
    return saturate((1.0-distance*scale)*fade);

}

//获取世界空间表面的阴影数据
ShadowData GetShadowData(Surface surfaceWS)
{
    ShadowData data;
    data.strength = FadedShadowStrength(surfaceWS.depth,_ShadowDistanceFade.x,_ShadowDistanceFade.y);

    int i;
    for(i = 0;i < _CascadeCount;i++)
    {
        float4 sphere = _CascadeCullingSpheres[i];
        float distanceSqr = DistanceSquared(surfaceWS.position,sphere.xyz);
        if(distanceSqr < sphere.w)
        {
            if(i == _CascadeCount - 1)
            {
                //如果在最后一个级联的范围中，需要计算级联的过渡阴影强度
                data.strength *= FadedShadowStrength(distanceSqr,_CascadeData[i].x,_ShadowDistanceFade.z);
            }

            //如果物体表面到球心的平方距离小于球体半径的平方
            //就说明该物体在这层级联包围球中 得到合适的级联层级索引
            break;
        }
    }
    if(i == _CascadeCount)
    {
        //如果超出最后一个级联的范围 就不对阴影进行采样
        data.strength = 0.0;
    }
    data.cascadeIndex = i;
    return data;
}

//采样阴影图集
float SampleDirectionalShadowAtlas(float3 positionSTS)
{
    return SAMPLE_TEXTURE2D_SHADOW(_DirectionalShadowAtlas,SHADOW_SAMPLER,positionSTS);
}

//计算阴影衰减
float GetDirectionalShadowAttenuation(DirectionalShadowData data,ShadowData global,Surface surfaceWS,)
{
    //片元完全被阴影覆盖则返回0，没有任何阴影则为1

    if(data.strength <= 0.0)
    {
        //阴影强度<=0 直接返回衰减为1
        return 1.0;
    }

    //计算法线偏差
    float3 normalBias = surfaceWS.normal * _CascadeData[global.cascadeIndex].y;

    //加上法线偏移后的顶点位置 得到阴影纹理空间的新位置，然后对图集进行采样
    float3 positionSTS = mul(_DirectionalShadowMatrices[data.tileIndex],float4(surfaceWS.position + normalBias,1.0)).xyz;

    float shadow = SampleDirectionalShadowAtlas(positionSTS);
    return lerp(1.0,shadow,data.strength);
}

#endif