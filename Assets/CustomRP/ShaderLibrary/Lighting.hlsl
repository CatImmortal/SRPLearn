#ifndef CUSTOM_LIGHTING_INCLUDED
#define CUSTOM_LIGHTING_INCLUDED

//根据物体表面信息后去最终光照结果
float3 GetLighting(Surface surface)
{
    return surface.normal.y;
}

#endif