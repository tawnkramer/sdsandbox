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
        client.dispatcher.Register("cam_config_b", new tk.Delegates.OnMsgRecv(OnCamConfigB));
        client.dispatcher.Register("lidar_config", new tk.Delegates.OnMsgRecv(OnLidarConfig));
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
    public string guid;
    public bool has_car;
    public bool got_qual_attempt = false;
    public bool dropped = false;

    public JSONObject carConfig;
    public JSONObject camConfig;
    public JSONObject camConfigB;
    public JSONObject lidarConfig;
    public JSONObject racerBio;

    public void OnRacerInfo(JSONObject json)
    {
        racerBio = json;

        car_name = json.GetField("car_name").str;
        racer_name = json.GetField("racer_name").str;
        country = json.GetField("country").str;
        info = json.GetField("bio").str;
        guid = json.GetField("guid").str;

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

    public void OnCamConfigB(JSONObject json)
    {
        if (racer_name != null)
            Debug.Log("Got cam config b for " + racer_name);
        camConfigB = json;
    }

    public void OnLidarConfig(JSONObject json)
    {
        if (racer_name != null)
            Debug.Log("Got lidar config for " + racer_name);
        lidarConfig = json;
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
public class RaceState
{
    public enum RaceStage
    {
        None,           // init
        Practice,       // 1 HR or more free runs, multiple competitors come and go.
    }

    public RaceState()
    {
        m_State = RaceState.RaceStage.None;
        m_PracticeTime = 100 * 60.0f * 30.0f; // 30 minutes
        m_QualTime = 60.0f * 30.0f; //30 minutes
        m_IntroTime = 60.0f * 5.0f;
        m_TimeLimitQual = 100.0f;
        m_TimeDelay = 0.0f;
        m_TimeLimitRace = 120.0f;
        m_CheckPointDelay = 40.0f;
        m_RacerBioDisplayTime = 15.0f;
        m_MinCompetitorCount = 3;
        m_RaceRestartsLimit = 3;
        m_RaceRestarts = 0;
        m_AnyCompetitorFinishALap = false;
        m_UseCheckpointDuringPractice = true;
        m_UsePracticeTimer = false;
        m_CurrentQualifier = "None";
        m_iQual = 0;

        m_Competitors = new List<Competitor>();
    }

    public RaceStage   m_State;
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
    public bool m_UseCheckpointDuringPractice;
    public bool m_UsePracticeTimer;

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

    // competitors
    public List<Competitor> m_Competitors;
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

    public RaceCheckPoint[] checkPoints;

    public RacerBioPanel racerBioPanel;
    public DropCompetitorUI dropCompetitorPanel;

    public GameObject raceControls;

    public bool bRaceActive = false;
    public bool bDevmode = false;
    RaceState raceState;

    public int raceStatusHeight = 100;
    int raceCompetitorHeight = 50;

    string removeRacerName = "";
    string removeRacerGuid = "";

    public int race_num_laps = 1000;
    public static float dq_time = 1000000.0f;
    public string race_state_filename = "default";
    int m_finishLineCheckpoint = 0;


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
            SetCheckpoinIndecies();
            if (bDevmode)
            StartDevMode();        
    }

    void SetCheckpoinIndecies()
    {
        for (int iCh = 0; iCh < checkPoints.Length; iCh++)
            checkPoints[iCh].SetCheckpointIndex(iCh);

        m_finishLineCheckpoint = checkPoints.Length - 1;
    }

   public void OnResetPressed()
  {
    Car[] cars = GameObject.FindObjectsOfType<Car>();
    for (int iCar = 0; iCar < cars.Length; iCar++)
    {
      Car car = cars[iCar];
      LapTimer t = car.transform.GetComponentInChildren<LapTimer>();
        car.RestorePosRot();
        t.ResetDisqualified();
        t.RestartCurrentLap();
        t.ResetRace();

        if (raceState.m_UseCheckpointDuringPractice)
        {
          GameObject body = CarSpawner.getChildGameObject(car.gameObject, "body");
          RemoveCarFromCheckpoints(body);
          Competitor c = GetCompetitor(t.guid);
          AddCarToStartLineCheckpoint(car.gameObject);
        }
      
    }
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
    
    public void ToggleRaceControls()
    {
        if(raceControls.activeInHierarchy)
            raceControls.SetActive(false);
        else
            raceControls.SetActive(true);
    }

  
    void HidePopUps()
    {
        raceCompetitorPanel.gameObject.SetActive(false);
        racerBioPanel.gameObject.SetActive(false);
        raceCamSwitcher.gameObject.SetActive(false);
        raceIntroGroup.SetActive(false);
        raceIntroPanel.SetActive(false);
        raceBanner.SetActive(false);
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
    }

    void StartDevMode()
    {
        SandboxServer server = GameObject.FindObjectOfType<SandboxServer>();

        int numDebugRacers = 1;

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
            json.AddField("guid", iRacer.ToString());
            json.AddField("country", "USA");
            c.OnRacerInfo(json);
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
        //raceInfoBar.SetStateName(msg);
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

            default:
                break;
        }
    }

    void OnPracticeStart()
    {
        SetStatus("Race will start with a reset");
    }

    void OnPracticeUpdate()
    {
        foreach (Competitor c in raceState.m_Competitors)
        {
            if(c.IsOnline() && !c.has_car)
            {
                Debug.Log("OnPracticeUpdate: making car for " + c.racer_name);
                GameObject carObj = CreateCarFor(c);

                if (carObj != null && raceState.m_UseCheckpointDuringPractice)
                {
                    Debug.Log("OnPracticeUpdate : add car to start checkpoint " + c.racer_name);
                    AddCarToStartLineCheckpoint(carObj);    
                }
            }            
        }

        PracticeResetRacerDQToStart();
    }

    public void PracticeResetRacerDQToStart()
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
                t.ResetRace();

                if(raceState.m_UseCheckpointDuringPractice)
                {
                    GameObject body = CarSpawner.getChildGameObject(car.gameObject, "body");
                    RemoveCarFromCheckpoints(body);
                    Competitor c = GetCompetitor(t.guid);
                    AddCarToStartLineCheckpoint(car.gameObject);
                }
            }
        }
    }

    void AddCarToStartLineCheckpoint(GameObject car)
    {
        RaceCheckPoint startCheckPoint = checkPoints[m_finishLineCheckpoint];
        float timeToHitStartline = 10.0f;

        GameObject body = CarSpawner.getChildGameObject(car.gameObject, "body");
        startCheckPoint.AddRequiredHit(body, timeToHitStartline);
    }

    public GameObject CreateCarFor(Competitor c)
    {
        if (c.has_car || c.carConfig == null || c.client == null)
            return null;

        CarSpawner spawner = GameObject.FindObjectOfType<CarSpawner>();

        if (spawner)
        {
            GameObject carObj = spawner.Spawn(c.client);
            c.has_car = true;
            c.client.dispatcher.Dispatch("car_config", c.carConfig);

            if (c.camConfig != null)
                c.client.dispatcher.Dispatch("cam_config", c.camConfig);

            if (c.camConfigB != null)
                c.client.dispatcher.Dispatch("cam_config_b", c.camConfigB);

            if (c.lidarConfig != null)
                c.client.dispatcher.Dispatch("lidar_config", c.lidarConfig);

            if(c.racerBio != null)
                c.client.dispatcher.Dispatch("racer_info", c.racerBio);

            Debug.Log("Creating car for " + c.racer_name);
            return carObj;
        }

        return null;
    }
  
    public Competitor GetCompetitor(string guid)
    {
        foreach (Competitor c in raceState.m_Competitors)
        {
            if (c.guid == guid)
                return c;
        }

        return null;
    }

    public string GetCompetitorRacerName(string guid)
    {
        foreach (Competitor c in raceState.m_Competitors)
        {
            if (c.guid == guid)
                return c.racer_name;
        }

        return "missing!";
    }

    public string GetCompetitorCarName(string guid)
    {
        foreach (Competitor c in raceState.m_Competitors)
        {
            if (c.guid == guid)
                return c.car_name;
        }

        return "missing!";
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

    public float GetBestTime(string guid)
    {
        LapTimer[] timers = GameObject.FindObjectsOfType<LapTimer>();

        foreach (LapTimer t in timers)
        {
            if (t.guid == guid)
                return t.GetBestLapTimeSec();
        }

        return 0.0f;
    }

    public LapTimer GetLapTimer(string guid)
    {
        LapTimer[] timers = GameObject.FindObjectsOfType<LapTimer>();

        foreach (LapTimer t in timers)
        {
            if (t.guid == guid)
                return t;
        }

        return null;
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
  
    public void OnCarOutOfBounds(GameObject car)
    {
        LapTimer status = car.transform.GetComponentInChildren<LapTimer>();

        if(status != null)
        {
            string msg = System.String.Format("{0} out of bounds!", status.car_name);
            SetStatus(msg);
        }

        GameObject body = CarSpawner.getChildGameObject(car, "body");
        RemoveCarFromCheckpoints(body);
        
        OnCarDQ(car, false);
    }

    // Usually from external Reset request from the car client.
    public void OnCarReset(GameObject car)
    {
        GameObject body = CarSpawner.getChildGameObject(car, "body");
        RemoveCarFromCheckpoints(body);

        if (raceState.m_State != RaceState.RaceStage.Practice || raceState.m_UseCheckpointDuringPractice )                
            AddCarToStartLineCheckpoint(body);
    }

    public void OnCarDQ(GameObject car, bool missedCheckpoint)
    {
        LapTimer status = car.transform.GetComponentInChildren<LapTimer>();

        if(status != null)
        {    
            status.OnDisqualified();

            Competitor c = GetCompetitor(status.guid);

            if(c != null)
                c.OnDQ(missedCheckpoint);
        }        
    }

    public void SendCrossStartMsg(GameObject car)
    {
        LapTimer status = car.transform.GetComponentInChildren<LapTimer>();

        float lapTime = 0.0f;

        if (status.GetNumLapsCompleted() > 0)
            lapTime = status.GetLapTimeSec(status.GetNumLapsCompleted() - 1);

        tk.TcpCarHandler handler = car.GetComponentInChildren<tk.TcpCarHandler>();

        if (handler)
            handler.SendCrosStartRaceMsg(lapTime);
    }

    public bool IsRaceCompleted(LapTimer t)
    {
        return t.GetNumLapsCompleted() == race_num_laps; 
    }


    public void OnHitCheckPoint(GameObject body, int iCheckPoint)
    {
        Transform car = body.transform.parent;
        LapTimer status = car.transform.GetComponentInChildren<LapTimer>();

         RaceCheckPoint currentCp = checkPoints[iCheckPoint];
         currentCp.RemoveBody(body);

        if (status != null)
        {
            string msg;

            if (iCheckPoint == m_finishLineCheckpoint)
            {
                bool starting = !status.HasCrossedStartLine();

                if (starting)
                {
                    msg = System.String.Format("{0} crossed start line!", status.car_name);
                }
                else
                {
                    msg = System.String.Format("{0} crossed finish line at {1:F2}!", status.car_name, status.GetCurrentLapTimeSec());
                }

                status.OnCollideFinishLine();
                SendCrossStartMsg(car.gameObject);
            }
            else
            {
                msg = System.String.Format("{0} hit checkpoint {1} at {2:F2}!", status.car_name, iCheckPoint + 1, status.GetCurrentLapTimeSec());
            }

            if(IsRaceCompleted(status))
            {
                status.OnRaceCompleted();
            }
            else
            {
                int iNextCheckpoint = (iCheckPoint + 1) % checkPoints.Length;
                RaceCheckPoint cp = checkPoints[iNextCheckpoint];
                cp.AddRequiredHit(body, raceState.m_CheckPointDelay);                
            }

            Debug.Log("OnHitCheckPoint: " + msg);
            SetStatus(msg);
        }
        else
        {
            SetStatus("Something wrong with checkpoints!");
        }
    }

    public void OnCheckPointTimedOut(GameObject body, int iCh)
    {
        Transform car = body.transform.parent;
        LapTimer status = car.GetComponentInChildren<LapTimer>();
        
        if(status != null)
        {
            string msg = System.String.Format("{0} failed to hit next checkpoint!", status.car_name);
            SetStatus(msg);
        }

        checkPoints[iCh].RemoveBody(body);

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
        if(GetCompetitor(c.guid) != null)
            Debug.LogError("Shouldn't be adding racer twice! " + c.racer_name);

        Debug.Log("Adding new competitor: " + c.racer_name);
        raceState.m_Competitors.Add(c);
    }
    
    IEnumerator SetRacerInfo(Competitor competitor)
    {
        m_TempCompetitors.Remove(competitor);

        Competitor c = GetCompetitor(competitor.guid);

        if(c == null)
        {
            if(raceState.m_State == RaceState.RaceStage.Practice)
                ShowRacerBio(competitor);

            // Can't add competitors during the race.

                // Only add the competitor once we have their full info.
                AddCompetitor(competitor);
                AddCompetitorDisplay(competitor);

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
