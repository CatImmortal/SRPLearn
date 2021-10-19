#ifndef CUSTOM_BRDF_INCLUDED
#define CUSTOM_BRDF_INCLUDED

//电介质的反射率平均约0.04
#define MIN_REFLECTIVITY 0.04

struct BRDF 
{
    float3 diffuse;
    float3 specular;

    //粗糙度
    float roughness;
};

float OneMinusReflectivity(float metallic)
{
    //将范围从0-1调整为0-0.96
    float range = 1.0 - MIN_REFLECTIVITY;
    return range - metallic * range;
}

//获取给定表面的BRDF数据
BRDF GetBRDF(Surface surface,bool applyAlphaToDiffuse = false)
{
    BRDF brdf;

    //金属度越高 漫反射越不明显
    float oneMinusReflectivity = OneMinusReflectivity(surface.metallic);
    brdf.diffuse = surface.color * oneMinusReflectivity;

    if(applyAlphaToDiffuse)
    {
        //预乘透明度
        brdf.diffuse *= surface.alpha;
    }
   

    //通过金属度 在最小反射率和表面颜色之间进行插值 得到高光
    brdf.specular = lerp(MIN_REFLECTIVITY,surface.color,surface.metallic);

    //将感知到的光滑度转换为感知到粗糙度 然后对其平方 得到粗糙度
    float perceptualRoughness = PerceptualSmoothnessToPerceptualRoughness(surface.smoothness);
    brdf.roughness = PerceptualRoughnessToRoughness(perceptualRoughness);

    return brdf;
}

//根据公式得到镜面反射强度
float SpecularStrength(Surface surface,BRDF brdf,Light light)
{
    //光源入射方向和视角方向的中间对角线向量
    float3 h = SafeNormalize(light.direction + surface.viewDirection);

    float nh2 = Square(saturate(dot(surface.normal,h)));
    float lh2 = Square(saturate(dot(light.direction,h)));

    float r2 = Square(brdf.roughness);
    float d2 = Square(nh2 * (r2 - 1.0) + 1.00001);
    float normalization = brdf.roughness * 4.0 + 2.0;
    return r2 / (d2 * max(0.1,lh2) * normalization);
}

//直接光照的表面颜色
float3 DirectBRDF(Surface surface,BRDF brdf,Light light)
{
    return SpecularStrength(surface,brdf,light) * brdf.specular + brdf.diffuse;
}

#endif