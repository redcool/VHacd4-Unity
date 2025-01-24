using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(VHACDExt))]
public class VHACDExtInspector : Editor
{
    private VHACDExt script { get { return target as VHACDExt; } }
    public override void OnInspectorGUI()
    {

        GUILayout.Label("The maximum number of convex hulls to produce");
        script.m_parameters.m_maxConvexHulls = (uint)EditorGUILayout.IntSlider((int)script.m_parameters.m_maxConvexHulls,1, 1024);
        GUILayout.Label("The voxel resolution to use");
        script.m_parameters.m_resolution = (uint)EditorGUILayout.IntSlider((int)script.m_parameters.m_resolution, 40000, 1000000000);
        GUILayout.Label("f the voxels are within 1% of the volume of the hull, we consider this a close enough approximation");
        script.m_parameters.m_minimumVolumePercentErrorAllowed = EditorGUILayout.Slider((float)script.m_parameters.m_minimumVolumePercentErrorAllowed, 0.1f, 100);
        GUILayout.Label("The maximum recursion depth");
        script.m_parameters.m_maxRecursionDepth = (uint)EditorGUILayout.IntSlider((int)script.m_parameters.m_maxRecursionDepth, 1, 16);
        GUILayout.Label(" Whether or not to shrinkwrap the voxel positions to the source mesh on output");
        script.m_parameters.m_shrinkWrap = EditorGUILayout.Toggle(script.m_parameters.m_shrinkWrap);
        GUILayout.Label("How to fill the interior of the voxelized mesh");
        script.m_parameters.m_fillMode = (uint)EditorGUILayout.IntSlider((int)script.m_parameters.m_fillMode, 0, 2);
        GUILayout.Label("The maximum number of vertices allowed in any output convex hull");
        script.m_parameters.m_maxNumVerticesPerCH = (uint)EditorGUILayout.IntField((int)script.m_parameters.m_maxNumVerticesPerCH);

        GUILayout.Label("Whether or not to run asynchronously, taking advantage of additional cores");
        script.m_parameters.m_asyncACD = EditorGUILayout.Toggle(script.m_parameters.m_asyncACD);
        GUILayout.Label("Once a voxel patch has an edge length of less than 4 on all 3 sides, we don't keep recursing");
        script.m_parameters.m_minEdgeLength = (uint)EditorGUILayout.IntSlider((int)script.m_parameters.m_minEdgeLength, 1, 3);
        GUILayout.Label("Whether or not to attempt to split planes along the best location. Experimental feature. False by default.");
        script.m_parameters.m_findBestPlane = EditorGUILayout.Toggle(script.m_parameters.m_findBestPlane);

        
        if(GUILayout.Button("Generate HullMesh"))
        {
            script.GenerateConvexAndSave();
        }

    }
}
 
