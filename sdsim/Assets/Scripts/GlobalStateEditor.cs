using UnityEngine;

public class GlobalStateEditor : MonoBehaviour
{

    public Rect rect;
    private Vector2 scrollPosition = Vector2.zero;

    public int port
    {
        get { return GlobalState.port; }
        set { GlobalState.port = value; }
    }
    public int portPrivateAPI
    {
        get { return GlobalState.portPrivateAPI; }
        set { GlobalState.portPrivateAPI = value; }
    }
    public int fps
    {
        get { return GlobalState.fps; }
        set { GlobalState.fps = value; }
    }
    public int maxSplitScreen
    {
        get { return GlobalState.maxSplitScreen; }
        set { GlobalState.maxSplitScreen = value; }
    }
    public bool generateTrees
    {
        get { return GlobalState.generateTrees; }
        set { GlobalState.generateTrees = value; }
    }
    public bool extendedTelemetry
    {
        get { return GlobalState.extendedTelemetry; }
        set { GlobalState.extendedTelemetry = value; }
    }
    public bool generateRandomCones
    {
        get { return GlobalState.generateRandomCones; }
        set { GlobalState.generateRandomCones = value; }
    }
    public bool randomLight
    {
        get { return GlobalState.randomLight; }
        set { GlobalState.randomLight = value; }
    }
    public bool raceCameras
    {
        get { return GlobalState.raceCameras; }
        set { GlobalState.raceCameras = value; }
    }
    public bool drawLidar
    {
        get { return GlobalState.drawLidar; }
        set { GlobalState.drawLidar = value; }
    }
    public bool useSeed
    {
        get { return GlobalState.useSeed; }
        set { GlobalState.useSeed = value; }
    }
    public int seed
    {
        get { return GlobalState.seed; }
        set { GlobalState.seed = value; }
    }
    public string privateKey
    {
        get { return GlobalState.privateKey; }
        set { GlobalState.privateKey = value; }
    }

    public string additionnalContentPath
    {
        get { return GlobalState.additionnalContentPath; }
        set { GlobalState.additionnalContentPath = value; }
    }

    private bool showPrivateKey = false;
    void Awake()
    {
        LoadPlayerPrefs();
        SaveToPlayerPrefs();
    }

    void OnGUI()
    {
        int pixXOffset = (int)(rect.x * Screen.width);
        int pixYOffset = (int)(rect.y * Screen.height);
        int width = (int)(rect.width * Screen.width);
        int height = (int)(rect.height * Screen.height);

        int LabelXOffset = 100;

        int scrollHeight = 220;
        int scrollWidth = 200;

        int YOffset = 0;
        int Ysteps = 20;


        GUI.BeginGroup(new Rect(pixXOffset, pixYOffset, width, height));
        scrollPosition = GUI.BeginScrollView(new Rect(0, 0, width, height), scrollPosition, new Rect(0, 0, scrollWidth, scrollHeight), false, false);

        GUI.Label(new Rect(0, YOffset, LabelXOffset, 20), "port");
        string portString = GUI.TextField(new Rect(LabelXOffset, YOffset, width, 20), port.ToString());
        int tmp_port = port;
        int.TryParse(portString, out tmp_port);
        if (tmp_port != port)
            port = tmp_port;
        YOffset += Ysteps;

        GUI.Label(new Rect(0, YOffset, LabelXOffset, 20), "portPrivateAPI");
        string portPrivateAPIString = GUI.TextField(new Rect(LabelXOffset, YOffset, width, 20), portPrivateAPI.ToString());
        int tmp_portPrivateAPI = portPrivateAPI;
        int.TryParse(portPrivateAPIString, out tmp_portPrivateAPI);
        if (tmp_portPrivateAPI != portPrivateAPI)
            portPrivateAPI = tmp_portPrivateAPI;
        YOffset += Ysteps;

        GUI.Label(new Rect(0, YOffset, LabelXOffset, 20), "fps limit");
        string fpsString = GUI.TextField(new Rect(LabelXOffset, YOffset, width, 20), fps.ToString());
        int tmp_fps = fps;
        int.TryParse(fpsString, out tmp_fps);
        if (tmp_fps != fps)
            fps = tmp_fps;
        YOffset += Ysteps;


        GUI.Label(new Rect(0, YOffset, LabelXOffset, 20), "Max SplitScreen");
        string maxspString = GUI.TextField(new Rect(LabelXOffset, YOffset, width, 20), maxSplitScreen.ToString());
        int tmp_maxsp = maxSplitScreen;
        int.TryParse(maxspString, out tmp_maxsp);
        if (tmp_maxsp != maxSplitScreen)
            maxSplitScreen = tmp_maxsp;
        YOffset += Ysteps;

        generateTrees = GUI.Toggle(new Rect(0, YOffset, width, 20), generateTrees, "generateTrees");
        YOffset += Ysteps;
        extendedTelemetry = GUI.Toggle(new Rect(0, YOffset, width, 20), extendedTelemetry, "extendedTelemetry");
        YOffset += Ysteps;
        generateRandomCones = GUI.Toggle(new Rect(0, YOffset, width, 20), generateRandomCones, "generateRandomCones");
        YOffset += Ysteps;
        randomLight = GUI.Toggle(new Rect(0, YOffset, width, 20), randomLight, "randomLight");
        YOffset += Ysteps;
        raceCameras = GUI.Toggle(new Rect(0, YOffset, width, 20), raceCameras, "raceCameras");
        YOffset += Ysteps;
        drawLidar = GUI.Toggle(new Rect(0, YOffset, width, 20), drawLidar, "drawLidar");
        YOffset += Ysteps;

        useSeed = GUI.Toggle(new Rect(0, YOffset, width, 20), useSeed, "useSeed");
        YOffset += Ysteps;
        if (useSeed)
        {
            GUI.Label(new Rect(0, YOffset, LabelXOffset, 20), "Seed");
            string seedString = GUI.TextField(new Rect(LabelXOffset, YOffset, width, 20), seed.ToString());
            YOffset += Ysteps;
            int tmp_seed = seed;
            int.TryParse(seedString, out tmp_seed);
            if (tmp_seed != seed)
                seed = tmp_seed;
        }

        YOffset += Ysteps;
        bool doSave = GUI.Button(new Rect(0, YOffset, width, 20), "Save");
        YOffset += Ysteps;

        YOffset += Ysteps;
        showPrivateKey = GUI.Toggle(new Rect(0, YOffset, width, 20), showPrivateKey, "showPrivateKey");
        YOffset += Ysteps;
        if (showPrivateKey)
        {
            GUI.Label(new Rect(0, YOffset, LabelXOffset, 20), "Private API Key");
            privateKey = GUI.TextField(new Rect(LabelXOffset, YOffset, width, 20), privateKey);
            YOffset += Ysteps;

            bool doRandomize = GUI.Button(new Rect(0, YOffset, width, 20), "Randomize private key");
            YOffset += Ysteps;
            if (doRandomize) { RandomizePrivateKey(); }
        }

        if (doSave) { SaveToPlayerPrefs(); }

        GUI.EndScrollView();
        GUI.EndGroup();
    }

    void SaveToPlayerPrefs()
    {
        PlayerPrefs.SetInt("port", port);
        PlayerPrefs.SetInt("portPrivateAPI", portPrivateAPI);
        PlayerPrefs.SetInt("fps", fps);
        PlayerPrefs.SetInt("maxSplitScreen", maxSplitScreen);
        PlayerPrefs.SetInt("generateTrees", generateTrees ? 1 : 0);
        PlayerPrefs.SetInt("extendedTelemetry", extendedTelemetry ? 1 : 0);
        PlayerPrefs.SetInt("generateRandomCones", generateRandomCones ? 1 : 0);
        PlayerPrefs.SetInt("randomLight", randomLight ? 1 : 0);
        PlayerPrefs.SetInt("raceCameras", raceCameras ? 1 : 0);
        PlayerPrefs.SetInt("drawLidar", drawLidar ? 1 : 0);
        PlayerPrefs.SetInt("useSeed", useSeed ? 1 : 0);
        PlayerPrefs.SetString("privateKey", privateKey);

        PlayerPrefs.Save();
    }

    void LoadPlayerPrefs()
    {
        port = PlayerPrefs.GetInt("port", port);
        portPrivateAPI = PlayerPrefs.GetInt("portPrivateAPI", portPrivateAPI);
        fps = PlayerPrefs.GetInt("fps", fps);
        maxSplitScreen = PlayerPrefs.GetInt("maxSplitScreen", maxSplitScreen);
        generateTrees = PlayerPrefs.GetInt("generateTrees", generateTrees ? 1 : 0) == 1 ? true : false;
        extendedTelemetry = PlayerPrefs.GetInt("extendedTelemetry", extendedTelemetry ? 1 : 0) == 1 ? true : false;
        generateRandomCones = PlayerPrefs.GetInt("generateRandomCones", generateRandomCones ? 1 : 0) == 1 ? true : false;
        randomLight = PlayerPrefs.GetInt("randomLight", randomLight ? 1 : 0) == 1 ? true : false;
        raceCameras = PlayerPrefs.GetInt("raceCameras", raceCameras ? 1 : 0) == 1 ? true : false;
        drawLidar = PlayerPrefs.GetInt("drawLidar", drawLidar ? 1 : 0) == 1 ? true : false;
        useSeed = PlayerPrefs.GetInt("useSeed", useSeed ? 1 : 0) == 1 ? true : false;
        privateKey = PlayerPrefs.GetString("privateKey", Random.Range(10000000, 99999999).ToString());
        additionnalContentPath = Application.streamingAssetsPath;
    }

    void RandomizePrivateKey()
    {
        privateKey = Random.Range(10000000, 99999999).ToString();
        PlayerPrefs.SetString("privateKey", privateKey);
        PlayerPrefs.Save();
    }
}