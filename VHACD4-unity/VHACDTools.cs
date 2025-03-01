
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor.Formats.Fbx.Exporter;
using UnityEditor;
#endif
using UnityEngine;
using System.IO;

namespace MeshProcess
{
    public class VHACDTools : MonoBehaviour
    {
        public static string ConvexHullGenFolder = "Assets/ConvexHulls";
        List<Mesh> mOriginMeshes = new List<Mesh>();

        public VHACD vhacd = new();

        private void Start()
        {
        }
        public void GenerateConvexAndSave()
        {
            mOriginMeshes.Clear();

            MeshFilter[] meshFilteres = GetComponentsInChildren<MeshFilter>();
            for (int i = 0; i < meshFilteres.Length; i++)
            {
                mOriginMeshes.Add(meshFilteres[i].sharedMesh);
            }

            List<Mesh> listMeshes = new List<Mesh>();
            if (mOriginMeshes.Count > 0)
            {
                for (int i = 0; i < mOriginMeshes.Count; i++)
                {
                    List<Mesh> convertMesh = vhacd.GenerateConvexMeshes(mOriginMeshes[i]);
                    if (convertMesh.Count > 0)
                        listMeshes.AddRange(convertMesh);

                }

                ExtractToFBX(listMeshes, gameObject.name + "_col.FBX");
            }
        }

        public static void ExtractToFBX(List<Mesh> listMeshes, string convexName)
        {
            if (!Directory.Exists(ConvexHullGenFolder))
                Directory.CreateDirectory(ConvexHullGenFolder);
            GameObject expGo = new GameObject();
            for (int i = 0; i < listMeshes.Count; i++)
            {
                GameObject meshGo = new GameObject("colMesh" + i);
                MeshFilter filter = meshGo.AddComponent<MeshFilter>();
                filter.mesh = listMeshes[i];
                meshGo.transform.parent = expGo.transform;
            }

#if UNITY_EDITOR
            string fbxPath = ModelExporter.ExportObject(Path.Combine(ConvexHullGenFolder, convexName), expGo);
            // set asset importer
            var fbxImporter = AssetImporter.GetAtPath(fbxPath) as ModelImporter;
            fbxImporter.addCollider = true;
            fbxImporter.SaveAndReimport();
            // setup meshCollider
            var fbxConvexHullGo = AssetDatabase.LoadAssetAtPath<GameObject>(fbxPath);

            EditorGUIUtility.PingObject(fbxConvexHullGo);
#endif

            GameObject.DestroyImmediate(expGo);
        }

        //[ContextMenu("Generate Convex Meshes")]
        //public List<Mesh> GenerateConvexMeshes(Mesh mesh = null)
        //{
        //    return null;
        //}
    }
}