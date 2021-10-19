Shader "CustomRP/Lit"
{
    Properties
    {
        _BaseColor("Color",Color) = (0.5,0.5,0.5,1.0)

        _BaseMap("Texture",2D) = "white"{}

        //透明度裁剪阈值
        _Cutoff("Alpha Cutoff",Range(0.0,1.0)) = 0.5

        //控制是否进行透明度裁剪
        [Toggle(_CLIPPING)]
        _Clipping("Alpha Clipping",Float) = 0

        //控制是否进行预乘
        [Toggle(_PREMULTIPLY_ALPHA)]
        _PremulAlpha("Premultiply Alpha",Float) = 0

        [Enum(UnityEngine.Rendering.BlendMode)]
        _SrcBlend("Src Blend",Float) = 1

         [Enum(UnityEngine.Rendering.BlendMode)]
        _DstBlend("Dst Blend",Float) = 0

        //是否写入深度
        [Enum(Off, 0, On, 1)]
        _ZWrite("Z Write",Float) = 1

        //金属度
        _Metallic("Metallic",Range(0,1)) = 0

        //光滑度
        _Smoothness("Smoothness",Range(0,1)) = 0.5
    }
    SubShader
    {
       

        Pass
        {
            Tags
            {
                //自定义光照
                "LightMode" = "CustomLit"
            }

            Blend[_SrcBlend][_DstBlend]
            ZWrite[_ZWrite]

            HLSLPROGRAM
            #pragma target 3.5
            #pragma shader_feature _CLIPPING
            #pragma shader_feature _PREMULTIPLY_ALPHA

            //GPU实例化
            #pragma multi_compile_instancing

            #pragma vertex LitPassVertex
            #pragma fragment LitPassFragment
            #include "LitPass.hlsl"

            ENDHLSL
           
        }
    }
}
