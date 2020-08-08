using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.IO;
using tk;
using System.Xml.Serialization;

[Serializable]
public class Competitor
{
    public Competitor()
    {
    }

    public Competitor(JsonTcpClient _client, RaceManager _raceMan)
    {
        raceMan = _raceMan;
        has_car = false;
        SetClient(_client);
        client.dispatcher.Register("racer_info", new tk.Delegates.OnMsgRecv(OnRacerInfo));
        client.dispatcher.Register("car_config", new tk.Delegates.OnMsgRecv(OnCarConfig));
        client.dispatcher.Register("cam_config", new tk.Delegates.OnMsgRecv(OnCamConfig));
        client.dispatcher.Register("connected", new tk.Delegates.OnMsgRecv(OnConnected));

        UnityMainThreadDispatcher.Instance().Enqueue(SendNeedCarConfig());
    }

    public void SetClient(JsonTcpClient _client)
    {
        client = _client;
    }

    IEnumerator SendNeedCarConfig()
    {
        JSONObject json = new JSONObject(JSONObject.Type.OBJECT);
        json.AddField("msg_type", "need_car_config");
        if(client != null)
            client.SendMsg(json);

        yield return null;
    }

    public bool IsOnline() { return client != null; }

    [XmlIgnoreAttribute]
    public RaceManager raceMan;

    [XmlIgnoreAttribute]
    public JsonTcpClient client;

    public string car_name;
    public string racer_name;
    public string country;
    public string info;
    public bool has_car;
    public bool got_qual_attempt = false;
    public bool dropped = false;

    public JSONObject carConfig;
    public JSONObject camConfig;
    public JSONObject racerBio;

    public int qual_place = 0;
    public int stage1_place = 0;
    public int stage2_place = 0;
    public float qual_time;
    public float best_stage1_time;
    public float best_stage2_time;
    public List<float> stage1_lap_times;
    public List<float> stage2_lap_times;


    public void OnRacerInfo(JSONObject json)
    {
        racerBio = json;

        car_name = json.GetField("car_name").str;
        racer_name = json.GetField("racer_name").str;
        country = json.GetField("country").str;
        info = json.GetField("bio").str;

        Debug.Log("Got racer info for " + racer_name);


        raceMan.OnRacerInfo(this);
    }

    public void OnCarConfig(JSONObject json)
    {
        if(racer_name != null)
            Debug.Log("Got car config for " + racer_name);

        carConfig = json;
    }

    public void OnCamConfig(JSONObject json)
    {
        if (racer_name != null)
            Debug.Log("Got cam config for " + racer_name);
        camConfig = json;
    }

    public void OnDQ(bool missedCheckpoint)
    {
        UnityMainThreadDispatcher.Instance().Enqueue(SendDQMsg(missedCheckpoint));
    }

    IEnumerator SendDQMsg(bool missedCheckpoint)
    {
        JSONObject json = new JSONObject(JSONObject.Type.OBJECT);
        json.AddField("msg_type", "DQ");
        json.AddField("missed_cp", missedCheckpoint);

        if (client != null)
            client.SendMsg(json);

        yield return null;
    }

    public void OnConnected(JSONObject json)
    {
        
    }
}

[Serializable]
public class Pairing
{
    public string name1;
    public string name2;
    public float time1;
    public float time2;

    public int GetNumRacers()
    {
        int n = 0;

        if (name1 != "drop")
            n++;

        if (name2 != "solo" && name2 != "drop")
            n++;

        return n;
    }
}

[Serializable]
public class RaceState
{
    public enum RaceStage
    {
        None,           // init
        Practice,       // 1 HR or more free runs, multiple competitors come and go.
        Qualifying,     // For 30 min prior. All competitors must finish a lap.
        PreEvent,       // Roll up to twitch feed begin.
        EventIntro,     // Twitch feed begins. Race intro.
        CompetitorIntro, // Competitor list shown
        Stage1PreRace,  // Competitors called to the line. Info screens shown.
        Stage1Race,     // Laps completed or DQ
        Stage1PostRace, // Finish times shown
        Stage1Completed, // All stage 1 finished. Ladder seeded with 8 competitors.
        Stage2PreRace,  // Show ladder, competitors called to the line.
        Stage2Race,     // Competition
        Stage2PostRace, // Results shown
        Stage2Complete, // Final top 3 competitors shown.
    }

    public RaceState()
    {
        m_State = RaceState.RaceStage.None;
        m_QueuedState = RaceState.RaceStage.None;
        m_PracticeTime = 100 * 60.0f * 30.0f; // 30 minutes
        m_QualTime = 60.0f * 30.0f; //30 minutes
        m_IntroTime = 60.0f * 5.0f;
        m_TimeLimitQual = 100.0f;
        m_TimeDelay = 0.0f;
        m_TimeToShowRaceSummary = 60.0f;
        m_BetweenStageTime = 10.0f;
        m_BetweenStageTwoTime = 20.0f;
        m_TimeLimitRace = 120.0f;
        m_CheckPointDelay = 20.0f;
        m_RacerBioDisplayTime = 15.0f;
        m_MinCompetitorCount = 3;
        m_RaceRestartsLimit = 3;
        m_RaceRestarts = 0;
        m_AnyCompetitorFinishALap = false;
        m_CurrentQualifier = "None";
        m_iQual = 0;
        m_Stage1Next = 0;
        m_Stage2Next = 0;

        m_Competitors = new List<Competitor>();
        m_Stage1Order = new List<Pairing>();
        m_Stage2a_4pairs = new List<Pairing>();
        m_Stage2b_2pairs = new List<Pairing>();
        m_Stage2c_final = new List<Pairing>();
    }

    public RaceStage   m_State;
    public RaceStage   m_QueuedState;
    public float m_TimeInState;
    public float m_TimeDelay;
    public string m_CurrentQualifier;
    public int m_iQual;
    public float m_CurrentQualElapsed;
    public int m_Stage1Next;  // index of next competitors
    public int m_Stage2Next;  // index of next pairing
    public int m_RaceRestarts;  // number of restarts.
    public int m_RaceRestartsLimit;  // max number of restarts.
    public bool m_AnyCompetitorFinishALap; // need to keep this because timers destroyed and this flag helps keep result in double DQ case.


    //constants
    public float m_CheckPointDelay;
    public float m_PracticeTime;
    public float m_QualTime;
    public float m_IntroTime;
    public float m_TimeLimitQual;
    public float m_BetweenStageTime;
    public float m_BetweenStageTwoTime;
    public float m_TimeLimitRace;
    public int m_MinCompetitorCount;
    public float m_RacerBioDisplayTime;
    public float m_TimeToShowRaceSummary;

    // lists of pairings
    public List<Pairing> m_Stage1Order;
    public List<Pairing> m_Stage2a_4pairs;
    public List<Pairing> m_Stage2b_2pairs;
    public List<Pairing> m_Stage2c_final;

    // competitors
    public List<Competitor> m_Competitors;


    public void Write(string path)
    {
        System.Xml.Serialization.XmlSerializer writer =
            new System.Xml.Serialization.XmlSerializer(typeof(RaceState));

        System.IO.FileStream file = System.IO.File.Create(path);

        writer.Serialize(file, this);
        file.Close();
    }

    public static RaceState Read(string path)
    {
        System.Xml.Serialization.XmlSerializer reader =
            new System.Xml.Serialization.XmlSerializer(typeof(RaceState));

        System.IO.StreamReader file = new System.IO.StreamReader(path);

        RaceState s = (RaceState)reader.Deserialize(file);
        file.Close();

        return s;
    }
}

public class RaceManager : MonoBehaviour
{
    List<GameObject> cars = new List<GameObject>();
    public GameObject raceBanner;
    public TMP_Text raceBannerText;
    public GameObject[] objToDisable;
    public CameraSwitcher[] camSwitchers;
    public GameObject raceStatusPrefab;
    public GameObject raceCompetitorPrefab;
    public RectTransform raceStatusPanel;
    public RectTransform raceCompetitorPanel;
    public RaceInfoBar raceInfoBar;
    public RaceSummary raceSummary;
    public RaceCamSwitcher raceCamSwitcher;
    public GameObject raceIntroGroup;
    public GameObject raceIntroPanel;
    public RaceLineupIntroPanel lineupIntroPanel;
    public RaceLadderUI racerLadder;

    public RaceCheckPoint[] checkPoints;

    public RacerBioPanel racerBioPanel;
    public DropCompetitorUI dropCompetitorPanel;

    public GameObject raceControls;

    public bool bRaceActive = false;
    public bool bDevmode = false;
    RaceState raceState;

    public int raceStatusHeight = 100;
    int raceCompetitorHeight = 50;
    bool paused = false;
    public Text pauseButtonText;

    string removeRacerName = "";

    public int race_num_laps = 2;
    public static float dq_time = 1000000.0f;
    public string race_state_filename = "default";

    List<Competitor> m_TempCompetitors = new List<Competitor>();

    void Start()
    {
        // specify --race path/to/race_state.xml to load a previous race state.
        string[] args = System.Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "--race")
            {
                race_state_filename = args[i + 1];
            }
        }

        raceState = new RaceState();

        try
        {
            if(race_state_filename != "default")
            {
                LoadRaceState();
                OnStateChange();
                foreach (Competitor c in raceState.m_Competitors)
                    AddCompetitorDisplay(c);
            }
            else
            {
                SaveRaceState();
            }
        }
        catch
        {
            Debug.LogError("Failed to load " + race_state_filename);
        }

        if (bDevmode)
            StartDevMode();        
    }

    string GetLogPath()
    {
        if (GlobalState.log_path != "default")
            return GlobalState.log_path + "/";

        string path = Application.dataPath + "/../log/";

        // Make an attempt to create log if we can.
        try
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }
        catch
        {
            // Well. I tried.
        }

        return path;
    }

    void SaveRaceState()
    {
        string filepath = race_state_filename;

        if (filepath == "default")
            filepath = GetLogPath() + "race_state.xml";

        // auto insert the current state and a time val to
        // help role the raceback to any state easilly.
        // first strip off the .xml
        string p = filepath.Substring(0, filepath.Length - 4);
        p += "_";
        p += raceState.m_State.ToString();
        p += "_";
        int t = (int)Time.realtimeSinceStartup;
        p += t.ToString();
        p += ".xml";
        
        Debug.Log("saving race to: " + p);
        
        raceState.Write(p);
    }

    void LoadRaceState()
    {
        string filepath = race_state_filename;
        
        if(filepath == "default")
            filepath = GetLogPath() + "race_state.xml";

        raceState = RaceState.Read(filepath);

        // if saved during a race, switch to pre-race state.
        if (raceState.m_State == RaceState.RaceStage.Stage2Race)
            raceState.m_State = RaceState.RaceStage.Stage2PreRace;
        else if (raceState.m_State == RaceState.RaceStage.Stage1Race)
            raceState.m_State = RaceState.RaceStage.Stage1PreRace;

        // All competitors don't have a car. So reset that var.
        foreach(Competitor c in raceState.m_Competitors)
        {
            c.has_car = false;
        }

        // Once we've read this state, update the ladder UI with previous results.
        if (raceState.m_Stage1Order.Count > 0)
        {
            lineupIntroPanel.Init(raceState.m_Stage1Order);
            SetStageOneResults(raceState.m_Stage1Order);
        }

        if(raceState.m_Stage2a_4pairs.Count > 0)
        {
            racerLadder.Init(raceState.m_Stage2a_4pairs, 0);
            SetLadderPairingResults(raceState.m_Stage2a_4pairs, 0);
        }

        if(raceState.m_Stage2b_2pairs.Count > 0)
        {
            racerLadder.Init(raceState.m_Stage2b_2pairs, 1);
            SetLadderPairingResults(raceState.m_Stage2b_2pairs, 1);
        }

        if(raceState.m_Stage2c_final.Count > 0)
        {
            racerLadder.Init(raceState.m_Stage2c_final, 2);
            SetLadderPairingResults(raceState.m_Stage2c_final, 2);
        }

        SetStatus("Loaded race state from file.");
    }

    void SetStageOneResults(List<Pairing> pairs)
    {
        foreach(Pairing p in pairs)
            if(p.time1 > 0.0)
                lineupIntroPanel.SetResult(p);
    }    

    void SetLadderPairingResults(List<Pairing> pairs, int stage)
    {
        foreach(Pairing p in pairs)
            if(p.time1 > 0.0)
                racerLadder.SetResult(p, stage);
    }    

    public void OnPausePressed()
    {
        paused = !paused;

        if (pauseButtonText == null)
            return;

        if (paused)
        {
            pauseButtonText.text = "~";
            Time.timeScale = 0.0f;
        }
        else
        {
            pauseButtonText.text = "II";
            Time.timeScale = 1.0f;
        }
    }    

    public void OnRewindPressed()
    {
        if (raceState.m_State == 0)
            return;

        raceState.m_QueuedState = raceState.m_State - 1;
        HidePopUps();
    }

    public void OnFastForwardPressed()
    {
        if (raceState.m_State == RaceState.RaceStage.Stage2Complete)
            return;

        if (raceState.m_State == RaceState.RaceStage.Stage2PostRace)
        {
            OnLeaveStage2PostRace(true);
        }
        else if (raceState.m_State == RaceState.RaceStage.Stage1PostRace)
        {
            OnLeaveStage1PostRace(true);
        }
        else if (raceState.m_State == RaceState.RaceStage.Qualifying)
        {
            // Give all competitors random qual time.
            foreach(Competitor c in raceState.m_Competitors)
            {
                if (c.qual_time == 0.0f)
                    c.qual_time = UnityEngine.Random.Range(30.0f, 35.0f);
            }

            raceState.m_QueuedState = raceState.m_State + 1;
        }
        else
        {
            raceState.m_QueuedState = raceState.m_State + 1;
        }
        

        HidePopUps();
    }

    public void ToggleRaceControls()
    {
        if(raceControls.activeInHierarchy)
            raceControls.SetActive(false);
        else
            raceControls.SetActive(true);
    }

    public void OnLoadRace()
    {
        LoadRaceState();
    }

    public void OnSaveRace()
    {
        SaveRaceState();
    }

    void HidePopUps()
    {
        raceCompetitorPanel.gameObject.SetActive(false);
        racerBioPanel.gameObject.SetActive(false);
        raceCamSwitcher.gameObject.SetActive(false);
        raceIntroGroup.SetActive(false);
        raceIntroPanel.SetActive(false);
        lineupIntroPanel.gameObject.SetActive(false);
        racerLadder.gameObject.SetActive(false);
        raceBanner.SetActive(false);
    }

    void Update()
    {
        RaceState.RaceStage prev = raceState.m_State;
        
        OnStateUpdate();

        if(raceState.m_State != prev)
        {
            raceState.m_TimeInState = 0.0f;
            SaveRaceState();
            OnStateChange();
        }
        else
        {
            if(!paused)
                raceState.m_TimeInState += Time.deltaTime;
        }

        if (bDevmode)
            UpdateDevMode();
    }

    void StartDevMode()
    {
        SandboxServer server = GameObject.FindObjectOfType<SandboxServer>();

        int numDebugRacers = 4;

        for(int iRacer = 0; iRacer < numDebugRacers; iRacer++)
        {
            server.MakeDebugClient();
            Competitor c = m_TempCompetitors[iRacer];

            JSONObject json = new JSONObject();
            
            json = new JSONObject();
            json.AddField("body_style", "car01");
            json.AddField("body_r", "10");
            json.AddField("body_g", "150");
            json.AddField("body_b", "20");
            json.AddField("car_name", "ai_car" + iRacer.ToString());
            c.OnCarConfig(json);

            json.AddField("car_name", "ai_car" + iRacer.ToString());
            json.AddField("racer_name", "ai_" + iRacer.ToString());
            json.AddField("bio", "I am a racer");
            json.AddField("country", "USA");
            c.OnRacerInfo(json);
        }
    }

    void UpdateDevMode()
    {
        bool bTestDisconnect = false;
        // Test disconnect
        if (raceState.m_TimeInState > 3.0f && raceState.m_Competitors.Count == 2 && raceState.m_Competitors[1].IsOnline() && bTestDisconnect)
        {
            Debug.Log("Test client disconnect: " + raceState.m_Competitors[1].racer_name);
            SandboxServer server = GameObject.FindObjectOfType<SandboxServer>();
            if (raceState.m_Competitors[1].client != null)
            {
                tk.TcpClient client = raceState.m_Competitors[1].client.gameObject.GetComponent<tk.TcpClient>();
                server.OnClientDisconnected(client);
            }
        }

        // Test re-connect
        if (raceState.m_TimeInState > 5.0f && raceState.m_Competitors.Count == 2 && !raceState.m_Competitors[1].IsOnline() && bTestDisconnect)
        {
            Competitor c = raceState.m_Competitors[1];
            Debug.Log("Test client re-connect: " + c.racer_name);

            SandboxServer server = GameObject.FindObjectOfType<SandboxServer>();
            server.MakeDebugClient();

            JSONObject json = new JSONObject();
            json.AddField("car_name", c.car_name);
            json.AddField("racer_name", c.racer_name);
            json.AddField("info", c.info);
            json.AddField("country", c.country);
            raceState.m_Competitors[2].OnRacerInfo(json);
        }

        // Random driving.
        foreach (Competitor c in raceState.m_Competitors)
        {
            if(c.IsOnline() && c.has_car)
            {
                JSONObject json = new JSONObject();
                float steer = UnityEngine.Random.Range(-0.5f, 0.5f);
                json.AddField("steering", steer.ToString());
                json.AddField("throttle", "0.2");
                json.AddField("brake", "0.0");
                c.client.dispatcher.Dispatch("control", json);
            }
        }

        bool raceOverQuick = true;
        float raceTargetTime = 4.0f;

        // force Qualifying time quickly.
//         if ((raceState.m_State == RaceState.RaceStage.Qualifying ||
//             raceState.m_State == RaceState.RaceStage.Stage1Race ||
//             raceState.m_State == RaceState.RaceStage.Stage2Race) && raceOverQuick)
         if (raceState.m_State == RaceState.RaceStage.Qualifying && raceOverQuick)
        {
            //Make race over quickly.
            LapTimer[] timers = GameObject.FindObjectsOfType<LapTimer>();

            foreach (LapTimer t in timers)
            {
                float rand_delay = UnityEngine.Random.Range(0.0f, 1.0f);

                if (t.GetCurrentLapTimeSec() > (raceTargetTime + rand_delay))
                {
                    t.min_lap_time = raceTargetTime;
                    t.OnCollideFinishLine();
                    OnHitStartLine(t.gameObject);
                }
            }
        }        
    }

    internal void OnClientJoined(JsonTcpClient client)
    {
        Competitor c = new Competitor(client, this);
        m_TempCompetitors.Add(c);
    }

    public void AddCompetitorDisplay(Competitor c)
    {
        if (raceCompetitorPrefab == null)
            return;

        Debug.Log("Adding race competitor display.");
        GameObject go = Instantiate(raceCompetitorPrefab) as GameObject;
        RaceCompetitor rs = go.GetComponent<RaceCompetitor>();
        rs.Init(c, raceState);
        go.transform.SetParent(raceCompetitorPanel.transform);

        float height = raceCompetitorPanel.transform.childCount * raceCompetitorHeight;
        float margin_y = 10.0f;
        raceCompetitorPanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
        float prev_x = raceCompetitorPanel.anchoredPosition.x;
        raceCompetitorPanel.anchoredPosition = new Vector3(prev_x, -1 * (height + margin_y), 0.0f);
    }

    internal void OnClientDisconnected(JsonTcpClient client)
    {
        foreach (Competitor c in raceState.m_Competitors)
        {
            if (c.client == client)
            {
                RemoveCar(c);
                Debug.Log("Competitor " + c.racer_name + " went offline.");
                c.SetClient(null);
                return;
            }
        }

        Debug.Log("Competitor went off-line but was not found!");
    }

    public void RemoveAllCars()
    {
        foreach (RaceCheckPoint cp in checkPoints)
        {
            cp.Reset();
        }

        foreach (Competitor c in raceState.m_Competitors)
        {
            RemoveCar(c);
        }
    }

    public void RemoveCar(Competitor c)
    {
        if (!c.has_car)
            return;

        CarSpawner spawner = GameObject.FindObjectOfType<CarSpawner>();

        if (spawner)
        {
            spawner.RemoveCar(c.client);
            c.has_car = false;
        }
    }

    public void SetStatus(string msg)
    {
        //set scrolling text status
        raceInfoBar.SetInfoText(msg);
    }

    public void SetStateStr(string msg)
    {
        //set scrolling text status
        raceInfoBar.SetStateName(msg);
    }

    public void SetTimerDisplay(float t)
    {
        raceInfoBar.SetTimerDisplay(t);
    }

    void OnStateChange()
    {
        SetStateStr(raceState.m_State.ToString());

        switch (raceState.m_State)
        {
            case RaceState.RaceStage.Practice:
                OnPracticeStart();
                break;

            case RaceState.RaceStage.Qualifying:
                OnQualStart();
                break;

            case RaceState.RaceStage.PreEvent:
                OnPreEventStart();
                break;

            case RaceState.RaceStage.EventIntro:
                OnEventIntroStart();
                break;

            case RaceState.RaceStage.CompetitorIntro:
                OnCompetitorIntroStart();
                break;

            case RaceState.RaceStage.Stage1PreRace:
                OnStage1PreRaceStart();
                break;

            case RaceState.RaceStage.Stage1Race:
                OnStage1RaceStart();
                break;

            case RaceState.RaceStage.Stage1PostRace:
                OnStage1PostRaceStart();
                break;

            case RaceState.RaceStage.Stage1Completed:
                OnStage1CompletedStart();
                break;

            case RaceState.RaceStage.Stage2PreRace:
                OnStage2PreRaceStart();
                break;

            case RaceState.RaceStage.Stage2Race:
                OnStage2RaceStart();
                break;

            case RaceState.RaceStage.Stage2PostRace:
                OnStage2PostRaceStart();
                break;

            case RaceState.RaceStage.Stage2Complete:
                OnStage2CompleteStart();
                break;

            default:
                break;
        }
    }

    bool DoDelay()
    {
        if (raceState.m_TimeDelay > 0.0f)
        {
            raceState.m_TimeDelay -= Time.deltaTime;

            if (raceState.m_TimeDelay < 0.0f)
            {
                raceState.m_TimeDelay = 0.0f;
            }
            else
            {
                //don't update until time delay finished.
                return true;
            }
        }

        return false;
    }

    void OnStateUpdate()
    {
        switch(raceState.m_State)
        {
            case RaceState.RaceStage.None:
                raceState.m_State = RaceState.RaceStage.Practice;
                break;

            case RaceState.RaceStage.Practice:
                OnPracticeUpdate();
                break;

            case RaceState.RaceStage.Qualifying:
                OnQualUpdate();
                break;

            case RaceState.RaceStage.PreEvent:
                OnPreEventUpdate();
                break;

            case RaceState.RaceStage.EventIntro:
                OnEventIntroUpdate();
                break;

            case RaceState.RaceStage.CompetitorIntro:
                OnCompetitorIntroUpdate();
                break;

            case RaceState.RaceStage.Stage1PreRace:
                OnStage1PreRaceUpdate();
                break;

            case RaceState.RaceStage.Stage1Race:
                OnStage1RaceUpdate();
                break;

            case RaceState.RaceStage.Stage1PostRace:
                OnStage1PostRaceUpdate();
                break;

            case RaceState.RaceStage.Stage1Completed:
                OnStage1CompletedUpdate();
                break;

            case RaceState.RaceStage.Stage2PreRace:
                OnStage2PreRaceUpdate();
                break;

            case RaceState.RaceStage.Stage2Race:
                OnStage2RaceUpdate();
                break;

            case RaceState.RaceStage.Stage2PostRace:
                OnStage2PostRaceUpdate();
                break;

            case RaceState.RaceStage.Stage2Complete:
                OnStage2CompleteUpdate();
                break;

            default:
                break;
        }

        if(raceState.m_QueuedState != RaceState.RaceStage.None)
        {
            raceState.m_State = raceState.m_QueuedState;
            raceState.m_QueuedState = RaceState.RaceStage.None;
            Debug.Log("Queued state applied: " + raceState.m_State.ToString());
        }
    }

    void OnPracticeStart()
    {
        SetStatus("This is practice time.\n Each competitor should verify code in working order, and select a strategy for race.");
    }

    void OnPracticeUpdate()
    {
        if(raceState.m_TimeInState > raceState.m_PracticeTime)
        {
            if(raceState.m_Competitors.Count >= raceState.m_MinCompetitorCount)
                raceState.m_State = RaceState.RaceStage.Qualifying;
            else
            {
                string msg = System.String.Format("Waiting for a minimum of {0} racers to begin qualifying.", raceState.m_MinCompetitorCount);
                SetStatus(msg);
                raceState.m_TimeInState -= 10.0f; //Subtract 10 seconds
            }
        }

        SetTimerDisplay(raceState.m_PracticeTime - raceState.m_TimeInState);

        foreach (Competitor c in raceState.m_Competitors)
        {
            if(c.IsOnline())
            {
                CreateCarFor(c);
            }            
        }

        ResetRacerDQToStart();
    }

    public void ResetRacerDQToStart()
    {
        Car[] cars = GameObject.FindObjectsOfType<Car>();
        for(int iCar = 0; iCar < cars.Length; iCar++)
        {
            Car car = cars[iCar];
            LapTimer t = car.transform.GetComponentInChildren<LapTimer>();
            if(t != null && t.IsDisqualified())
            {
                car.RestorePosRot();
                t.ResetDisqualified();
                t.RestartCurrentLap();
            }
        }
    }

    public void CreateCarFor(Competitor c)
    {
        if (c.has_car || c.carConfig == null || c.client == null)
            return;

        CarSpawner spawner = GameObject.FindObjectOfType<CarSpawner>();

        if (spawner)
        {
            Debug.Log("spawning car.");
            spawner.Spawn(c.client);
            c.has_car = true;
            c.client.dispatcher.Dispatch("car_config", c.carConfig);

            if (c.camConfig != null)
                c.client.dispatcher.Dispatch("cam_config", c.camConfig);
        }
    }

    void OnQualStart() 
    {
        SetStatus("This is qualification time. Each competitor must complete one AI lap to qualify for the race.");

        RemoveAllCars();
    }

    Competitor GetNextQualifier()
    {
        //first try for the first competitor with no qualifying time.
        //but follow the raceState.m_iQual
        int iComp = 0;
        foreach(Competitor c in raceState.m_Competitors)
        {
            if (c.qual_time == 0.0 && c.IsOnline())
            {
                if(raceState.m_iQual == iComp)
                    return c;

                iComp += 1;
            }
        }

        return null;
    }

    public Competitor GetCompetitorbyName(string race_name)
    {
        foreach (Competitor c in raceState.m_Competitors)
        {
            if (c.racer_name == race_name)
                return c;
        }

        return null;
    }

    public Competitor GetCompetitorbyCarName(string car_name)
    {
        foreach (Competitor c in raceState.m_Competitors)
        {
            if (c.car_name == car_name)
                return c;
        }

        return null;
    }

    public Competitor GetCompetitorByClient(tk.JsonTcpClient client)
    {
        foreach (Competitor c in raceState.m_Competitors)
        {
            if (c.client == client)
                return c;
        }

        return null;
    }

    public float GetBestTime(string car_name)
    {
        LapTimer[] timers = GameObject.FindObjectsOfType<LapTimer>();

        foreach (LapTimer t in timers)
        {
            if (t.car_name == car_name)
                return t.GetBestLapTimeSec();
        }

        return 0.0f;
    }

    public LapTimer GetLapTimer(string car_name)
    {
        LapTimer[] timers = GameObject.FindObjectsOfType<LapTimer>();

        foreach (LapTimer t in timers)
        {
            if (t.car_name == car_name)
                return t;
        }

        return null;
    }

    public int GetNumQualified()
    {
        int numQualified = 0;

        foreach (Competitor c in raceState.m_Competitors)
            if (c.qual_time != 0.0)
                numQualified += 1;

        return numQualified;
    }

    bool HaveAllAttemptedQual()
    {
        foreach (Competitor c in raceState.m_Competitors)
            if (!c.got_qual_attempt)
                return false;

        return true;
    }

    void OnQualUpdate() 
    {
        race_num_laps = 1;

        if (!raceCompetitorPanel.gameObject.activeInHierarchy)
            raceCompetitorPanel.gameObject.SetActive(true);

        if (DoDelay())
        {
            //mainly to allow people to read status.
        }
        else if(raceState.m_CurrentQualifier == "None")
        {
            Competitor c = GetNextQualifier();

            int max_tries = raceState.m_Competitors.Count + 1;
            int iTry = 0;
            while (c == null && iTry < max_tries)
            {
                raceState.m_iQual += 1;
                if(raceState.m_iQual >= raceState.m_Competitors.Count)
                {
                    raceState.m_iQual = 0;
                    break;
                }

                c = GetNextQualifier();
                iTry += 1;
            }

            if(c != null)
            {
                Debug.Log("Starting qual for " + c.racer_name);
                SetStatus("Starting qualification lap for " + c.racer_name);

                CreateCarFor(c);
                //put car at start line and let them go.
                raceState.m_CurrentQualifier = c.racer_name;
                raceState.m_CurrentQualElapsed = 0.0f;
                OnResetRace();
                StartRace();
                SaveRaceState();

                ShowRacerBio(c);
            }    
        }
        else
        {
            Competitor c = GetCompetitorbyName(raceState.m_CurrentQualifier);

            if (c == null)
            {
                Debug.LogError("Where is our competitor? " + raceState.m_CurrentQualifier);
                raceState.m_CurrentQualifier = "None";
            }

            LapTimer lt = GetLapTimer(c.car_name);
            float t = dq_time;

            if (lt == null)
            {
                Debug.LogError("Where is our competitor LapTimer? " + raceState.m_CurrentQualifier);
                t = dq_time;
            }
            else
            {
                t = lt.GetCurrentLapTimeSec();
            }

            if (c == null || !c.IsOnline())
                raceState.m_CurrentQualifier = "None";

            raceState.m_CurrentQualElapsed = t;            

            if(raceState.m_CurrentQualElapsed > raceState.m_TimeLimitQual)
            {
                c.got_qual_attempt = true;
                string msg = System.String.Format("Qualifying run over time limit of {0} seconds for {1}.", raceState.m_TimeLimitQual, c.racer_name);
                Debug.Log(msg);
                SetStatus(msg);
                //Boot current car.
                raceState.m_CurrentQualifier = "None";
                raceState.m_TimeDelay = 3.0f;
            }
            else if(IsRaceOver())
            {
                if(c != null)
                    c.got_qual_attempt = true;

                if (lt.IsDisqualified())
                {
                    raceState.m_iQual += 1;
                    string msg = c.racer_name + " was disqualified. Will get another chance, given enough time.";
                    Debug.Log(msg);
                    SetStatus(msg);

                    c.qual_time = 0.0f;
                    RemoveCar(c);
                }
                else if (c != null)
                {
                    t = lt.GetBestLapTimeSec();
                    string msg = c.racer_name + " finished the qualification lap in " + System.String.Format("{0:F2}", t) + " sec.";
                    Debug.Log(msg);
                    SetStatus(msg);

                    c.qual_time = t;
                    RemoveCar(c);

                    SortAndRankQual();
                }
                else
                {
                    SetStatus(raceState.m_CurrentQualifier + " left before completing a lap.");
                }

                raceState.m_CurrentQualifier = "None";
                raceState.m_TimeDelay = 3.0f;

                SaveRaceState();
            }
        }

        if (raceState.m_TimeInState > raceState.m_QualTime)
        {
            int numQualified = GetNumQualified();

            if (numQualified < 2)
            {
                SetStatus("Fewer than two competitors have qualified. Extending Qualification state time.");
                raceState.m_QualTime *= 2.0f;
                raceState.m_TimeDelay = 3.0f;
            }
            else if(!HaveAllAttemptedQual())
            {
                SetStatus("Not everyone has had a qualification attempt. Extending Qualification state time.");
                raceState.m_QualTime *= 2.0f;
                raceState.m_TimeDelay = 3.0f;
            }
            else
            {
                //Leave for the event intro state.
                raceState.m_State = RaceState.RaceStage.PreEvent;
            }
        }

        SetTimerDisplay(raceState.m_QualTime - raceState.m_TimeInState);
    }

    void OnPreEventStart()
    {
        race_num_laps = 3;
        RemoveAllCars();

        raceCompetitorPanel.gameObject.SetActive(false);
        raceCamSwitcher.gameObject.SetActive(false);
    }

    void OnPreEventUpdate()
    {
        if (raceState.m_TimeInState > raceState.m_BetweenStageTime)
        {
            raceState.m_State = RaceState.RaceStage.EventIntro;
        }

        SetStatus("Get Ready for live Twitch feed.");

        SetTimerDisplay(raceState.m_BetweenStageTime - raceState.m_TimeInState);
    }

    void OnEventIntroStart()
    {
        RemoveAllCars();

        racerBioPanel.gameObject.SetActive(false);
        raceCompetitorPanel.gameObject.SetActive(false);
        raceCamSwitcher.gameObject.SetActive(false);

        SetStatus("Welcome to Virtual Race League DIYRobocars online racing event!!");

        raceIntroGroup.SetActive(true);
        raceIntroPanel.SetActive(true);

        DropCompetitorsThatDidNotQual();
        PrepareStage1Pairings();
    }

    void OnEventIntroUpdate()
    {
        if (raceState.m_TimeInState > raceState.m_IntroTime)
        {
            raceState.m_State = RaceState.RaceStage.CompetitorIntro;            
        }

        SetTimerDisplay(raceState.m_IntroTime - raceState.m_TimeInState);
    }

    void OnCompetitorIntroStart()
    {
        raceIntroPanel.SetActive(false);
        lineupIntroPanel.gameObject.SetActive(true);
    }

    void OnCompetitorIntroUpdate()
    {
        if (raceState.m_TimeInState > raceState.m_IntroTime)
        {
            raceState.m_State = RaceState.RaceStage.Stage1PreRace;
            lineupIntroPanel.gameObject.SetActive(false);
            raceIntroGroup.SetActive(false);
            AnnounceDueUp();
        }
    }

    List<Pairing> GetStage2List()
    {
        if(raceState.m_Stage2c_final.Count > 0)
            return raceState.m_Stage2c_final;

        if(raceState.m_Stage2b_2pairs.Count > 0)
            return raceState.m_Stage2b_2pairs;

        if(raceState.m_Stage2a_4pairs.Count > 0)
            return raceState.m_Stage2a_4pairs;

            
        return null;
    }

    private static int QualTimeSort(Competitor x, 
                            Competitor y) 
    {
        if (x.qual_time == 0.0f || x.dropped)
            return 1;

        if (y.qual_time == 0.0f || y.dropped)
            return -1;

        return x.qual_time.CompareTo(y.qual_time);
    }

    private static int Stage1TimeSort(Competitor x, 
                            Competitor y) 
    { 
        return x.best_stage1_time.CompareTo(y.best_stage1_time);
    }

    void DropCompetitorsThatDidNotQual()
    {
        for(int i = raceState.m_Competitors.Count - 1; i > 0; i--)
        {
            if(raceState.m_Competitors[i].qual_time == 0.0f)
                raceState.m_Competitors.RemoveAt(i);
        }
    }

    public static bool IsOdd(int value)
    {
        return value % 2 != 0;
    }

    void PrepareStage1Pairings()
    {
        raceState.m_Stage1Next = 0;
        raceState.m_Stage1Order = new List<Pairing>();
        raceState.m_Competitors.Sort(QualTimeSort);

        //make mutable copy of competitor list
        List<Competitor> comp = new List<Competitor>();
        foreach (Competitor c in raceState.m_Competitors)
            comp.Add(c);

        if (IsOdd(comp.Count))
        {
            //Then the first competitor gets a bye.
            Pairing p = new Pairing();
            Competitor a = raceState.m_Competitors[0];
            p.name1 = a.racer_name;
            p.time1 = 0.0f;

            p.time2 = dq_time;
            p.name2 = "solo";

            raceState.m_Stage1Order.Add(p);

            //remove top competitor to make list even.
            comp.Remove(a);
        }

        for (int i = 0; i < comp.Count / 2; i++)
        {
            Pairing p = new Pairing();
            Competitor a = comp[i];
            p.name1 = a.racer_name;
            p.time1 = 0.0f;
            p.time2 = 0.0f;

            int iB = comp.Count - i - 1;

            if(iB > i)
            {
                Competitor b = comp[iB];
                p.name2 = b.racer_name;
            }
            else
            {
                p.name2 = "solo";
                Debug.LogError("Logic error. Should be even pairing!");
            }

            raceState.m_Stage1Order.Add(p);
        }

        lineupIntroPanel.Init(raceState.m_Stage1Order);
    }

    void SortAndRankQual()
    {
        raceState.m_Competitors.Sort(QualTimeSort);

        for (int i = 0; i < raceState.m_Competitors.Count; i++)
        {
            raceState.m_Competitors[i].qual_place = i + 1;
        }
    }

    void SortAndRankStage1()
    {
        raceState.m_Competitors.Sort(Stage1TimeSort);

        for (int i = 0; i < raceState.m_Competitors.Count; i++)
        {
            raceState.m_Competitors[i].stage1_place = i + 1;
        }
    }

    void TrimToTopEightCompetitors()
    {
        //remove dropped competitors.
        for (int i = raceState.m_Competitors.Count - 1; i >= 0; i--)
        {
            Competitor c = raceState.m_Competitors[i];

            if (c.dropped)
                raceState.m_Competitors.RemoveAt(i);
        }

        raceState.m_Competitors.Sort(Stage1TimeSort);
        List<Competitor> stage2Competitors = new List<Competitor>();

        //only the top 8 go through.
        for (int i = 0; i < raceState.m_Competitors.Count && i < 8; i++)
        {
            Competitor c = raceState.m_Competitors[i];
            stage2Competitors.Add(c);
        }

        //Re-make the list with just remaining competitors.
        raceState.m_Competitors.Clear();
        for (int i = 0; i < stage2Competitors.Count; i++)
            raceState.m_Competitors.Add(stage2Competitors[i]);
    }

    List<Pairing> PreparePairingsFromStage1()
    {
        raceState.m_Stage2Next = 0;
        List<Pairing> pairs = new List<Pairing>();
        List<Competitor> stage2Competitors = new List<Competitor>();

        for (int i = 0; i < raceState.m_Competitors.Count; i++)
            stage2Competitors.Add(raceState.m_Competitors[i]);

        //make mutable copy of competitor list
        List<Competitor> comp = new List<Competitor>();
        foreach (Competitor c in stage2Competitors)
            comp.Add(c);

        if (IsOdd(comp.Count))
        {
            //Then the first competitor gets a bye.
            Pairing p = new Pairing();
            Competitor a = comp[0];
            p.name1 = a.racer_name;
            p.time1 = 0.0f;

            p.time2 = dq_time;
            p.name2 = "solo";

            pairs.Add(p);

            //remove top competitor to make list even.
            comp.Remove(a);
        }

        for (int i = 0; i < comp.Count / 2; i++)
        {
            Pairing p = new Pairing();
            Competitor a = comp[i];
            p.name1 = a.racer_name;
            p.time1 = 0.0f;
            p.time2 = 0.0f;

            int iB = comp.Count - i - 1;

            if(iB > i)
            {
                Competitor b = comp[iB];
                p.name2 = b.racer_name;
            }
            else
            {
                Debug.LogError("Logic error, should be even pairings.");
                p.name2 = "solo";
            }

            pairs.Add(p);
        }

        return pairs;
    }

    List<Pairing> PrepareStage2bPairings(List<Pairing> prevPairs)
    {
        raceState.m_Stage2Next = 0;
        List<Pairing> new_pairs = new List<Pairing>();

        Pairing addEnd = null;

        if (IsOdd(prevPairs.Count))
        {
            //Take the winner of the last pairing and allow them a bye.
            Pairing p = prevPairs[prevPairs.Count - 1];

            if (p.time1 > p.time2)
            {
                p.name1 = p.name2;
                p.time1 = p.time2;
            }
            
            p.time2 = dq_time;
            p.name2 = "solo";

            addEnd = p;
            prevPairs.Remove(p);
        }

        for (int i = 0; i < prevPairs.Count; i += 2)
        {
            Pairing p1 = prevPairs[i];
            Pairing p2 = prevPairs[i + 1];
            Pairing p = new Pairing();

            if(p1.time1 < p1.time2)
                p.name1 = p1.name1;
            else
                p.name1 = p1.name2;


            if(p2.time1 < p2.time2)
                p.name2 = p2.name1;
            else
                p.name2 = p2.name2;

            p.time1 = 0.0f;
            p.time2 = 0.0f;

            new_pairs.Add(p);
        }

        if (addEnd != null)
            new_pairs.Add(addEnd);

        return new_pairs;
    }


    Pairing GetCurrentPairing()
    {
        switch(raceState.m_State)
        {
            case RaceState.RaceStage.Qualifying:
            case RaceState.RaceStage.Stage1PreRace:
            case RaceState.RaceStage.Stage1Race:
            case RaceState.RaceStage.Stage1PostRace:
            {
                if (raceState.m_Stage1Order.Count == 0)
                    return null;

                return raceState.m_Stage1Order[raceState.m_Stage1Next % raceState.m_Stage1Order.Count];
            }

            case RaceState.RaceStage.Stage1Completed:
            case RaceState.RaceStage.Stage2PreRace:
            case RaceState.RaceStage.Stage2Race:
            case RaceState.RaceStage.Stage2PostRace:
            {
                List<Pairing> pairingList = GetStage2List();
                if (pairingList == null || pairingList.Count == 0)
                    return null;

                return pairingList[raceState.m_Stage2Next % pairingList.Count];
            }
            
            default:
                break;
        }

        return null;
    }

    Pairing GetNextPairing()
    {
        if(raceState.m_State == RaceState.RaceStage.Qualifying ||
            raceState.m_State == RaceState.RaceStage.Stage1PreRace)
        {
            if (raceState.m_Stage1Order.Count == 0)
                return null;

            Pairing p = raceState.m_Stage1Order[(raceState.m_Stage1Next + 1) % raceState.m_Stage1Order.Count];
            return p;
        }

        return null;
    }

    void AnnounceDueUp()
    {
        Pairing p = GetCurrentPairing();

        if(p != null)
        {
            string dueUp;

            if (p.GetNumRacers() == 0)
            {
                dueUp = "Due up: No competitors.";
            }
            else if (p.GetNumRacers() == 1)
            {
                dueUp = System.String.Format("Odd number of competitors, so {0} races alone for time.", p.name1);
            }
            else
            {
                dueUp = System.String.Format("Due up: {0} vs {1}", p.name1, p.name2);
            }

            SetStatus(dueUp);
        }
    }

    void CreateCarsForPairing(Pairing p)
    {
        Competitor c = GetCompetitorbyName(p.name1);

        if (c != null)
            CreateCarFor(c);

        c = GetCompetitorbyName(p.name2);

        if (c != null)
            CreateCarFor(c);
    }

    void OnStage1PreRaceStart()
    {
        AnnounceDueUp();

        raceCompetitorPanel.gameObject.SetActive(true);

        Pairing p = GetCurrentPairing();

        if (p == null)
        {
            Debug.LogWarning("No competitors!");
            SetStatus("No competitors found.");
            return;
        }

        OnResetRace();
    }

    int GetNumCars()
    {
        Car[] cars = GameObject.FindObjectsOfType<Car>();
        return cars.Length;
    }

    void BlockCarsFromMoving()
    {
        Car[] icars = GameObject.FindObjectsOfType<Car>();
        foreach (Car car in icars)
        {
            car.blockControls = true;
        }
    }

    void ShowRemoveRacerDialog()
    {
        Pairing p = GetCurrentPairing();

        Competitor c1 = GetCompetitorbyName(p.name1);
        Competitor c2 = GetCompetitorbyName(p.name2);

        if(c1 != null && !c1.has_car && !c1.dropped)
        {
            removeRacerName = p.name1;
        }
        else if (c2 != null && !c2.has_car && !c2.dropped)
        {
            removeRacerName = p.name2;
        }

        if (removeRacerName.Length < 1)
            return;

        dropCompetitorPanel.gameObject.SetActive(true);
        dropCompetitorPanel.prompt.text = System.String.Format("{0} has not connected. Would you like to remove them from the race?", removeRacerName);
    }

    public void OnChooseToRemoveRacer()
    {
        dropCompetitorPanel.gameObject.SetActive(false);

        Pairing p = GetCurrentPairing();

        Competitor c = GetCompetitorbyName(removeRacerName);

        if (c != null)
            c.dropped = true;
        else
            return;

        if (p.name1 == removeRacerName)
        {
            if (p.name2 != "solo")
                p.name1 = p.name2;
            else
                p.name1 = "drop";

            p.name2 = "solo";
        }
        else if (p.name2 == removeRacerName)
        {
            p.name2 = "solo";            
        }

        string msg = System.String.Format("{0} has been dropped from the event.", removeRacerName);
        SetStatus(msg);

        removeRacerName = "";
        
        SaveRaceState();
    }

    public void OnCancelRemoveRacer()
    {
        dropCompetitorPanel.gameObject.SetActive(false);
        removeRacerName = "";
    }

    void OnStage1PreRaceUpdate()
    {
        Pairing p = GetCurrentPairing();

        if(p != null)
            CreateCarsForPairing(p);

        int numCars = GetNumCars();

        BlockCarsFromMoving();

        if (p.GetNumRacers() == 0)
        {
            SetStatus("No competitors to race.");
            raceState.m_State = RaceState.RaceStage.Stage1PostRace;
        }
        else
            if (numCars > p.GetNumRacers())
        {
            SetStatus("Too many cars for the race.");
            RemoveAllCars();            
        }

        if (raceState.m_TimeInState > raceState.m_BetweenStageTime)
        {            
            if (numCars < p.GetNumRacers())
            {                
                SetStatus("Waiting for competitors to connect.");
                ShowRemoveRacerDialog();
                raceState.m_TimeInState = 0.0f;
            }
            else
            if (numCars == p.GetNumRacers())
            {
                raceState.m_State = RaceState.RaceStage.Stage1Race;
            }
        }

        if (!raceCompetitorPanel.gameObject.activeInHierarchy)
            raceCompetitorPanel.gameObject.SetActive(true);

        SetTimerDisplay(raceState.m_BetweenStageTime - raceState.m_TimeInState);
    }


    void OnStage1RaceStart()
    {
        raceCompetitorPanel.gameObject.SetActive(false);
        StartRace();
    }
    
    void OnStage1RaceUpdate()
    {
        if(raceState.m_TimeInState > 10.0f && !racerBioPanel.gameObject.activeInHierarchy && raceState.m_TimeInState < 11.0f)
        {
            StartCoroutine(ShowBothRacerBios());
        }

        // testing for time in state allows easier fast forward, rewind.
        if(IsRaceOver() && raceState.m_TimeInState > 10.0f)
        {
            raceState.m_State = RaceState.RaceStage.Stage1PostRace;
        }

        if (!raceCompetitorPanel.gameObject.activeInHierarchy)
            raceCompetitorPanel.gameObject.SetActive(true);

        SetTimerDisplay(raceState.m_TimeInState);
    }

    IEnumerator ShowBothRacerBios()
    {
        Pairing p = GetCurrentPairing();

        Competitor c1 = GetCompetitorbyName(p.name1);
        Competitor c2 = GetCompetitorbyName(p.name2);

        racerBioPanel.gameObject.SetActive(true);
        racerBioPanel.SetBio(c1.racerBio);

        yield return new WaitForSeconds(raceState.m_RacerBioDisplayTime);

        racerBioPanel.gameObject.SetActive(false);

        if(c2 != null)
        {
            yield return new WaitForSeconds(3.0f);

            racerBioPanel.gameObject.SetActive(true);
            racerBioPanel.SetBio(c2.racerBio);

            yield return new WaitForSeconds(raceState.m_RacerBioDisplayTime);

            racerBioPanel.gameObject.SetActive(false);
        }
    }

    void OnStage1PostRaceStart()
    {
        Pairing p = GetCurrentPairing();

        if (p == null)
            return;

        Competitor a = GetCompetitorbyName(p.name1);

        if (a != null)
        {
            p.time1 = GetBestTime(a.car_name);
            a.best_stage1_time = p.time1;
            LapTimer ta = GetLapTimer(a.car_name);

            if (ta == null)
                return;

            if (ta.GetNumLapsCompleted() > 0)
                raceState.m_AnyCompetitorFinishALap = true;

            if (ta.IsDisqualified())
            {
                p.time1 = dq_time;
            }
        }
        else
        {
            p.time1 = dq_time;
        }


        //remember sometimes only one competitor!
        Competitor b = GetCompetitorbyName(p.name2);

        if (b != null)
        {
            p.time2 = b != null ? GetBestTime(b.car_name) : dq_time;
            b.best_stage1_time = p.time2;
            LapTimer tb = GetLapTimer(b.car_name);

            if (tb.GetNumLapsCompleted() > 0)
                raceState.m_AnyCompetitorFinishALap = true;

            if (tb.IsDisqualified())
            {
                p.time2 = dq_time;
            }
        }
        else
        {
            p.time2 = dq_time;
        }

        SortAndRankStage1();
        DoRaceSummary();
        RemoveAllCars();
    }

    bool DidAllRacersDQ()
    {
        Pairing p = GetCurrentPairing();
        return p.time1 == dq_time && p.time2 == dq_time;
    }

    bool DidAnyRacersDQ()
    {
        Pairing p = GetCurrentPairing();
        return p.time1 == dq_time || p.time2 == dq_time;
    }   

    void OnStage1PostRaceUpdate()
    {
        if(raceState.m_TimeInState > raceState.m_TimeToShowRaceSummary / 2)
        {
            DoLineupSummary();
        }

        if(raceState.m_TimeInState > raceState.m_TimeToShowRaceSummary)
        {
            OnLeaveStage1PostRace(false);
        }

        SetTimerDisplay(raceState.m_TimeToShowRaceSummary - raceState.m_TimeInState);
    }

    void OnLeaveStage1PostRace(bool ff)
    {
        if (DidAllRacersDQ() && raceState.m_RaceRestarts < raceState.m_RaceRestartsLimit && !raceState.m_AnyCompetitorFinishALap)
        {
            SetStatus("All racers DQ'ed. Restarting race!");
            raceState.m_RaceRestarts += 1;
        }
        else
        {
            if (DidAllRacersDQ() && raceState.m_RaceRestarts >= raceState.m_RaceRestartsLimit)
            {
                SetStatus("Hit limit of restarts. Racers accept times!");
            }

            raceState.m_RaceRestarts = 0;
            raceState.m_Stage1Next += 1; //needs to hit limit and go to stage 2.
        }

        lineupIntroPanel.gameObject.SetActive(false);

        if (raceState.m_Stage1Next >= raceState.m_Stage1Order.Count)
        {
            if(ff)
                raceState.m_QueuedState = RaceState.RaceStage.Stage1Completed;
            else
                raceState.m_State = RaceState.RaceStage.Stage1Completed;
        }
        else
        {
            if (ff)
                raceState.m_QueuedState = RaceState.RaceStage.Stage1PreRace;
            else
                raceState.m_State = RaceState.RaceStage.Stage1PreRace;
        }
    }

    void OnStage1CompletedStart()
    {
        TrimToTopEightCompetitors();

        lineupIntroPanel.gameObject.SetActive(true);

        SetStatus("Stage1 Complete!");
    }

    void OnStage1CompletedUpdate()
    {
        if (raceState.m_TimeInState > raceState.m_BetweenStageTime)
        {
            lineupIntroPanel.gameObject.SetActive(false);
            raceState.m_State = RaceState.RaceStage.Stage2PreRace;
        }

        SetTimerDisplay(raceState.m_BetweenStageTime - raceState.m_TimeInState);
    }

    int NumFinishedPairs(List<Pairing> pairs)
    {
        int count = 0;

        foreach(Pairing p in pairs)
        {
            if (p.time1 != 0.0f || p.time2 != 0.0f)
                count += 1;
        }

        return count;
    }

    bool AllPairsFinished(List<Pairing> pairs)
    {
        foreach (Pairing p in pairs)
        {
            if (p.time1 == 0.0f || p.time2 == 0.0f)
                return false;
        }

        return true;
    }

    void OnStage2PreRaceStart()
    {
        int numRacers = raceState.m_Competitors.Count;

        //Show the tree view of stage2
        if(raceState.m_Stage2a_4pairs.Count == 0 && numRacers >= 5)
        {
            raceState.m_Stage2a_4pairs = PreparePairingsFromStage1();
            racerLadder.Init(raceState.m_Stage2a_4pairs, 0);
        }
        else if (raceState.m_Stage2b_2pairs.Count == 0)
        {
            if (numRacers >= 5 && NumFinishedPairs(raceState.m_Stage2a_4pairs) == ((numRacers + 1) / 2))
            {
                raceState.m_Stage2b_2pairs = PrepareStage2bPairings(raceState.m_Stage2a_4pairs);
                racerLadder.Init(raceState.m_Stage2b_2pairs, 1);
            }
            else if (numRacers < 5)
            {
                raceState.m_Stage2b_2pairs = PreparePairingsFromStage1();
                racerLadder.Init(raceState.m_Stage2b_2pairs, 1);
            }            
        }
        else if (raceState.m_Stage2c_final.Count == 0)
        {
            if (numRacers > 2 && NumFinishedPairs(raceState.m_Stage2b_2pairs) == 2)
            {
                raceState.m_Stage2c_final = PrepareStage2bPairings(raceState.m_Stage2b_2pairs);
                racerLadder.Init(raceState.m_Stage2c_final, 2);
            }
            else if (numRacers == 2)
            {
                raceState.m_Stage2c_final = PreparePairingsFromStage1();
                racerLadder.Init(raceState.m_Stage2c_final, 2);
            }
        }

        racerLadder.gameObject.SetActive(true);

        AnnounceDueUp();

        // ladderPanel.gameObject.SetActive(true);

        Pairing p = GetCurrentPairing();

        if (p == null)
        {
            Debug.LogWarning("No competitors!");
            SetStatus("No competitors found.");
            raceState.m_State = RaceState.RaceStage.Stage1Completed;
            return;
        }

        CreateCarsForPairing(p);

        OnResetRace();
    }

    void OnStage2PreRaceUpdate()
    {
        Pairing p = GetCurrentPairing();

        CreateCarsForPairing(p);

        BlockCarsFromMoving();

        int numCars = GetNumCars();

        if (numCars == p.GetNumRacers() && numCars == 1)
        {
            SetStatus("Solo racer gets a BYE in the race ladder.");
            raceState.m_State = RaceState.RaceStage.Stage2PostRace;
        }
        else
        if (p.GetNumRacers() == 0)
        {
            SetStatus("No competitors to race.");
            raceState.m_State = RaceState.RaceStage.Stage2PostRace;
        }
        else
            if (numCars > p.GetNumRacers())
        {
            SetStatus("Too many cars for the race.");
            RemoveAllCars();
        }

        if (raceState.m_TimeInState > raceState.m_BetweenStageTwoTime)
        {          
            if (numCars < p.GetNumRacers())
            {
                SetStatus("Waiting for competitors to connect.");
                ShowRemoveRacerDialog();
                raceState.m_TimeInState = 0.0f;
            }
            else
            if (numCars == p.GetNumRacers())
            {
                OnResetRace();
                racerLadder.gameObject.SetActive(false);
                raceState.m_State = RaceState.RaceStage.Stage2Race;
            }
        }

        if (!raceCompetitorPanel.gameObject.activeInHierarchy)
            raceCompetitorPanel.gameObject.SetActive(true);

        SetTimerDisplay(raceState.m_BetweenStageTwoTime - raceState.m_TimeInState);
    }

    void OnStage2RaceStart()
    {
        StartRace();
    }

    void OnStage2RaceUpdate()
    {
        // testing for time in state allows easier fast forward, rewind.
        if (IsRaceOver() && raceState.m_TimeInState > 10.0f)
        {
            raceState.m_State = RaceState.RaceStage.Stage2PostRace;
        }

        SetTimerDisplay(raceState.m_TimeInState);
    }
    
    void OnStage2PostRaceStart()
    {
        Pairing p = GetCurrentPairing();
        Competitor a = GetCompetitorbyName(p.name1);
        p.time1 = GetBestTime(a.car_name);
        LapTimer ta = GetLapTimer(a.car_name);

        if(p.GetNumRacers() == 1)
        {
            p.time1 = a.best_stage1_time;
        }
        else if (ta.IsDisqualified())
        {
            if(ta.GetNumLapsCompleted() == 0)                
                p.time1 = dq_time;
        }

        if (ta.GetNumLapsCompleted() > 0)
            raceState.m_AnyCompetitorFinishALap = true;

        Competitor b = GetCompetitorbyName(p.name2);

        if (b != null)
        {
            p.time2 = GetBestTime(b.car_name);
            LapTimer tb = GetLapTimer(b.car_name);

            if (tb.IsDisqualified())
            {
                if (tb.GetNumLapsCompleted() == 0)
                    p.time2 = dq_time;
            }

            if (tb.GetNumLapsCompleted() > 0)
                raceState.m_AnyCompetitorFinishALap = true;
        }
        else
        {
            p.time2 = dq_time;
        }

        if(p.GetNumRacers() == 2)
            DoRaceSummary();

        RemoveAllCars();

        if (DidAllRacersDQ() && !raceState.m_AnyCompetitorFinishALap)
        {
            SetStatus("All racers DQ'ed. Restarting race!");
        }
        else
        {
            int stage = 0;

            if (raceState.m_Stage2c_final.Count > 0)
                stage = 2;
            else if (raceState.m_Stage2b_2pairs.Count > 0)
                stage = 1;

            racerLadder.SetResult(p, stage);
        }
    }

    void OnStage2PostRaceUpdate()
    {
        if (raceState.m_TimeInState > raceState.m_BetweenStageTwoTime)
        {
            OnLeaveStage2PostRace(false);            
        }

        SetTimerDisplay(raceState.m_BetweenStageTwoTime - raceState.m_TimeInState);
    }

    void OnLeaveStage2PostRace(bool ff)
    {
        bool restart = false;

        if (DidAllRacersDQ() && !raceState.m_AnyCompetitorFinishALap)
        {
            restart = true;

            // The times need to be reset to cause the race to go again.
            Pairing p = GetCurrentPairing();
            p.time1 = 0.0f;
            p.time2 = 0.0f;
        }
        else
        {
            // needs to hit limit and go to stage 2.
            raceState.m_Stage2Next += 1;
        }

        if (raceState.m_Stage2c_final.Count == 1 && !restart)
        {
            if (ff)
                raceState.m_QueuedState = RaceState.RaceStage.Stage2Complete;
            else
                raceState.m_State = RaceState.RaceStage.Stage2Complete;
        }
        else
        {
            if (ff)
                raceState.m_QueuedState = RaceState.RaceStage.Stage2PreRace;
            else
                raceState.m_State = RaceState.RaceStage.Stage2PreRace;
        }
    }

    void OnStage2CompleteStart()
    {
        raceSummary.gameObject.SetActive(false);

        //Put up thanks dialog...
        // Trophy... final status..
        racerLadder.gameObject.SetActive(true);

        RemoveAllCars();

        //Event is finished.
        SetStatus("Event has concluded! Thanks for watching. Bye everyone!");
    }

    void OnStage2CompleteUpdate()
    {
        if(raceState.m_TimeInState > 10.0f && !raceSummary.gameObject.activeInHierarchy)
        {
            racerLadder.gameObject.SetActive(false);
            DoFinalRaceSummary();
        }

        SetTimerDisplay(raceState.m_TimeInState);
    }

    public void OnRaceRestartPressed()
    {
        if (raceState.m_State == RaceState.RaceStage.Qualifying)
        {
            raceState.m_CurrentQualElapsed = 0.0f;
            OnResetRace();
            StartRace();
        }
        else if (raceState.m_State == RaceState.RaceStage.Stage1Race)
        {
            RemoveAllCars();
            raceState.m_QueuedState = RaceState.RaceStage.Stage1PreRace;
        }
        else if (raceState.m_State == RaceState.RaceStage.Stage2Race)
        {
            RemoveAllCars();
            raceState.m_QueuedState = RaceState.RaceStage.Stage2PreRace;
        }
    }

    public void OnStopRacePressed()
    {
        DQAll();        
    }

    public void OnDQPressed()
    {
        Car[] cars = GameObject.FindObjectsOfType<Car>();
        foreach(Car car in cars)
        {
            LapTimer timer = car.gameObject.transform.GetComponentInChildren<LapTimer>();

            if(timer == null)
                continue;

            if(!timer.IsDisqualified())
            {
                OnCarDQ(car.gameObject, true);
                break;
            }
        }
    }

    public void DQAll()
    {
        Car[] cars = GameObject.FindObjectsOfType<Car>();
        foreach (Car car in cars)
        {
            LapTimer timer = car.gameObject.transform.GetComponentInChildren<LapTimer>();

            if (timer == null)
                continue;

            if (!timer.IsDisqualified())
            {
                OnCarDQ(car.gameObject, true);
            }
        }
    }

    public void ResetRaceStatus()
    {

    }

    public void ResetRaceCams()
    {
        if(camSwitchers.Length < 2)
            return;

        CarSpawner spawner = GameObject.FindObjectOfType<CarSpawner>();

        if(spawner != null)
            spawner.DeactivateSplitScreen();

        if(Camera.main)
            Camera.main.gameObject.SetActive(false);

        CameraSwitcher prev = camSwitchers[camSwitchers.Length - 1];

        for (int iCam = 0; iCam < camSwitchers.Length; iCam++)
        {
            CameraSwitcher sw = camSwitchers[iCam];
            sw.next = camSwitchers[(iCam + 1) % camSwitchers.Length];
            sw.previous = prev;
            prev = sw;

            if(iCam == 1)
            {
                sw.gameObject.SetActive(true);
            }
            else
                sw.gameObject.SetActive(false);
        }

        raceCamSwitcher.OnDynamicCams();
        camSwitchers[0].SwitchToThisCam();
    }

    public void OnResetRace()
    {
        bRaceActive = true;

        //Reset race status panels
        raceStatusPanel.gameObject.SetActive(false);
        raceSummary.gameObject.SetActive(false);
        

        // disable these things that distract from the race.
        foreach(GameObject obj in objToDisable)
        {
            obj.SetActive(false);
        }

        CarSpawner spawner = GameObject.FindObjectOfType<CarSpawner>();

        Car[] cars = GameObject.FindObjectsOfType<Car>();
        for(int iCar = 0; iCar < cars.Length; iCar++)
        {
            Car car = cars[iCar];

            // We may have had cars leave, so find a new position.
            Vector3 startPos = spawner.GetCarStartPos(iCar, false);

            car.startPos = startPos;

            car.RestorePosRot();
        }

        BlockCarsFromMoving();

        tk.TcpCarHandler[] carHanders = GameObject.FindObjectsOfType<tk.TcpCarHandler>();
        foreach (tk.TcpCarHandler handler in carHanders)
        {
            handler.SendStopRaceMsg();
        }

        // reset lap timers
        LapTimer[] timers = GameObject.FindObjectsOfType<LapTimer>();
        foreach(LapTimer t in timers)
        {
            t.ResetRace();
        }

        foreach (RaceCheckPoint cp in checkPoints)
        {
            cp.Reset();
        }

        raceState.m_AnyCompetitorFinishALap = false;
        raceCamSwitcher.gameObject.SetActive(true);
        ResetRaceCams();
    }

    public void StartRace()
    {
        BlockCarsFromMoving();
        raceBanner.SetActive(true);
        StartCoroutine(DoRaceBanner());
    }

    public void AddLapTimer(LapTimer t, tk.JsonTcpClient client)
    {
        if(raceStatusPrefab == null)
            return;

        Competitor c = GetCompetitorByClient(client);

        Debug.Log("Adding lap timer for " + c.car_name);
        GameObject go = Instantiate(raceStatusPrefab) as GameObject;
        RaceStatus rs = go.GetComponent<RaceStatus>();
        rs.Init(t, c);
        t.car_name = c.car_name;
        go.transform.SetParent(raceStatusPanel.transform);
        
        float height = raceStatusPanel.transform.childCount * raceStatusHeight;
        raceStatusPanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,  height);
        raceStatusPanel.anchoredPosition = new Vector3(8.0f, -1 * height, 0.0f);      
    }

    public void RemoveLapTimer(LapTimer t)
    {
        if(raceStatusPanel == null)
            return;

        int count = raceStatusPanel.transform.childCount;
        for(int i = 0; i < count; i++)
        {
            Transform child = raceStatusPanel.transform.GetChild(i);
            RaceStatus rs = child.GetComponent<RaceStatus>();
            if(rs.timer == t)
            {
                Destroy(child.gameObject);
                float height = (count - 1) * raceStatusHeight;
                raceStatusPanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,  height);
                raceStatusPanel.anchoredPosition = new Vector3(8.0f, -1 * height, 0.0f);
                Debug.Log("removed lap timer");
                return;
            }
        }

        Debug.LogError("failed to remove lap timer");
    }

    IEnumerator DoRaceBanner()
	{
        if(raceState.m_State == RaceState.RaceStage.Qualifying)
        {
            raceBannerText.text = "Go!";
            yield return new WaitForSeconds(2);
        }
        else
        {
            raceBannerText.text = "Let's Race!";
            yield return new WaitForSeconds(3);

            raceBannerText.text = "Ready?";
            yield return new WaitForSeconds(1);

            raceBannerText.text = "Set?";
            yield return new WaitForSeconds(1);

            raceBannerText.text = "Go!";
        }

        Car[] icars = GameObject.FindObjectsOfType<Car>();
        foreach(Car car in icars)
        {         
            car.blockControls = false;
        }

        tk.TcpCarHandler[] carHanders = GameObject.FindObjectsOfType<tk.TcpCarHandler>();
        foreach(tk.TcpCarHandler handler in carHanders)
        {
            handler.SendStartRaceMsg();
        }

        yield return new WaitForSeconds(2);

		raceBanner.SetActive(false);
        raceStatusPanel.gameObject.SetActive(true);
        raceCamSwitcher.gameObject.SetActive(true);
	}

    public void OnCarOutOfBounds(GameObject car)
    {
        LapTimer status = car.transform.GetComponentInChildren<LapTimer>();

        if(status != null)
        {
            string msg = System.String.Format("{0} out of bounds!", status.car_name);
            SetStatus(msg);
        }
        
        OnCarDQ(car, false);
    }

    public void OnCarDQ(GameObject car, bool missedCheckpoint)
    {
        LapTimer status = car.transform.GetComponentInChildren<LapTimer>();

        if(status != null)
        {    
            status.OnDisqualified();

            Competitor c = GetCompetitorbyCarName(status.car_name);

            if(c != null)
                c.OnDQ(missedCheckpoint);
        }

        GameObject body = CarSpawner.getChildGameObject(car, "body");
    
        if(body != null)
            RemoveCarFromCheckpoints(body);
    }

    public void OnCarCrosStartLine(GameObject car)
    {
        LapTimer status = car.transform.GetComponentInChildren<LapTimer>();

        float lapTime = 0.0f;

        if (status.GetNumLapsCompleted() > 0)
            lapTime = status.GetLapTimeSec(status.GetNumLapsCompleted() - 1);

        tk.TcpCarHandler handler = car.GetComponentInChildren<tk.TcpCarHandler>();

        if (handler)
            handler.SendCrosStartRaceMsg(lapTime);
    }

    public void OnHitStartLine(GameObject body)
    {
        float delay = raceState.m_CheckPointDelay;
        int iCh = 1;

        Transform car = body.transform.parent;
        LapTimer[] status = car.GetComponentsInChildren<LapTimer>();

        if(status.Length == 1 && body.name == "body")
        {
            if(status[0].GetNumLapsCompleted() == race_num_laps && 
                raceState.m_State != RaceState.RaceStage.Practice)
            {
                //No need to register any more checkpoints.
                status[0].OnRaceCompleted();
                string msg = System.String.Format("{0} finished in {1}!", status[0].car_name, status[0].GetBestLapTimeSec().ToString("00.00"));
                SetStatus(msg);

                foreach(RaceCheckPoint cp in checkPoints)
                {
                    cp.RemoveBody(body);
                }                
            }
            else
            {
                foreach (RaceCheckPoint cp in checkPoints)
                {
                    cp.AddRequiredHit(body, delay);
                    iCh += 1;
                    delay = iCh * raceState.m_CheckPointDelay;
                }
            }
        }
    }

    public void OnHitCheckPoint(GameObject body, int iCheckPoint)
    {
        Transform car = body.transform.parent;
        LapTimer status = car.transform.GetComponentInChildren<LapTimer>();
        if(status != null)
        {
            string msg = System.String.Format("{0} hit checkpoint {1} at {2:F2}!", status.car_name, iCheckPoint, status.GetCurrentLapTimeSec());
            SetStatus(msg);
        }
    }

    public void OnCheckPointTimedOut(GameObject body)
    {
        Transform car = body.transform.parent;
        LapTimer status = car.GetComponentInChildren<LapTimer>();
        
        if(status != null)
        {
            string msg = System.String.Format("{0} failed to hit next checkpoint!", status.car_name);
            SetStatus(msg);
        }

        OnCarDQ(car.gameObject, true);
    }

    public void RemoveCarFromCheckpoints(GameObject body)
    {
        foreach (RaceCheckPoint ch in checkPoints)
            ch.RemoveBody(body);
    }

    bool IsRaceOver()
    {
        if(!bRaceActive)
            return false;

        LapTimer[] timers = GameObject.FindObjectsOfType<LapTimer>();

        if(timers is null || timers.Length == 0)
            return true;

        foreach(LapTimer t in timers)
        {
            if(!t.IsDisqualified())
            {
                if(t.GetNumLapsCompleted() < race_num_laps)
                {
                    return false;
                }
            }
        }

        return true;
    }

    void DoRaceSummary()
    {
        if(raceStatusPanel)
            raceStatusPanel.gameObject.SetActive(false);

        if(raceCamSwitcher)
        {
            raceCamSwitcher.OnDynamicCams();
        }

        if(raceSummary)
        {
            raceSummary.gameObject.SetActive(true);
            raceSummary.Init();
        }
    }

    void DoFinalRaceSummary()
    {
        if (raceSummary)
        {
            raceSummary.gameObject.SetActive(true);

            Competitor firstPlace;
            Competitor secondPlace;
            Competitor thirdPlace;

            if (raceState.m_Stage2c_final.Count == 0)
                return;

            Pairing final = raceState.m_Stage2c_final[0];

            if (final.time1 < final.time2)
            {
                firstPlace = GetCompetitorbyName(final.name1);
                secondPlace = GetCompetitorbyName(final.name2);
                if (firstPlace == null || secondPlace == null)
                    return;
                firstPlace.best_stage2_time = final.time1;
                secondPlace.best_stage2_time = final.time2;
            }
            else
            {
                firstPlace = GetCompetitorbyName(final.name2);
                secondPlace = GetCompetitorbyName(final.name1);
                if (firstPlace == null || secondPlace == null)
                    return;

                firstPlace.best_stage2_time = final.time2;
                secondPlace.best_stage2_time = final.time1;
            }

            firstPlace.stage2_place = 1;
            secondPlace.stage2_place = 2;
            string thirdPlaceCandidate1;
            float thirdPlaceTimeCand1 = 0.0f;
            string thirdPlaceCandidate2;
            float thirdPlaceTimeCand2 = 0.0f;

            List<Competitor> finalPlace = new List<Competitor>();
            finalPlace.Add(firstPlace);
            finalPlace.Add(secondPlace);

            if (raceState.m_Stage2b_2pairs.Count == 2)
            {
                Pairing p = raceState.m_Stage2b_2pairs[0];
                thirdPlaceCandidate1 = (p.name1 == final.name1 || p.name1 == final.name2) ? p.name2 : p.name1;
                thirdPlaceTimeCand1 = (p.name1 == final.name1 || p.name1 == final.name2) ? p.time2 : p.time1;

                p = raceState.m_Stage2b_2pairs[1];
                thirdPlaceCandidate2 = (p.name1 == final.name1 || p.name1 == final.name2) ? p.name2 : p.name1;
                thirdPlaceTimeCand2 = (p.name1 == final.name1 || p.name1 == final.name2) ? p.time2 : p.time1;

                thirdPlace = GetCompetitorbyName(thirdPlaceTimeCand1 < thirdPlaceTimeCand2 ? thirdPlaceCandidate1 : thirdPlaceCandidate2);
                thirdPlace.stage2_place = 3;
                thirdPlace.best_stage2_time = thirdPlaceTimeCand1 < thirdPlaceTimeCand2 ? thirdPlaceTimeCand1 : thirdPlaceTimeCand2;
                
                finalPlace.Add(thirdPlace);
            }



            raceSummary.InitFinal(finalPlace);
        }
    }

    void DoLineupSummary()
    {
         if(raceSummary.gameObject.activeInHierarchy)
            raceSummary.gameObject.SetActive(false);

        if(!lineupIntroPanel.gameObject.activeInHierarchy)
        {
            Pairing p = GetCurrentPairing();
            
            if(p != null)
                lineupIntroPanel.SetResult(p);

            lineupIntroPanel.gameObject.SetActive(true);
        }
    }

    public void DropRacers()
    {
        tk.JsonTcpClient[] clients = GameObject.FindObjectsOfType<tk.JsonTcpClient>();

        foreach(tk.JsonTcpClient client in clients)
        {
            client.Drop();
        }
    }

    public void ShowRacerBio(Competitor c)
    {
        racerBioPanel.gameObject.SetActive(true);
        racerBioPanel.SetBio(c.racerBio);
        StartCoroutine(HideRacerBio());
    }

    public IEnumerator HideRacerBio()
    {
        yield return new WaitForSeconds(raceState.m_RacerBioDisplayTime);

        racerBioPanel.gameObject.SetActive(false);
    }

    internal void OnRacerInfo(Competitor competitor)
    {
        UnityMainThreadDispatcher.Instance().Enqueue(SetRacerInfo(competitor));
    }

    public void AddCompetitor(Competitor c)
    {
        if(GetCompetitorbyName(c.racer_name) != null)
            Debug.LogError("Shouldn't be adding racer twice! " + c.racer_name);

        Debug.Log("Adding new competitor: " + c.racer_name);
        raceState.m_Competitors.Add(c);
    }
    
    IEnumerator SetRacerInfo(Competitor competitor)
    {
        m_TempCompetitors.Remove(competitor);

        Competitor c = GetCompetitorbyName(competitor.racer_name);

        if(c == null)
        {
            if(raceState.m_State == RaceState.RaceStage.Practice)
                ShowRacerBio(competitor);

            // Can't add competitors during the race.
            if (raceState.m_State <= RaceState.RaceStage.Qualifying)
            {
                // Only add the competitor once we have their full info.
                AddCompetitor(competitor);
                AddCompetitorDisplay(competitor);
            }            
        }
        else
        {
            c.SetClient(competitor.client);

            //Set the car config if we don't have it.
            if (c.carConfig == null && competitor.carConfig != null)
                c.carConfig = competitor.carConfig;
        }

        yield return null;
    }
}
