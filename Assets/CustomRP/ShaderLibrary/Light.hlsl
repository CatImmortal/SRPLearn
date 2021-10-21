#ifndef CUSTOM_Light_INCLUDED
#define CUSTOM_Light_INCLUDED

#define MAX_DIRECTIONAL_LIGHT_COUNT 4

//多个方向光的数据
CBUFFER_START(_CustomLight)
    int _DirectionalLightCount;
    float4 _DirectionalLightColors[MAX_DIRECTIONAL_LIGHT_COUNT];
    float4 _DirectionalLightDirections[MAX_DIRECTIONAL_LIGHT_COUNT];
    float4 _DirectionalLightShadowData[MAX_DIRECTIONAL_LIGHT_COUNT];
CBUFFER_END 

//灯光
struct Light
{
    float3 color;
    float3 direction;
    float attenuation;
};

//获取方向光的数量
int GetDirectionalLightCount()
{
    return _DirectionalLightCount;
}

//获取指定索引的方向光阴影数据
DirectionalShadowData GetDirectionalShadowData(int lightIndex)
{
    DirectionalShadowData data;

    data.strength = _DirectionalLightShadowData[lightIndex].x;
    data.tileIndex = _DirectionalLightShadowData[lightIndex].y;

    return data;
}

//获取指定索引的方向光的数据
Light GetDirectionalLight(int index,Surface surfaceWS)
{
    Light light;

    light.color = _DirectionalLightColors[index].rgb;
    light.direction = _DirectionalLightDirections[index].xyz;

    DirectionalShadowData shadowData = GetDirectionalShadowData(index);
    light.attenuation = GetDirectionalShadowAttenuation(shadowData,surfaceWS);

    return light;
}



#endif