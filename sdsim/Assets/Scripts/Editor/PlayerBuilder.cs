using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Reporting;

public class PlayerBuilder : Editor
{

    [MenuItem("Build/ Build Every Player")]
    static void Build()
    {
        WinBuild();
        MacBuild();
        LinuxBuild();
    }

    [MenuItem("Build/ Build Windows Player")]
    static void WinBuild()
    {
        EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;

        BuildReport report = BuildPipeline.BuildPlayer(scenes, "Builds/DonkeySimWin/donkey_sim.exe", BuildTarget.StandaloneWindows64, BuildOptions.None);
        BuildSummary summary = report.summary;


        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log("Build succeeded: " + summary.totalSize + " bytes");

            string assetBundlePath = "Assets/AssetBundles/" + BuildTarget.StandaloneWindows64.ToString();
            string destPath = "Builds/DonkeySimWin/donkey_sim_Data/StreamingAssets/";
            CopyDirectory(assetBundlePath, destPath);
        }

        if (summary.result == BuildResult.Failed)
        {
            Debug.Log("Build failed");
        }
    }
    [MenuItem("Build/ Build MacOS Player")]
    static void MacBuild()
    {
        EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;

        BuildReport report = BuildPipeline.BuildPlayer(scenes, "Builds/DonkeySimMac/donkey_sim.app", BuildTarget.StandaloneOSX, BuildOptions.None);
        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log("Build succeeded: " + summary.totalSize + " bytes");

            string assetBundlePath = "Assets/AssetBundles/" + BuildTarget.StandaloneOSX.ToString();
            string destPath = "Builds/DonkeySimMac/donkey_sim.app/Contents/Resources/Data/StreamingAssets";
            CopyDirectory(assetBundlePath, destPath);
        }

        if (summary.result == BuildResult.Failed)
        {
            Debug.Log("Build failed");
        }
    }
    [MenuItem("Build/ Build Linux Player")]
    static void LinuxBuild()
    {
        EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;

        BuildReport report = BuildPipeline.BuildPlayer(scenes, "Builds/DonkeySimLinux/donkey_sim.x86_64", BuildTarget.StandaloneLinux64, BuildOptions.None);
        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log("Build succeeded: " + summary.totalSize + " bytes");

            string assetBundlePath = "Assets/AssetBundles/" + BuildTarget.StandaloneLinux64.ToString();
            string destPath = "Builds/DonkeySimLinux/donkey_sim_Data/StreamingAssets";
            CopyDirectory(assetBundlePath, destPath);
        }

        if (summary.result == BuildResult.Failed)
        {
            Debug.Log("Build failed");
        }
    }


    static void CopyDirectory(string SourcePath, string DestinationPath)
    {

        if (!System.IO.Directory.Exists(DestinationPath)) { System.IO.Directory.CreateDirectory(DestinationPath); }

        foreach (string dirPath in System.IO.Directory.GetDirectories(SourcePath, "*",
            System.IO.SearchOption.AllDirectories))
            System.IO.Directory.CreateDirectory(dirPath.Replace(SourcePath, DestinationPath));

        foreach (string newPath in System.IO.Directory.GetFiles(SourcePath, "*.*",
            System.IO.SearchOption.AllDirectories))
            System.IO.File.Copy(newPath, newPath.Replace(SourcePath, DestinationPath), true);
    }
}
