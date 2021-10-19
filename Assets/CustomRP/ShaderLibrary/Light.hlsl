#ifndef CUSTOM_Light_INCLUDED
#define CUSTOM_Light_INCLUDED

#define MAX_DIRECTIONAL_LIGHT_COUNT 4

//多个方向光的数据
CBUFFER_START(_CustomLight)
    int _DirectionalLightCount;
    float4 _DirectionalLightColors[MAX_DIRECTIONAL_LIGHT_COUNT];
    float4 _DirectionalLightDirections[MAX_DIRECTIONAL_LIGHT_COUNT];
CBUFFER_END 

//灯光
struct Light
{
    float3 color;
    float3 direction;
};

//获取方向光的数量
int GetDirectionalLightCount()
{
    return _DirectionalLightCount;
}

//获取指定索引的方向光的数据
Light GetDirectionalLight(int index)
{
    Light light;
    light.color = _DirectionalLightColors[index].rgb;
    light.direction = _DirectionalLightDirections[index].xyz;
    return light;
}

#endif