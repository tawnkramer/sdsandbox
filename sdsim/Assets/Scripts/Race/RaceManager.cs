using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using tk;

[Serializable]
public class Competitor
{
    public Competitor(JsonTcpClient _client, RaceManager _raceMan)
    {
        raceMan = _raceMan;
        has_car = false;
        SetClient(_client);
        client.dispatcher.Register("racer_info", new tk.Delegates.OnMsgRecv(OnRacerInfo));
        client.dispatcher.Register("car_config", new tk.Delegates.OnMsgRecv(OnCarConfig));

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

    public RaceManager raceMan;
    public string car_name;
    public string racer_name;
    public string country;
    public string info;
    public bool has_car;
    public bool got_qual_attempt = false;
    public JsonTcpClient client;
    public JSONObject carConfig;
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
        Debug.Log("Got racer info");
        racerBio = json;

        car_name = json.GetField("car_name").str;
        racer_name = json.GetField("racer_name").str;
        country = json.GetField("country").str;
        info = json.GetField("bio").str;
        
        raceMan.OnRacerInfo(this);
    }

    public void OnCarConfig(JSONObject json)
    {
        Debug.Log("Got car config message");
        carConfig = json;

        //raceMan.AddCompetitor(this);
    }
}

[Serializable]
public class Pairing
{
    public string name1;
    public string name2;
    public float time1;
    public float time2;
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
        EventIntro,     // Twitch feed begins. Race intro. Competitor list shown
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
        m_Competitors = new List<Competitor>();
        m_Stage1Order = new List<Pairing>();
        m_Stage2a_4pairs = new List<Pairing>();
        m_Stage2b_2pairs = new List<Pairing>();
        m_Stage2c_final = new List<Pairing>();
    }

    public RaceStage   m_State;
    public float m_TimeInState;
    public float m_TimeDelay;
    public List<Competitor> m_Competitors;
    public string m_CurrentQualifier;
    public int m_iQual;
    public float m_CurrentQualElapsed;
    public List<Pairing> m_Stage1Order;
    public int m_Stage1Next;  // index of next competitors
    public List<Pairing> m_Stage2a_4pairs;
    public List<Pairing> m_Stage2b_2pairs;
    public List<Pairing> m_Stage2c_final;
    public int m_Stage2Next;  // index of next pairing

    //constants
    public float m_CheckPointDelay;
    public float m_PracticeTime;
    public float m_QualTime;
    public float m_IntroTime;
    public float m_TimeLimitQual;
    public float m_BetweenStageTime;
    public float m_TimeLimitRace;

    public float m_RacerBioDisplayTime;

    public float m_TimeToShowRaceSummary;
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

    public bool bRaceActive = false;
    public bool bDevmode = false;
    public RaceState raceState;

    public int raceStatusHeight = 100;
    int raceCompetitorHeight = 50;

    public int race_num_laps = 2;
    public float dq_time = 10000.0f;

    List<Competitor> m_TempCompetitors = new List<Competitor>();

    void Start()
    {
        raceState = new RaceState();
        raceState.m_State = RaceState.RaceStage.None;
        raceState.m_PracticeTime = 60.0f * 10; //seconds
        raceState.m_QualTime = 30.0f; //seconds
        raceState.m_IntroTime = 20.0f;
        raceState.m_TimeLimitQual = 60.0f;
        raceState.m_TimeDelay = 0.0f;
        raceState.m_TimeToShowRaceSummary = 10.0f;
        raceState.m_BetweenStageTime = 5.0f;
        raceState.m_TimeLimitRace = 120.0f;
        raceState.m_CheckPointDelay = 20.0f;
        raceState.m_RacerBioDisplayTime = 10.0f;

        if (bDevmode)
            StartDevMode();        
    }

    void Update()
    {
        RaceState.RaceStage prev = raceState.m_State;
        
        OnStateUpdate();

        if(raceState.m_State != prev)
        {
            raceState.m_TimeInState = 0.0f;
            OnStateChange();
        }
        else
        {
            raceState.m_TimeInState += Time.deltaTime;
        }

        if (bDevmode)
            UpdateDevMode();
    }

    void StartDevMode()
    {
        SandboxServer server = GameObject.FindObjectOfType<SandboxServer>();

        int numDebugRacers = 10;

        for(int iRacer = 0; iRacer < numDebugRacers; iRacer++)
        {
            server.MakeDebugClient();

            JSONObject json = new JSONObject();
            json.AddField("car_name", "ai_car" + iRacer.ToString());
            json.AddField("racer_name", "ai_" + iRacer.ToString());
            json.AddField("bio", "I am a racer");
            json.AddField("country", "USA");
            raceState.m_Competitors[iRacer].OnRacerInfo(json);

            json = new JSONObject();
            json.AddField("body_style", "car01");
            json.AddField("body_r", "10");
            json.AddField("body_g", "150");
            json.AddField("body_b", "20");
            json.AddField("car_name", "ai_car" + iRacer.ToString());
            raceState.m_Competitors[iRacer].OnCarConfig(json);
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
                c.client.dispatcher.Dipatch("control", json);
            }
        }

        bool raceOverQuick = false;
        float raceTargetTime = 3.0f;

        // force Qualifying time quickly.
        if ((raceState.m_State == RaceState.RaceStage.Qualifying ||
            raceState.m_State == RaceState.RaceStage.Stage1Race ||
            raceState.m_State == RaceState.RaceStage.Stage2Race) && raceOverQuick)
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
    }

    void OnPracticeStart()
    {
        SetStatus("This is practice time.\n Each competitor should verify code in working order, and select a strategy for race.");
    }

    void OnPracticeUpdate()
    {
        if(raceState.m_TimeInState > raceState.m_PracticeTime)
        {
            raceState.m_State = RaceState.RaceStage.Qualifying;
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

            if(c.carConfig)
                c.client.dispatcher.Dipatch("car_config", c.carConfig);
        }
    }

    void OnQualStart() 
    {
        SetStatus("This is qualification time. Each competitor must complete one AI lap to qualify for the race.");

        RemoveAllCars();

        raceState.m_CurrentQualifier = "None";
        raceState.m_iQual = 0;
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

        if(DoDelay())
        {
            //mainly to allow people to read status.
        }
        else if(raceState.m_CurrentQualifier == "None")
        {
            Competitor c = GetNextQualifier();

            while(c == null)
            {
                raceState.m_iQual += 1;
                if(raceState.m_iQual == raceState.m_Competitors.Count)
                {
                    raceState.m_iQual = 0;
                    break;
                }

                c = GetNextQualifier();
            }

            if(c != null)
            {
                Debug.Log("Starting qual for " + c.racer_name);
                SetStatus("Starting qualification lap for " + c.racer_name);

                CreateCarFor(c);
                //put car at start line and let them go.
                raceState.m_CurrentQualifier = c.racer_name;
                raceState.m_CurrentQualElapsed = 0.0f;
                c.got_qual_attempt = true;
                OnResetRace();
                StartRace();

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
                string msg = System.String.Format("Qualifying run over time limit of {0} seconds for {1}.", raceState.m_TimeLimitQual, c.racer_name);
                Debug.Log(msg);
                SetStatus(msg);
                //Boot current car.
                raceState.m_CurrentQualifier = "None";
                raceState.m_TimeDelay = 3.0f;
            }
            else if(IsRaceOver())
            {
                if(lt.IsDisqualified())
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

                int numQualified = GetNumQualified();

                if (numQualified == raceState.m_Competitors.Count)
                {
                    raceState.m_TimeInState = raceState.m_QualTime;
                }
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
        if (raceState.m_TimeInState > raceState.m_IntroTime / 2.0f)
        {
            raceIntroPanel.SetActive(false);
            lineupIntroPanel.gameObject.SetActive(true);
        }

        if (raceState.m_TimeInState > raceState.m_IntroTime)
        {
            raceState.m_State = RaceState.RaceStage.Stage1PreRace;
            lineupIntroPanel.gameObject.SetActive(false);
            raceIntroGroup.SetActive(false);
            AnnounceDueUp();
        }

        SetTimerDisplay(raceState.m_IntroTime - raceState.m_TimeInState);
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
        if (x.qual_time == 0.0f)
            return 1;

        if (y.qual_time == 0.0f)
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

    void PrepareStage1Pairings()
    {
        raceState.m_Stage1Next = 0;
        raceState.m_Stage1Order = new List<Pairing>();
        raceState.m_Competitors.Sort(QualTimeSort);


        for(int i = 0; i < raceState.m_Competitors.Count / 2; i++)
        {
            Pairing p = new Pairing();
            Competitor a = raceState.m_Competitors[i];
            p.name1 = a.racer_name;
            p.time1 = 0.0f;
            p.time2 = 0.0f;

            int iB = raceState.m_Competitors.Count - i - 1;

            if(iB > i)
            {
                Competitor b = raceState.m_Competitors[iB];
                p.name2 = b.racer_name;
            }
            else
            {
                p.name2 = "solo";
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
        raceState.m_Competitors.Sort(Stage1TimeSort);
        List<Competitor> stage2Competitors = new List<Competitor>();

        //only the top 8 go through.
        for (int i = 0; i < raceState.m_Competitors.Count && i < 8; i++)
            stage2Competitors.Add(raceState.m_Competitors[i]);

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

        for (int i = 0; i < stage2Competitors.Count / 2; i++)
        {
            Pairing p = new Pairing();
            Competitor a = stage2Competitors[i];
            p.name1 = a.racer_name;
            p.time1 = 0.0f;
            p.time2 = 0.0f;

            int iB = stage2Competitors.Count - i - 1;

            if(iB > i)
            {
                Competitor b = stage2Competitors[iB];
                p.name2 = b.racer_name;
            }
            else
            {
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
            string dueUp = System.String.Format("Due up: {0} vs {1}", p.name1, p.name2);
            SetStatus(dueUp);
        }
    }

    void OnStage1PreRaceStart()
    {
        AnnounceDueUp();

        raceCompetitorPanel.gameObject.SetActive(true);

        Pairing p = GetCurrentPairing();

        if(p == null)
        {
            Debug.LogWarning("No competitors!");
            SetStatus("No competitors found.");
            raceState.m_State = RaceState.RaceStage.Stage1Completed;
            return;
        }

        Competitor c = GetCompetitorbyName(p.name1);

        if(c != null)
            CreateCarFor(c);

        c = GetCompetitorbyName(p.name2);

        if (c != null)
            CreateCarFor(c);

        OnResetRace();
    }

    void OnStage1PreRaceUpdate()
    {
        if(raceState.m_TimeInState > raceState.m_BetweenStageTime)
        {
            raceState.m_State = RaceState.RaceStage.Stage1Race;
        }

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

        if(IsRaceOver())
        {
            raceState.m_State = RaceState.RaceStage.Stage1PostRace;
        }

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

        yield return new WaitForSeconds(3.0f);

        racerBioPanel.gameObject.SetActive(true);
        racerBioPanel.SetBio(c1.racerBio);

        yield return new WaitForSeconds(raceState.m_RacerBioDisplayTime);

        racerBioPanel.gameObject.SetActive(false);
    }

    void OnStage1PostRaceStart()
    {
        Pairing p = GetCurrentPairing();
        Competitor a = GetCompetitorbyName(p.name1);
        Competitor b = GetCompetitorbyName(p.name2);
        p.time1 = GetBestTime(a.car_name);
        p.time2 = GetBestTime(b.car_name);
        a.best_stage1_time = p.time1;
        b.best_stage1_time = p.time2;
        LapTimer ta = GetLapTimer(a.car_name);
        LapTimer tb = GetLapTimer(b.car_name);

        if(ta.IsDisqualified())
        {
            p.time1 = dq_time;
        }

        if (tb.IsDisqualified())
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
            if(DidAllRacersDQ())
            {
                SetStatus("All racers DQ'ed. Restarting race!");
            }
            else
            {
                raceState.m_Stage1Next += 1; //needs to hit limit and go to stage 2.
            }

            lineupIntroPanel.gameObject.SetActive(false);

            if (raceState.m_Stage1Next >= raceState.m_Stage1Order.Count)
            {
                raceState.m_State = RaceState.RaceStage.Stage1Completed;
            }
            else
            {
                raceState.m_State = RaceState.RaceStage.Stage1PreRace;
            }
        }

        SetTimerDisplay(raceState.m_TimeToShowRaceSummary - raceState.m_TimeInState);
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
        if(raceState.m_Stage2a_4pairs.Count == 0 && numRacers == 8)
        {
            raceState.m_Stage2a_4pairs = PreparePairingsFromStage1();
            racerLadder.Init(raceState.m_Stage2a_4pairs, 0);
        }
        else if (raceState.m_Stage2b_2pairs.Count == 0)
        {
            if (numRacers == 8 && NumFinishedPairs(raceState.m_Stage2a_4pairs) == 4)
            {
                raceState.m_Stage2b_2pairs = PrepareStage2bPairings(raceState.m_Stage2a_4pairs);
                racerLadder.Init(raceState.m_Stage2b_2pairs, 1);
            }
            else if (numRacers == 4)
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

        Competitor c = GetCompetitorbyName(p.name1);

        if (c != null)
            CreateCarFor(c);

        c = GetCompetitorbyName(p.name2);

        if (c != null)
            CreateCarFor(c);

        OnResetRace();
    }

    void OnStage2PreRaceUpdate()
    {
        if (raceState.m_TimeInState > raceState.m_BetweenStageTime)
        {
            racerLadder.gameObject.SetActive(false);
            raceState.m_State = RaceState.RaceStage.Stage2Race;
        }

        SetTimerDisplay(raceState.m_BetweenStageTime - raceState.m_TimeInState);
    }

    void OnStage2RaceStart()
    {
        StartRace();
    }

    void OnStage2RaceUpdate()
    {
        if (IsRaceOver())
        {
            raceState.m_State = RaceState.RaceStage.Stage2PostRace;
        }

        SetTimerDisplay(raceState.m_TimeInState);
    }
    
    void OnStage2PostRaceStart()
    {
        Pairing p = GetCurrentPairing();
        Competitor a = GetCompetitorbyName(p.name1);
        Competitor b = GetCompetitorbyName(p.name2);
        p.time1 = GetBestTime(a.car_name);
        p.time2 = GetBestTime(b.car_name);
        LapTimer ta = GetLapTimer(a.car_name);
        LapTimer tb = GetLapTimer(b.car_name);

        if (ta.IsDisqualified())
        {
            p.time1 = dq_time;
        }

        if (tb.IsDisqualified())
        {
            p.time2 = dq_time;
        }

        DoRaceSummary();
        RemoveAllCars();

        if (DidAllRacersDQ())
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
        if (raceState.m_TimeInState > raceState.m_BetweenStageTime)
        {
            bool re_start = false;

            if (DidAllRacersDQ())
            {
                re_start = true;
            }
            else
            {
                raceState.m_Stage2Next += 1; //needs to hit limit and go to stage 2.
            }

            if (raceState.m_Stage2c_final.Count == 1 && !re_start)
            {
                raceState.m_State = RaceState.RaceStage.Stage2Complete;
            }
            else
            {
                raceState.m_State = RaceState.RaceStage.Stage2PreRace;
            }            
        }

        SetTimerDisplay(raceState.m_BetweenStageTime - raceState.m_TimeInState);
    }

    void OnStage2CompleteStart()
    {
        raceSummary.gameObject.SetActive(false);

        //Put up thanks dialog...
        // Trophy... final status..
        racerLadder.gameObject.SetActive(true);


        //Event is finished.
        SetStatus("Event is completed!");
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

    public void OnRaceStarted()
    {
         OnResetRace();
    }

    public void OnStopRace()
    {
        if(bRaceActive)
        {
            bRaceActive = false;
            DoRaceSummary();
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

            //keep them at the start line.
            car.blockControls = true;
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


        raceCamSwitcher.gameObject.SetActive(true);
        ResetRaceCams();
    }

    public void StartRace()
    {
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

		yield return new WaitForSeconds(2);

		raceBanner.SetActive(false);
        raceStatusPanel.gameObject.SetActive(true);
        raceCamSwitcher.gameObject.SetActive(true);
	}

    public void OnCarOutOfBounds(GameObject car)
    {
        LapTimer[] status = car.transform.GetComponentsInChildren<LapTimer>();

        foreach(LapTimer t in status)
        {
            t.OnDisqualified();
        }
    }

    public void OnHitStartLine(GameObject body)
    {
        float delay = raceState.m_CheckPointDelay;
        int iCh = 1;

        Transform car = body.transform.parent;
        LapTimer[] status = car.GetComponentsInChildren<LapTimer>();

        if(status.Length == 1)
        {
            if(status[0].GetNumLapsCompleted() == race_num_laps)
            {
                //No need to register any more checkpoints.
                status[0].OnRaceCompleted();
                string msg = System.String.Format("{0} finished in {1}!", status[0].car_name, status[0].GetBestLapTimeSec().ToString("00.00"));
                SetStatus(msg);

                foreach(RaceCheckPoint cp in checkPoints)
                {
                    cp.RemoveBody(body);
                }

                return;
            }
        }

        foreach(RaceCheckPoint cp in checkPoints)
        {
            cp.SetReqTime(delay);
            cp.AddRequiredHit(body);
            iCh += 1;
            delay = iCh * raceState.m_CheckPointDelay;
        }
    }

    public void OnHitCheckPoint(GameObject body)
    {
        Transform car = body.transform.parent;
        LapTimer[] status = car.GetComponentsInChildren<LapTimer>();
        if(status.Length == 1)
        {
            string msg = System.String.Format("{0} hit checkpoint at {1:F2}!", status[0].car_name, status[0].GetCurrentLapTimeSec());
            SetStatus(msg);
        }
    }

    public void OnCheckPointTimedOut(GameObject body)
    {
        Transform car = body.transform.parent;
        LapTimer[] status = car.GetComponentsInChildren<LapTimer>();
        if(status.Length == 1)
        {
            string msg = System.String.Format("{0} failed to hit next checkpoint!", status[0].car_name);
            SetStatus(msg);
            status[0].OnDisqualified();
        }
    }

    bool IsRaceOver()
    {
        if(!bRaceActive)
            return false;

        LapTimer[] timers = GameObject.FindObjectsOfType<LapTimer>();

        if(timers is null || timers.Length == 0)
            return true;

        //Needs some checkpoints!!!
        if (raceState.m_TimeInState > raceState.m_TimeLimitRace)
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

            Pairing final = raceState.m_Stage2c_final[0];

            if (final.time1 < final.time2)
            {
                firstPlace = GetCompetitorbyName(final.name1);
                secondPlace = GetCompetitorbyName(final.name2);
                firstPlace.best_stage2_time = final.time1;
                secondPlace.best_stage2_time = final.time2;
            }
            else
            {
                firstPlace = GetCompetitorbyName(final.name2);
                secondPlace = GetCompetitorbyName(final.name1);
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
            Debug.LogError("Shouldnt' be adding racer twice! " + c.racer_name);

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

            // Only add the competitor once we have their full info.
            AddCompetitor(competitor);
            AddCompetitorDisplay(competitor);
        }
        else
        {
            c.SetClient(competitor.client);
        }

        yield return null;
    }
}
