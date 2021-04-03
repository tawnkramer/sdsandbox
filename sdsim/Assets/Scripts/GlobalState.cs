using System.Collections.Generic;
using UnityEngine;


public static class GlobalState
{
    public static string host = "0.0.0.0";
    public static int port = 9091;
    public static int portPrivateAPI = 9092;
    public static int fps = 60;
    public static int maxSplitScreen = 4;
    public static bool bAutoHideSceneMenu = false;

    // should we create a car even though we don't have a network client?
    public static bool bCreateCarWithoutNetworkClient = true;
    public static string log_path = "default";
    public static bool extendedTelemetry = true;
    public static bool generateTrees = true;
    public static bool generateRandomCones = true;
    public static bool randomLight = true;
    public static bool raceCameras = false;
    public static string privateKey = "";
    public static bool useSeed = false;
    public static int seed = 20432814;
    public static string additionnalContentPath = "";
    public static string[] sceneNames;
    public static List<AssetBundle> bundleScenes = new List<AssetBundle>();
    public static bool drawLidar = true;
}
