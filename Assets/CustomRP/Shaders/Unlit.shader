Shader "CustomRP/Unlit"
{
    Properties
    {
        _BaseColor("Color",Color) = (1.0,1.0,1.0,1.0)

        _BaseMap("Texture",2D) = "white"{}

        //透明度裁剪阈值
        _Cutoff("Alpha Cutoff",Range(0.0,1.0)) = 0.5

        //控制是否进行透明度裁剪
        [Toggle(_CLIPPING)]
        _Clipping("Alpha Clipping",Float) = 0

        [Enum(UnityEngine.Rendering.BlendMode)]
        _SrcBlend("Src Blend",Float) = 1

         [Enum(UnityEngine.Rendering.BlendMode)]
        _DstBlend("Dst Blend",Float) = 0

        //是否写入深度
        [Enum(Off, 0, On, 1)]
        _ZWrite("Z Write",Float) = 1
    }
    SubShader
    {
       

        Pass
        {
            Blend[_SrcBlend][_DstBlend]
            ZWrite[_ZWrite]

            HLSLPROGRAM
            
            #pragma shader_feature _CLIPPING

            //GPU实例化
            #pragma multi_compile_instancing

            #pragma vertex UnlitPassVertex
            #pragma fragment UnlitPassFragment
            #include "UnlitPass.hlsl"

            ENDHLSL
           
        }
    }
}
