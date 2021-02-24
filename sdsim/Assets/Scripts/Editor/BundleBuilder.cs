using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class BundleBuilder : Editor
{
    [MenuItem("Build/ Build All AssetsBundles")]
    static void BuildAllAssetsBundles()
    {
        BuildTarget[] platforms = new BuildTarget[] { BuildTarget.StandaloneWindows64, BuildTarget.StandaloneLinux64, BuildTarget.StandaloneOSX };
        foreach (BuildTarget platform in platforms)
        {
            BuildAssetBundles(platform);
        }
    }

    [MenuItem("Build/ Build Active Target AssetsBundles")]
    static void BuildCurrentTargetAssetsBundles()
    {
        BuildTarget platform = EditorUserBuildSettings.activeBuildTarget;
        BuildAssetBundles(platform);
    }


    static void BuildAssetBundles(BuildTarget platform)
    {
        string path = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "Assets/AssetBundles/" + platform.ToString());

        if (System.IO.Directory.Exists(path) == false)
        {
            System.IO.Directory.CreateDirectory(path);
            Debug.LogFormat("creating directory {0}", path);
        }

        BuildPipeline.BuildAssetBundles(path, BuildAssetBundleOptions.ChunkBasedCompression, platform);
    }
}
