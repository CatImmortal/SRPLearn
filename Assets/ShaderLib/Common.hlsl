#ifndef CUSTOM_COMMON_INCLUDED
#define CUSTOM_COMMON_INCLUDED


#define UNITY_MATRIX_M unity_ObjectToWorld
#define UNITY_MATRIX_I_M unity_WorldToObject
#define UNITY_MATRIX_V unity_MatrixV
#define UNITY_MATRIX_VP unity_MatrixVP
#define UNITY_MATRIX_P glstate_matrix_projection

#include "UnityInput.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/SpaceTransforms.hlsl"


// //将位置从模型空间转换到世界空间
// float3 TransformObjectToWorld(float3 positionOS)
// {
//     return mul(unity_ObjectToWorld,float4(positionOS,1.0)).xyz;
// }

// //将位置从世界空间转换到裁剪空间
// float4 TransformWorldToClip(float3 positoinWS)
// {
//     return mul(unity_MatrixVP,float4(positoinWS,1.0));
// }

#endif