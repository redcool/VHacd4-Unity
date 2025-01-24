#include "VHACD.h"

#ifdef WIN32
#define EXTERN  extern "C" __declspec(dllexport)
#else
#define EXTERN  extern "C"
#endif

EXTERN void* CreateVHACD()
{
    return VHACD::CreateVHACD();
}

EXTERN void DestroyVHACD(void* pVHACD)
{
    auto vhacd = (VHACD::IVHACD*)pVHACD;
    vhacd->Clean();
    vhacd->Release();
}

EXTERN bool ComputeFloat(
    void* pVHACD,
    const float* const points,
    const uint32_t countPoints,
    const uint32_t* const triangles,
    const uint32_t countTriangles,
    const void*  params)
{
    auto vhacd = (VHACD::IVHACD*)pVHACD;
    return vhacd->Compute(points, countPoints, triangles, countTriangles, *(VHACD::IVHACD::Parameters const *)params);
}

EXTERN bool ComputeDouble(
    void* pVHACD,
    const double* const points,
    const uint32_t countPoints,
    const uint32_t* const triangles,
    const uint32_t countTriangles,
    const void* params)
{
    auto vhacd = (VHACD::IVHACD*)pVHACD;
    return vhacd->Compute(points, countPoints, triangles, countTriangles, *(VHACD::IVHACD::Parameters const *)params);
}

EXTERN uint32_t GetNConvexHulls(
    void* pVHACD
    )
{
    auto vhacd = (VHACD::IVHACD*)pVHACD;
    return vhacd->GetNConvexHulls();
}

class ConvexHull {
public:
    double* m_points;
    uint32_t* m_triangles;
    uint32_t m_nPoints;
    uint32_t m_nTriangles;
    double		m_volume;
    double		m_center[3];
};
 VHACD::IVHACD::ConvexHull cacheHull;

EXTERN void GetConvexHull(
    void* pVHACD,
    const uint32_t index ,
    double** points,
    uint32_t** triangles,
    uint32_t* pointNum,
    uint32_t* triNum
    )
{
    auto vhacd = (VHACD::IVHACD*)pVHACD; 
    vhacd->GetConvexHull(index, cacheHull);
    *points =  (double*) & cacheHull.m_points[0];
    *triangles =  (uint32_t*)&cacheHull.m_triangles[0];
    *pointNum = cacheHull.m_points.size();
    *triNum = cacheHull.m_triangles.size();
}
