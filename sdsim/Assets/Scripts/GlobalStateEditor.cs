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
    public bool bCreateCarWithoutNetworkClient
    {
        get { return GlobalState.bCreateCarWithoutNetworkClient; }
        set { GlobalState.bCreateCarWithoutNetworkClient = value; GlobalState.bAutoHideSceneMenu = !value; }
    }
    public bool generateTrees
    {
        get { return GlobalState.generateTrees; }
        set { GlobalState.generateTrees = value; }
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
    public string privateKey
    {
        get { return GlobalState.privateKey; }
        set { GlobalState.privateKey = value; }
    }

    private bool doShowPrivateKey = false;
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


        GUI.BeginGroup(new Rect(pixXOffset, pixYOffset, width, height));
        scrollPosition = GUI.BeginScrollView(new Rect(0, 0, width, height), scrollPosition, new Rect(0, 0, scrollWidth, scrollHeight), false, false);

        GUI.Label(new Rect(0, 0, LabelXOffset, 20), "port");
        string portString = GUI.TextField(new Rect(LabelXOffset, 0, width, 20), port.ToString());
        int tmp_port = port;
        int.TryParse(portString, out tmp_port);
        if (tmp_port != port)
            port = tmp_port;

        GUI.Label(new Rect(0, 20, LabelXOffset, 20), "fps limit");
        string fpsString = GUI.TextField(new Rect(LabelXOffset, 20, width, 20), fps.ToString());
        int tmp_fps = fps;
        int.TryParse(fpsString, out tmp_fps);
        if (tmp_fps != fps)
            fps = tmp_fps;


        GUI.Label(new Rect(0, 40, LabelXOffset, 20), "Max SplitScreen");
        string maxspString = GUI.TextField(new Rect(LabelXOffset, 40, width, 20), maxSplitScreen.ToString());
        int tmp_maxsp = maxSplitScreen;
        int.TryParse(maxspString, out tmp_maxsp);
        if (tmp_maxsp != maxSplitScreen)
            maxSplitScreen = tmp_maxsp;

        bCreateCarWithoutNetworkClient = GUI.Toggle(new Rect(0, 60, width, 20), bCreateCarWithoutNetworkClient, "bCreateCarWithoutNetworkClient");
        generateTrees = GUI.Toggle(new Rect(0, 80, width, 20), generateTrees, "generateTrees");
        generateRandomCones = GUI.Toggle(new Rect(0, 100, width, 20), generateRandomCones, "generateRandomCones");
        randomLight = GUI.Toggle(new Rect(0, 120, width, 20), randomLight, "randomLight");

        bool doSave = GUI.Button(new Rect(0, 140, width, 20), "Save");
        if (doSave) { SaveToPlayerPrefs(); }


        doShowPrivateKey = GUI.Toggle(new Rect(0, 180, width, 20), doShowPrivateKey, "doShowPrivateKey");
        if (doShowPrivateKey)
        {
            GUI.Label(new Rect(0, 200, LabelXOffset + width, 20), string.Format("Private API Key: {0}", privateKey));
            bool doRandomize = GUI.Button(new Rect(0, 220, width, 20), "Randomize private key");
            if (doRandomize) { RandomizePrivateKey(); }
        }

        GUI.EndScrollView();
        GUI.EndGroup();
    }

    void SaveToPlayerPrefs()
    {
        PlayerPrefs.SetInt("port", port);
        PlayerPrefs.SetInt("fps", fps);
        PlayerPrefs.SetInt("maxSplitScreen", maxSplitScreen);
        PlayerPrefs.SetInt("bCreateCarWithoutNetworkClient", bCreateCarWithoutNetworkClient ? 1 : 0);
        PlayerPrefs.SetInt("generate Trees", generateTrees ? 1 : 0);
        PlayerPrefs.SetInt("generate Random Cones", generateRandomCones ? 1 : 0);
        PlayerPrefs.SetInt("randomLight", randomLight ? 1 : 0);
        PlayerPrefs.SetString("privateKey", privateKey);

        PlayerPrefs.Save();
    }

    void LoadPlayerPrefs()
    {
        port = PlayerPrefs.GetInt("port", port);
        fps = PlayerPrefs.GetInt("fps", fps);
        maxSplitScreen = PlayerPrefs.GetInt("maxSplitScreen", maxSplitScreen);
        bCreateCarWithoutNetworkClient = PlayerPrefs.GetInt("bCreateCarWithoutNetworkClient", 1) == 1 ? true : false;
        generateTrees = PlayerPrefs.GetInt("generateTrees", 1) == 1 ? true : false;
        generateRandomCones = PlayerPrefs.GetInt("generateRandomCones", 0) == 1 ? true : false;
        randomLight = PlayerPrefs.GetInt("randomLight", 1) == 1 ? true : false;
        privateKey = PlayerPrefs.GetString("privateKey", Random.Range(10000000, 99999999).ToString());
    }

    void RandomizePrivateKey()
    {
        privateKey = Random.Range(10000000, 99999999).ToString();
        PlayerPrefs.SetString("privateKey", privateKey);
        PlayerPrefs.Save();
    }
}