
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class UnusedAssetsFinder : EditorWindow
{
    [MenuItem("Tools/Find Unused Materials and Textures")]
    public static void FindUnusedAssets()
    {
        string[] allMaterials = Directory.GetFiles("Assets", "*.mat", SearchOption.AllDirectories);
        string[] allTextures = Directory.GetFiles("Assets", "*.png", SearchOption.AllDirectories); // thêm *.jpg nếu cần

        List<string> unusedAssets = new List<string>();

        foreach (var assetPath in allMaterials)
        {
            string relativePath = assetPath.Replace("\\", "/");
            string[] dependencies = AssetDatabase.GetDependencies(relativePath, true);
            bool isUsed = false;

            foreach (var scene in AssetDatabase.FindAssets("t:Scene"))
            {
                string scenePath = AssetDatabase.GUIDToAssetPath(scene);
                string[] sceneDeps = AssetDatabase.GetDependencies(scenePath, true);
                if (System.Array.Exists(sceneDeps, d => d == relativePath))
                {
                    isUsed = true;
                    break;
                }
            }

            if (!isUsed)
                unusedAssets.Add(relativePath);
        }

        foreach (var texPath in allTextures)
        {
            string relativePath = texPath.Replace("\\", "/");
            string[] allDeps = AssetDatabase.GetAllAssetPaths();

            bool isUsed = false;
            foreach (var path in allDeps)
            {
                string[] deps = AssetDatabase.GetDependencies(path, true);
                if (System.Array.Exists(deps, d => d == relativePath))
                {
                    isUsed = true;
                    break;
                }
            }

            if (!isUsed)
                unusedAssets.Add(relativePath);
        }

        Debug.Log($"Found {unusedAssets.Count} unused assets:");
        foreach (var u in unusedAssets)
        {
            Debug.Log(u);
        }
    }
}
