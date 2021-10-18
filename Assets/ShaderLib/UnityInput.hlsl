#ifndef CUSTOM_UNITY_INPUT_INCLUDED
#define CUSTOM_UNITY_INPUT_INCLUDED


float4x4 unity_ObjectToWorld;

float4x4 unity_WorldToObject;

//这个矩阵包含一些在这里我们不需要的转换信息
float4 unity_WorldTransformParams;


float4x4 unity_MatrixVP;
float4x4 unity_MatrixV;
float4x4 glstate_matrix_projection;

#endif