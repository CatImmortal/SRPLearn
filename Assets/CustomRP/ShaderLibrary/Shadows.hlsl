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
//阴影转换矩阵
float4x4 _DirectionalShadowMatrices[MAX_SHADOWED_DIRECTIONAL_LIGHT_COUNT * MAX_CASCADE_COUNT];
CBUFFER_END

//阴影数据
struct DirectionalShadowData
{
    float strength;
    int tileIndex;
};

//采样阴影图集
float SampleDirectionalShadowAtlas(float3 positionSTS)
{
    return SAMPLE_TEXTURE2D_SHADOW(_DirectionalShadowAtlas,SHADOW_SAMPLER,positionSTS);
}

//计算阴影衰减
float GetDirectionalShadowAttenuation(DirectionalShadowData data,Surface surfaceWS)
{
    //片元完全被阴影覆盖则返回0，没有任何阴影则为1

    if(data.strength <= 0.0)
    {
        //阴影强度<=0 直接返回衰减为1
        return 1.0;
    }

    //将表面位置从世界空间转到阴影纹理空间，然后对阴影图集采样
    float3 positionSTS = mul(_DirectionalShadowMatrices[data.tileIndex],float4(surfaceWS.position,1.0)).xyz;

    float shadow = SampleDirectionalShadowAtlas(positionSTS);
    return lerp(1.0,shadow,data.strength);
}

#endif