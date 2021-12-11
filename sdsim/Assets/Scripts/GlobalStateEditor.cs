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
    public bool overheadCamera
    {
        get { return GlobalState.overheadCamera; }
        set { GlobalState.overheadCamera = value; }
    }
    public bool drawLidar
    {
        get { return GlobalState.drawLidar; }
        set { GlobalState.drawLidar = value; }
    }
    public bool paceCar
    {
        get { return GlobalState.paceCar; }
        set { GlobalState.paceCar = value; }
    }
    public bool manualDriving
    {
        get { return GlobalState.manualDriving; }
        set { GlobalState.manualDriving = value; }
    }
    public float kp
    {
        get { return GlobalState.kp; }
        set { GlobalState.kp = value; }
    }
    public float kd
    {
        get { return GlobalState.kd; }
        set { GlobalState.kd = value; }
    }
    public float ki
    {
        get { return GlobalState.ki; }
        set { GlobalState.ki = value; }
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
    public float timeScale
    {
        get { return GlobalState.timeScale; }
        set { GlobalState.timeScale = value; Time.timeScale = value; }
    }
    public float timeOut
    {
        get { return GlobalState.timeOut; }
        set { GlobalState.timeOut = value; }
    }

    private bool showPrivateKey = false;
    private VersionCheck versionCheck;
    void Awake()
    {
        LoadPlayerPrefs();
        SaveToPlayerPrefs();

        versionCheck = gameObject.GetComponent<VersionCheck>();
        
        //keep it processing even when not in focus.
        Application.runInBackground = true;

        //Set desired frame rate as high as possible.
        Application.targetFrameRate = GlobalState.fps;
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
        string portString = GUI.TextField(new Rect(LabelXOffset, YOffset, width-LabelXOffset, 20), port.ToString());
        int tmp_port = port;
        int.TryParse(portString, out tmp_port);
        if (tmp_port != port)
            port = tmp_port;
        YOffset += Ysteps;

        GUI.Label(new Rect(0, YOffset, LabelXOffset, 20), "portPrivateAPI");
        string portPrivateAPIString = GUI.TextField(new Rect(LabelXOffset, YOffset, width-LabelXOffset, 20), portPrivateAPI.ToString());
        int tmp_portPrivateAPI = portPrivateAPI;
        int.TryParse(portPrivateAPIString, out tmp_portPrivateAPI);
        if (tmp_portPrivateAPI != portPrivateAPI)
            portPrivateAPI = tmp_portPrivateAPI;
        YOffset += Ysteps;

        GUI.Label(new Rect(0, YOffset, LabelXOffset, 20), "FPS limit");
        string fpsString = GUI.TextField(new Rect(LabelXOffset, YOffset, width-LabelXOffset, 20), fps.ToString());
        int tmp_fps = fps;
        int.TryParse(fpsString, out tmp_fps);
        if (tmp_fps != fps)
            fps = tmp_fps;
        YOffset += Ysteps;

        GUI.Label(new Rect(0, YOffset, LabelXOffset, 20), "Time scale");
        string timeScaleString = GUI.TextField(new Rect(LabelXOffset, YOffset, width-LabelXOffset, 20), timeScale.ToString());
        float tmp_timeScale = timeScale;
        float.TryParse(timeScaleString, out tmp_timeScale);
        if (tmp_timeScale != timeScale)
            timeScale = tmp_timeScale;
        YOffset += Ysteps;

        GUI.Label(new Rect(0, YOffset, LabelXOffset, 20), "Time out");
        string timeOutString = GUI.TextField(new Rect(LabelXOffset, YOffset, width-LabelXOffset, 20), timeOut.ToString());
        float tmp_timeOut = timeOut;
        float.TryParse(timeOutString, out tmp_timeOut);
        if (tmp_timeOut != timeOut)
            timeOut = tmp_timeOut;
        YOffset += Ysteps;

        GUI.Label(new Rect(0, YOffset, LabelXOffset, 20), "Max SplitScreen");
        string maxspString = GUI.TextField(new Rect(LabelXOffset, YOffset, width-LabelXOffset, 20), maxSplitScreen.ToString());
        int tmp_maxsp = maxSplitScreen;
        int.TryParse(maxspString, out tmp_maxsp);
        if (tmp_maxsp != maxSplitScreen)
            maxSplitScreen = tmp_maxsp;
        YOffset += Ysteps;

        extendedTelemetry = GUI.Toggle(new Rect(0, YOffset, width, 20), extendedTelemetry, "extendedTelemetry");
        YOffset += Ysteps;
        generateTrees = GUI.Toggle(new Rect(0, YOffset, width, 20), generateTrees, "generateTrees");
        YOffset += Ysteps;
        generateRandomCones = GUI.Toggle(new Rect(0, YOffset, width, 20), generateRandomCones, "generateRandomCones");
        YOffset += Ysteps;
        randomLight = GUI.Toggle(new Rect(0, YOffset, width, 20), randomLight, "randomLight");
        YOffset += Ysteps;
        raceCameras = GUI.Toggle(new Rect(0, YOffset, width, 20), raceCameras, "raceCameras");
        YOffset += Ysteps;
        overheadCamera = GUI.Toggle(new Rect(0, YOffset, width, 20), overheadCamera, "overheadCamera");
        YOffset += Ysteps;
        drawLidar = GUI.Toggle(new Rect(0, YOffset, width, 20), drawLidar, "drawLidar");
        YOffset += Ysteps;
        paceCar = GUI.Toggle(new Rect(0, YOffset, width, 20), paceCar, "paceCar");
        YOffset += Ysteps;
        if (paceCar)
        {
            manualDriving = GUI.Toggle(new Rect(Ysteps, YOffset, width, 20), manualDriving, "manualDriving");
            YOffset += Ysteps;

            if (!manualDriving)
            {
                GUI.Label(new Rect(Ysteps, YOffset, LabelXOffset, 20), "kp");
                string kpString = GUI.TextField(new Rect(LabelXOffset+Ysteps, YOffset, width-LabelXOffset-Ysteps, 20), kp.ToString());
                float tmp_kp = kp;
                float.TryParse(kpString, out tmp_kp);
                if (tmp_kp != kp)
                    kp = tmp_kp;
                YOffset += Ysteps;

                GUI.Label(new Rect(Ysteps, YOffset, LabelXOffset, 20), "kd");
                string kdString = GUI.TextField(new Rect(LabelXOffset+Ysteps, YOffset, width-LabelXOffset-Ysteps, 20), kd.ToString());
                float tmp_kd = kd;
                float.TryParse(kdString, out tmp_kd);
                if (tmp_kd != kd)
                    kd = tmp_kd;
                YOffset += Ysteps;

                GUI.Label(new Rect(Ysteps, YOffset, LabelXOffset, 20), "ki");
                string kiString = GUI.TextField(new Rect(LabelXOffset+Ysteps, YOffset, width-LabelXOffset-Ysteps, 20), ki.ToString());
                float tmp_ki = ki;
                float.TryParse(kiString, out tmp_ki);
                if (tmp_ki != ki)
                    ki = tmp_ki;
                YOffset += Ysteps;
            }
        }

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

        // Check if the version used is the latest version if not, notify the user !
        if (versionCheck.latest != GlobalState.version)
        {
            YOffset += Ysteps;
            bool getLatest = GUI.Button(new Rect(0, YOffset, width, Ysteps * 2), "A new version is available, \n click here to get latest version !");
            YOffset += Ysteps * 2;
            if (getLatest) { versionCheck.GetLatestVersion(); }
        }

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
        PlayerPrefs.SetFloat("timeScale", timeScale);
        PlayerPrefs.SetFloat("timeOut", timeOut);
        PlayerPrefs.SetInt("maxSplitScreen", maxSplitScreen);
        PlayerPrefs.SetInt("generateTrees", generateTrees ? 1 : 0);
        PlayerPrefs.SetInt("extendedTelemetry", extendedTelemetry ? 1 : 0);
        PlayerPrefs.SetInt("generateRandomCones", generateRandomCones ? 1 : 0);
        PlayerPrefs.SetInt("randomLight", randomLight ? 1 : 0);
        PlayerPrefs.SetInt("raceCameras", raceCameras ? 1 : 0);
        PlayerPrefs.SetInt("overheadCamera", overheadCamera ? 1 : 0);
        PlayerPrefs.SetInt("drawLidar", drawLidar ? 1 : 0);
        PlayerPrefs.SetInt("paceCar", paceCar ? 1 : 0);
        PlayerPrefs.SetInt("manualDriving", manualDriving ? 1 : 0);
        PlayerPrefs.SetFloat("kp", kp);
        PlayerPrefs.SetFloat("kd", kd);
        PlayerPrefs.SetFloat("ki", ki);
        PlayerPrefs.SetInt("useSeed", useSeed ? 1 : 0);
        PlayerPrefs.SetString("privateKey", privateKey);

        PlayerPrefs.Save();
    }

    void LoadPlayerPrefs()
    {
        port = PlayerPrefs.GetInt("port", port);
        portPrivateAPI = PlayerPrefs.GetInt("portPrivateAPI", portPrivateAPI);
        fps = PlayerPrefs.GetInt("fps", fps);
        timeScale = PlayerPrefs.GetFloat("timeScale", timeScale);
        timeOut = PlayerPrefs.GetFloat("timeOut", timeOut);
        maxSplitScreen = PlayerPrefs.GetInt("maxSplitScreen", maxSplitScreen);
        generateTrees = PlayerPrefs.GetInt("generateTrees", generateTrees ? 1 : 0) == 1 ? true : false;
        extendedTelemetry = PlayerPrefs.GetInt("extendedTelemetry", extendedTelemetry ? 1 : 0) == 1 ? true : false;
        generateRandomCones = PlayerPrefs.GetInt("generateRandomCones", generateRandomCones ? 1 : 0) == 1 ? true : false;
        randomLight = PlayerPrefs.GetInt("randomLight", randomLight ? 1 : 0) == 1 ? true : false;
        raceCameras = PlayerPrefs.GetInt("raceCameras", raceCameras ? 1 : 0) == 1 ? true : false;
        overheadCamera = PlayerPrefs.GetInt("overheadCamera", overheadCamera ? 1 : 0) == 1 ? true : false;
        drawLidar = PlayerPrefs.GetInt("drawLidar", drawLidar ? 1 : 0) == 1 ? true : false;
        paceCar = PlayerPrefs.GetInt("paceCar", paceCar ? 1 : 0) == 1 ? true : false;
        manualDriving = PlayerPrefs.GetInt("manualDriving", manualDriving ? 1 : 0) == 1 ? true : false;
        kp = PlayerPrefs.GetFloat("kp", kp);
        kd = PlayerPrefs.GetFloat("kd", kd);
        ki = PlayerPrefs.GetFloat("ki", ki);
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