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
DirectionalShadowData GetDirectionalShadowData(int lightIndex,ShadowData shadowData)
{
    DirectionalShadowData data;

    data.strength = _DirectionalLightShadowData[lightIndex].x * shadowData.strength;
    data.tileIndex = _DirectionalLightShadowData[lightIndex].y;

    //将级联索引和光源的阴影图块索引相加 得到最终的图块索引
    data.tileIndex = _DirectionalLightShadowData[lightIndex].y + shadowData.cascadeIndex;
    
    return data;
}

//获取指定索引的方向光的数据
Light GetDirectionalLight(int index,Surface surfaceWS,ShadowData shadowData)
{
    Light light;

    light.color = _DirectionalLightColors[index].rgb;
    light.direction = _DirectionalLightDirections[index].xyz;

    DirectionalShadowData dirShadowData = GetDirectionalShadowData(index,shadowData);
    light.attenuation = GetDirectionalShadowAttenuation(dirShadowData,shadowData,surfaceWS);

    return light;
}



#endif