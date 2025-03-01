#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MeshProcess
{

    [CustomEditor(typeof(VHACDTools))]
    public class VHACDToolsInspector : Editor
    {
        int selectedLanguageId = 0;
        GUIContent[] languageTexts = new []{new GUIContent("CN"),new GUIContent("EN") };

        GUIContent
            m_maxConvexHulls_CN = new GUIContent("最大凸包数量", "指定算法最终生成的凸形状（convex hulls）的最大数量，基于游戏中应用的话，如果分解后的凸包Mesh近似原始模型，该值越小越好"),
            m_maxConvexHulls_EN = new GUIContent("The maximum number of convex hulls to produce"),

            m_resolution_CN = new GUIContent("体素分辨率", "控制体素化过程中将模型分割成多少个小立方体（体素），相当于“网格精度”。分辨率越高（数值越大），分解越精细，能捕捉更多细节，但计算时间和内存占用会显著增加；分辨率低则更快但精度下降。\r\n\r\n典型取值：10000 到 1000000（根据模型大小调整，当设置过大时远超1000000时\r\n，会有生成模型位置错误问题，还未找到算法bug产生原因）"),
            m_resolution_EN = new GUIContent("The voxel resolution to use", "more resolution more "),

            m_minimumVolumePercentErrorAllowed_CN = new GUIContent("允许的体积误差百分比", "用于控制分解后的凸形状与原始模型体积的接近程度。值越小，分解结果越贴近原始模型的体积，精度更高；值越大，允许更大误差，分解更粗糙，一般设置0.1% 到 5%之间"),
            m_minimumVolumePercentErrorAllowed_EN = new GUIContent("minimum Volume PercentError Allowed", "if the voxels are within 1% of the volume of the hull, we consider this a close enough approximation"),

            m_maxRecursionDepth_CN = new GUIContent("递归最大深度", "递归深度越大，分解越细致，能处理更复杂的凹形区域；深度小则分解更简单，速度更快，取值在10 到 20。"),
            m_maxRecursionDepth_EN = new GUIContent("maxRecursionDepth", "The maximum recursion depth"),

            m_shrinkWrap_CN = new GUIContent("收缩到表面", "决定是否将生成的凸包尽量“贴合”原始模型表面，开启（true）时，凸包更紧凑，减少多余空间；关闭（false）时，凸包可能更大但计算更快，通常设为 true。"),
            m_shrinkWrap_EN = new GUIContent("shrinkWrap", "Whether or not to shrinkwrap the voxel positions to the source mesh on output"),

            m_fillMode_CN = new GUIContent("指定体素化的填充方式", "决定如何处理模型内部。选择0,Flood Fill（洪水填充）：填充所有封闭的内部空间；选择1,Surface Only（仅表面）：只考虑模型表面，不填充内部；选择2，Raycast Fill（射线填充）：使用射线检测填充内部。选择不同模式会影响分解结果的准确性，一般封闭实心Mesh选择0，有镂空或者非封闭的Mesh选择2，只有单层表面的Mesh选择1\r\n\r\n"),
            m_fillMode_EN = new GUIContent("FillMode", "How to fill the interior of the voxelized mesh"),

            m_maxNumVerticesPerCH_CN = new GUIContent("设置生成单个凸包的最大顶点数量", "值越小，凸包更简单（接近多面体），适合实时应用；值越大，凸包更复杂，能更好地拟合形状，Unity中该值必须小于255."),
            m_maxNumVerticesPerCH_EN = new GUIContent("maxNumVerticesPerConvexHull", "The maximum number of vertices allowed in any output convex hull"),

            m_asyncACD_CN = new GUIContent("异步处理", "是否启动多线程异步处理"),
            m_asyncACD_EN = new GUIContent("asyncACD", "Whether or not to run asynchronously, taking advantage of additional cores"),

            m_minEdgeLength_CN = new GUIContent("限制凸包的最小边长", "避免生成过小的碎片。值越大，小细节会被忽略，结果更简洁；值越小，细节保留更多,0.01 到 0.1（单位取决于模型尺度）。"),
            m_minEdgeLength_EN = new GUIContent("minEdgeLength", "Once a voxel patch has an edge length of less than 4 on all 3 sides, we don't keep recursing"),

            m_findBestPlane_CN = new GUIContent("查找最佳分割平面", "决定是否在分解时优化分割平面的选择。开启（true）时，结果更合理但计算量增加；关闭（false）时速度更快但分解可能不够理想。通常设为 true，除非追求极致性能。"),
            m_findBestPlane_EN = new GUIContent("findBestPlane", "Whether or not to attempt to split planes along the best location. Experimental feature")
            ;

        Dictionary<string, GUIContent[]> labelDict;
        private void OnEnable()
        {
            SetupLabelDict();
        }

        private void SetupLabelDict()
        {
            labelDict = new Dictionary<string, GUIContent[]>
            {
                { "m_maxConvexHulls", new[]{ m_maxConvexHulls_CN, m_maxConvexHulls_EN } },
                { "m_resolution", new[]{ m_resolution_CN, m_resolution_EN } },
                { "m_minimumVolumePercentErrorAllowed", new[]{ m_minimumVolumePercentErrorAllowed_CN, m_minimumVolumePercentErrorAllowed_EN } },
                { "m_maxRecursionDepth", new[]{ m_maxRecursionDepth_CN, m_maxRecursionDepth_EN } },
                { "m_shrinkWrap", new[]{ m_shrinkWrap_CN, m_shrinkWrap_EN } },

                { "m_fillMode", new[]{ m_fillMode_CN, m_fillMode_EN } },
                { "m_maxNumVerticesPerCH", new[]{ m_maxNumVerticesPerCH_CN, m_maxNumVerticesPerCH_EN } },
                { "m_asyncACD", new[]{ m_asyncACD_CN, m_asyncACD_EN } },
                { "m_minEdgeLength", new[]{ m_minEdgeLength_CN, m_minEdgeLength_EN } },
                { "m_findBestPlane", new[]{ m_findBestPlane_CN, m_findBestPlane_EN } },
            };
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox("Generate Convex Mesh use vhacd", MessageType.Info);

            GUILayout.Label("Language:");
            selectedLanguageId = GUILayout.SelectionGrid(selectedLanguageId, languageTexts, languageTexts.Length);

            var vhacdTools = target as VHACDTools;
            var vhacd = vhacdTools.vhacd;

            GUILayout.Label(labelDict[nameof(VHACD.Parameters.m_maxConvexHulls)][selectedLanguageId]);
            vhacd.m_parameters.m_maxConvexHulls = (uint)EditorGUILayout.IntSlider((int)vhacd.m_parameters.m_maxConvexHulls, 1, 1024);

            GUILayout.Label(labelDict[nameof(VHACD.Parameters.m_resolution)][selectedLanguageId]);
            vhacd.m_parameters.m_resolution = (uint)EditorGUILayout.IntSlider((int)vhacd.m_parameters.m_resolution, 40000, 1000000000);

            GUILayout.Label(labelDict[nameof(VHACD.Parameters.m_minimumVolumePercentErrorAllowed)][selectedLanguageId]);
            vhacd.m_parameters.m_minimumVolumePercentErrorAllowed = EditorGUILayout.Slider((float)vhacd.m_parameters.m_minimumVolumePercentErrorAllowed, 0.1f, 100);

            GUILayout.Label(labelDict[nameof(VHACD.Parameters.m_maxRecursionDepth)][selectedLanguageId]);
            vhacd.m_parameters.m_maxRecursionDepth = (uint)EditorGUILayout.IntSlider((int)vhacd.m_parameters.m_maxRecursionDepth, 1, 16);

            GUILayout.Label(labelDict[nameof(VHACD.Parameters.m_shrinkWrap)][selectedLanguageId]);
            vhacd.m_parameters.m_shrinkWrap = EditorGUILayout.Toggle(vhacd.m_parameters.m_shrinkWrap);

            GUILayout.Label(labelDict[nameof(VHACD.Parameters.m_fillMode)][selectedLanguageId]);
            vhacd.m_parameters.m_fillMode = (uint)EditorGUILayout.IntSlider((int)vhacd.m_parameters.m_fillMode, 0, 2);

            GUILayout.Label(labelDict[nameof(VHACD.Parameters.m_maxNumVerticesPerCH)][selectedLanguageId]);
            vhacd.m_parameters.m_maxNumVerticesPerCH = (uint)EditorGUILayout.IntField((int)vhacd.m_parameters.m_maxNumVerticesPerCH);

            GUILayout.Label(labelDict[nameof(VHACD.Parameters.m_asyncACD)][selectedLanguageId]);
            vhacd.m_parameters.m_asyncACD = EditorGUILayout.Toggle(vhacd.m_parameters.m_asyncACD);

            GUILayout.Label(labelDict[nameof(VHACD.Parameters.m_minEdgeLength)][selectedLanguageId]);
            vhacd.m_parameters.m_minEdgeLength = (uint)EditorGUILayout.IntSlider((int)vhacd.m_parameters.m_minEdgeLength, 1, 3);

            GUILayout.Label(labelDict[nameof(VHACD.Parameters.m_findBestPlane)][selectedLanguageId]);
            vhacd.m_parameters.m_findBestPlane = EditorGUILayout.Toggle(vhacd.m_parameters.m_findBestPlane);


            if (GUILayout.Button("Generate HullMesh"))
            {
                vhacdTools.GenerateConvexAndSave();
            }

        }
    }
}
#endif