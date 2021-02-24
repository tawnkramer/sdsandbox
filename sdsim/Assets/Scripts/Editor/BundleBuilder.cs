using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class BundleBuilder : Editor
{
    [MenuItem("Assets/ Build AssetsBundles")]
    static void BuildAllAssetsBundles()
    {
        BuildTarget platform = EditorUserBuildSettings.activeBuildTarget;
        string path = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "Assets/AssetBundles/" + platform.ToString());

        if (System.IO.Directory.Exists(path) == false)
        {
            System.IO.Directory.CreateDirectory(path);
            Debug.LogFormat("creating directory {0}", path);
        }

        BuildPipeline.BuildAssetBundles(path, BuildAssetBundleOptions.ChunkBasedCompression, platform);
    }
}
