#ifndef CUSTOM_UNLIT_PASS_INCLUDED
#define CUSTOM_UNLIT_PASS_INCLUDED

#include "Assets/ShaderLib/Common.hlsl"

//顶点函数
float4 UnlitPassVertex(float3 positionOS : POSITION) : SV_POSITION
{
    float3 positionWS = TransformObjectToWorld(positionOS.xyz);
    return TransformWorldToClip(positionWS);
}

//片元函数
void UnlitPassFragment(){}

#endif