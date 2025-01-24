using Autodesk.Fbx;
using MeshProcess;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Formats.Fbx.Exporter;
using UnityEditor;
using UnityEngine;
using UnityEngine.XR;
using System.IO;

public class VHACDExt :  VHACD
{
    public static string ConvexHullGenFolder = "Assets/ConvexHulls";
    List<Mesh> mOriginMeshes = new List<Mesh>();

    private void Start()
    { 
    }
    public void GenerateConvexAndSave()
    {
        mOriginMeshes.Clear();
  
        MeshFilter[] meshFilteres = GetComponentsInChildren<MeshFilter>();
        for(int i=0;i<meshFilteres.Length;i++)
        {
            mOriginMeshes.Add(meshFilteres[i].sharedMesh);
        } 
      
        List<Mesh> listMeshes = new List<Mesh>();
        if (mOriginMeshes.Count>0)
        {
            for (int i = 0; i < mOriginMeshes.Count; i++)
            {
                List<Mesh> convertMesh = GenerateConvexMeshes(mOriginMeshes[i]);
                if(convertMesh.Count> 0)
                    listMeshes.AddRange(convertMesh);

            }

            ExtractToFBX(listMeshes, gameObject.name + "_col.FBX");
        }
    }
     
    public static void ExtractToFBX(List<Mesh> listMeshes,string convexName)
    {
        if (!Directory.Exists(ConvexHullGenFolder))
            Directory.CreateDirectory(ConvexHullGenFolder);
        GameObject expGo = new GameObject();
        for(int i=0;i< listMeshes.Count;i++)
        {
            GameObject meshGo = new GameObject("colMesh" + i) ;
            MeshFilter filter = meshGo.AddComponent<MeshFilter>(); 
            filter.mesh = listMeshes[i];
            meshGo.transform.parent = expGo.transform;
        }
       
       
        string fbxPath = ModelExporter.ExportObject(Path.Combine(ConvexHullGenFolder, convexName), expGo);
        Debug.Log(fbxPath + "Genearated");

        GameObject.DestroyImmediate(expGo);
    } 
}
