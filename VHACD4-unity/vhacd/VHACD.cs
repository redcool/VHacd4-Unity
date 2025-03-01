using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

namespace MeshProcess
{
    public class VHACD
    {
        [System.Serializable]
        public unsafe struct Parameters
        {
            //uint32_t m_maxConvexHulls { 64 };         // The maximum number of convex hulls to produce
            //uint32_t m_resolution { 400000 };         // The voxel resolution to use
            //double m_minimumVolumePercentErrorAllowed { 1 }; // if the voxels are within 1% of the volume of the hull, we consider this a close enough approximation
            //uint32_t m_maxRecursionDepth { 10 };        // The maximum recursion depth
            //bool m_shrinkWrap {true};             // Whether or not to shrinkwrap the voxel positions to the source mesh on output
            //FillMode m_fillMode { FillMode::FLOOD_FILL }; // How to fill the interior of the voxelized mesh
            //uint32_t m_maxNumVerticesPerCH { 64 };    // The maximum number of vertices allowed in any output convex hull
            //bool m_asyncACD { true };             // Whether or not to run asynchronously, taking advantage of additional cores
            //uint32_t m_minEdgeLength { 2 };           // Once a voxel patch has an edge length of less than 4 on all 3 sides, we don't keep recursing
            //bool m_findBestPlane { false };       // Whether or not to attempt to split planes along the best location. Experimental feature. False by default.


            public void Init()
            {
                m_callback = null;            // Optional user provided callback interface for progress
                m_logger = null;              // Optional user provided callback interface for log messages
                m_taskRunner = null;          // Optional user provided interface for creating tasks
                m_maxConvexHulls = 64;         // The maximum number of convex hulls to produce
                m_resolution =  400000 ;         // The voxel resolution to use
                m_minimumVolumePercentErrorAllowed = 1; // if the voxels are within 1% of the volume of the hull, we consider this a close enough approximation
                m_maxRecursionDepth  =10;        // The maximum recursion depth
                m_shrinkWrap = true;             // Whether or not to shrinkwrap the voxel positions to the source mesh on output
                m_fillMode = 0;   // How to fill the interior of the voxelized mesh
                                    // FLOOD_FILL, // This is the default behavior, after the voxelization step it uses a flood fill to determine 'inside'
                                    //            // from 'outside'. However, meshes with holes can fail and create hollow results.
                                    //SURFACE_ONLY, // Only consider the 'surface', will create 'skins' with hollow centers.
                                    //RAYCAST_FILL, // Uses raycasting to determine inside from outside.
                m_maxNumVerticesPerCH  = 64 ;    // The maximum number of vertices allowed in any output convex hull
                m_asyncACD  = true ;             // Whether or not to run asynchronously, taking advantage of additional cores
                m_minEdgeLength = 2;           // Once a voxel patch has an edge length of less than 4 on all 3 sides, we don't keep recursing
                m_findBestPlane =false;       // Whether or not to attempt to split planes along the best location. Experimental feature. False by default.

            }

            void* m_callback;            // Optional user provided callback interface for progress
            void* m_logger;              // Optional user provided callback interface for log messages
            void* m_taskRunner;          // Optional user provided interface for creating tasks

            public uint m_maxConvexHulls;         // The maximum number of convex hulls to produce
            public uint m_resolution;         // The voxel resolution to use
            public double m_minimumVolumePercentErrorAllowed ; // if the voxels are within 1% of the volume of the hull, we consider this a close enough approximation
            public uint m_maxRecursionDepth;        // The maximum recursion depth
            public bool m_shrinkWrap  ;             // Whether or not to shrinkwrap the voxel positions to the source mesh on output
            public uint m_fillMode; // How to fill the interior of the voxelized mesh
            public uint m_maxNumVerticesPerCH ;    // The maximum number of vertices allowed in any output convex hull
            public bool m_asyncACD;             // Whether or not to run asynchronously, taking advantage of additional cores
            public uint m_minEdgeLength;           // Once a voxel patch has an edge length of less than 4 on all 3 sides, we don't keep recursing
            public bool m_findBestPlane;       // Whether or not to attempt to split planes along the best location. Experimental feature. False by default.

 
        };

 

        [DllImport("libvhacd")] static extern unsafe void* CreateVHACD();

        [DllImport("libvhacd")] static extern unsafe void DestroyVHACD(void* pVHACD);

        [DllImport("libvhacd")]
        static extern unsafe bool ComputeFloat(
            void* pVHACD,
            float* points,
            uint countPoints,
            uint* triangles,
            uint countTriangles,
            Parameters* parameters);

        [DllImport("libvhacd")]
        static extern unsafe bool ComputeDouble(
            void* pVHACD,
            double* points,
            uint countPoints,
            uint* triangles,
            uint countTriangles,
            Parameters* parameters);

        [DllImport("libvhacd")] static extern unsafe uint GetNConvexHulls(void* pVHACD);

        [DllImport("libvhacd")]
        static extern unsafe void GetConvexHull(
            void* pVHACD,
            uint index,
        out IntPtr points,
            out IntPtr triangles,
            out uint pointNum,
            out uint triNum);

        public Parameters m_parameters;

        public  VHACD() { m_parameters.Init(); }

        [ContextMenu("Generate Convex Meshes")]
        public unsafe List<Mesh> GenerateConvexMeshes(Mesh mesh = null)
        {
            var vhacd = CreateVHACD();
            var parameters = m_parameters;

            var verts = mesh.vertices;
            var tris = mesh.triangles;
            fixed (Vector3* pVerts = verts)
            fixed (int* pTris = tris)
            {
                ComputeFloat(
                    vhacd,
                    (float*)pVerts, (uint)verts.Length,
                    (uint*)pTris, (uint)tris.Length / 3,
                    &parameters);
            }

            var numHulls = GetNConvexHulls(vhacd);
            List<Mesh> convexMesh = new List<Mesh>((int)numHulls);
            foreach (var index in Enumerable.Range(0, (int)numHulls))
            {
                IntPtr points;
                IntPtr tries;
                uint pointNum;
                uint triNum;
                GetConvexHull(vhacd, (uint)index, out points, out tries,out pointNum,out triNum);

                double[] outPoints = new double[pointNum*3];
                Marshal.Copy(points,outPoints,  0, (int)pointNum * 3);


                var hullMesh = new Mesh();
                var hullVerts = new Vector3[pointNum];
       
                for (var pointCount = 0; pointCount < pointNum; pointCount++)
                {
                    hullVerts[pointCount].x = (float)outPoints[pointCount * 3 + 0];
                    hullVerts[pointCount].y = (float)outPoints[pointCount * 3 + 1];
                    hullVerts[pointCount].z = (float)outPoints[pointCount * 3 + 2];
                     
                }
             

                hullMesh.SetVertices(hullVerts);

                var indices = new int[triNum * 3];
                Marshal.Copy(tries, indices, 0, indices.Length);
                hullMesh.SetTriangles(indices, 0);


                convexMesh.Add(hullMesh);
            }
            return convexMesh;
        }
    }
}
