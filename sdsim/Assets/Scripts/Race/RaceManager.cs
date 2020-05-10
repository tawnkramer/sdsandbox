using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using tk;

public class Competitor
{
    public Competitor(JsonTcpClient _client)
    {
        has_car = false;
        SetClient(_client);
        client.dispatcher.Register("racer_info", new tk.Delegates.OnMsgRecv(OnRacerInfo));
        client.dispatcher.Register("car_config", new tk.Delegates.OnMsgRecv(OnCarConfig));
    }

    public void SetClient(JsonTcpClient _client)
    {
        client = _client;
        is_online = true;
    }

    public string car_name;
    public string racer_name;
    public string country;
    public string info;
    public bool is_online;
    public bool has_car;
    public JsonTcpClient client;
    public JSONObject carConfig;

    public int stage1_place;
    public float qual_time;
    public float best_stage1_time;
    public List<float> stage1_lap_times;
    public List<float> stage2_lap_times;

    public void OnRacerInfo(JSONObject json)
    {
        Debug.Log("Got racer info");

        car_name = json.GetField("car_name").str;
        racer_name = json.GetField("racer_name").str;
        country = json.GetField("country").str;
        info = json.GetField("info").str;

        RaceManager raceMan = GameObject.FindObjectOfType<RaceManager>();
        raceMan.OnRacerInfo(this);
    }

    public void OnCarConfig(JSONObject json)
    {
        Debug.Log("Got car config message");
        carConfig = json;
    }
}

public struct Pairing
{
    public string name1;
    public string name2;
    public float time1;
    public float time2;
}

public class RaceState
{
    public enum RaceStage
    {
        None,           // init
        Practice,       // 1 HR or more free runs, multiple competitors come and go.
        Qualifying,     // For 30 min prior. All competitors must finish a lap.
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
    public float m_PracticeTime;
    public float m_QualTime;
    public float m_IntroTime;
    public float m_TimeLimitQual;
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

    public bool bRaceActive = false;
    public bool bDevmode = false;
    public RaceState raceState;

    public int raceStatusHeight = 100;
    int raceCompetitorHeight = 50;

    public int race_num_laps = 2;

    void Start()
    {
        raceState = new RaceState();
        raceState.m_State = RaceState.RaceStage.None;
        raceState.m_PracticeTime = 5.0f; //seconds
        raceState.m_QualTime = 20.0f; //seconds
        raceState.m_IntroTime = 10.0f;
        raceState.m_TimeLimitQual = 60.0f;

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
        server.MakeDebugClient();

        if (raceState.m_Competitors.Count == 1)
        {
            JSONObject json = new JSONObject();
            json.AddField("car_name", "test_car");
            json.AddField("racer_name", "Tawn Kramer");
            json.AddField("info", "I am a racer");
            json.AddField("country", "USA");
            raceState.m_Competitors[0].OnRacerInfo(json);
        }

        server.MakeDebugClient();

        if (raceState.m_Competitors.Count == 2)
        {
            JSONObject json = new JSONObject();
            json.AddField("car_name", "other_car");
            json.AddField("racer_name", "Not Tawn Kramer");
            json.AddField("info", "I am a ai racer");
            json.AddField("country", "Japan");
            raceState.m_Competitors[1].OnRacerInfo(json);
        }
    }

    void UpdateDevMode()
    {
        bool bTestDisconnect = false;
        // Test disconnect
        if (raceState.m_TimeInState > 3.0f && raceState.m_Competitors.Count == 2 && raceState.m_Competitors[1].is_online && bTestDisconnect)
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
        if (raceState.m_TimeInState > 5.0f && raceState.m_Competitors.Count == 2 && !raceState.m_Competitors[1].is_online && bTestDisconnect)
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
            if(c.is_online && c.has_car)
            {
                JSONObject json = new JSONObject();
                json.AddField("steering", "0.0");
                json.AddField("throttle", "0.2");
                json.AddField("brake", "0.0");
                c.client.dispatcher.Dipatch("control", json);
            }
        }

        // force Qualifying time quickly.
        if (raceState.m_State == RaceState.RaceStage.Qualifying && 
            raceState.m_CurrentQualElapsed >= 5.0f && 
            raceState.m_CurrentQualifier != "None")
        {
            Debug.Log("raceState.m_CurrentQualElapsed: " + raceState.m_CurrentQualElapsed.ToString());
            //Make race over quickly.
            LapTimer[] timers = GameObject.FindObjectsOfType<LapTimer>();

            foreach (LapTimer t in timers)
            {
                t.OnCollideFinishLine();
            }
        }
    }

    internal void OnClientJoined(JsonTcpClient client)
    {
        Competitor c = new Competitor(client);
        raceState.m_Competitors.Add(c);        
    }

    public void AddCompetitorDisplay(Competitor c)
    {
        if (raceCompetitorPrefab == null)
            return;

        Debug.Log("Adding race competitor display.");
        GameObject go = Instantiate(raceCompetitorPrefab) as GameObject;
        RaceCompetitor rs = go.GetComponent<RaceCompetitor>();
        rs.Init(c);
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
                c.is_online = false;
                c.client = null;
                return;
            }
        }

        Debug.Log("Competitor went off-line but was not found!");
    }

    public void RemoveAllCars()
    {
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

        foreach(Competitor c in raceState.m_Competitors)
        {
            if(c.is_online)
            {
                CreateCarFor(c);
            }
        }
    }

    public void CreateCarFor(Competitor c)
    {
        if (c.has_car)
            return;

        CarSpawner spawner = GameObject.FindObjectOfType<CarSpawner>();

        if (spawner)
        {
            Debug.Log("spawning car.");

            spawner.Spawn(c.client);
            c.has_car = true;
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
            if (c.qual_time == 0.0 && c.is_online)
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
                return t.GetBestLapTime();
        }

        return 1000.0f;
    }

    void OnQualUpdate() 
    {
        race_num_laps = 1;

        if(raceState.m_CurrentQualifier == "None")
        {
            Competitor c = GetNextQualifier();

            if(c == null)
            {
                //reset iQual and try again.
                raceState.m_iQual = 0;
                c = GetNextQualifier();
            }

            if(c != null)
            {
                Debug.Log("Starting qual for " + c.racer_name);
                CreateCarFor(c);
                //put car at start line and let them go.
                raceState.m_CurrentQualifier = c.racer_name;
                raceState.m_CurrentQualElapsed = 0.0f;
                OnResetRace();
            }    
        }
        else
        {
            Competitor c = GetCompetitorbyName(raceState.m_CurrentQualifier);

            if (c == null || !c.is_online)
                raceState.m_CurrentQualifier = "None";

            raceState.m_CurrentQualElapsed += Time.deltaTime;            

            if(raceState.m_CurrentQualElapsed > raceState.m_TimeLimitQual)
            {
                Debug.Log("Qual past time limit for " + c.racer_name);
                //Boot current car.
                raceState.m_CurrentQualifier = "None";
                raceState.m_iQual += 1;
            }
            else if(IsRaceOver())
            {
                Debug.Log("Qual finished for " + c.racer_name + " at " + raceState.m_CurrentQualElapsed);

                if (c != null)
                {
                    c.qual_time = raceState.m_CurrentQualElapsed;
                    RemoveCar(c);
                }

                raceState.m_CurrentQualifier = "None";
                raceState.m_iQual += 1;
            }
        }

        if (raceState.m_TimeInState > raceState.m_QualTime)
        {
            //Leave for the event intro state.
            raceState.m_State = RaceState.RaceStage.EventIntro;
        }

        SetTimerDisplay(raceState.m_QualTime - raceState.m_TimeInState);
    }

    void OnEventIntroStart()
    {
        RemoveAllCars();

        SetStatus("Welcome to Virtual Race League DIYRobocars online racing event!!");
    }

    void OnEventIntroUpdate()
    {
        if (raceState.m_TimeInState > raceState.m_IntroTime)
        {
            raceState.m_State = RaceState.RaceStage.Stage1PreRace;
        }

        SetTimerDisplay(raceState.m_IntroTime - raceState.m_TimeInState);
    }

    void OnStage1PreRaceStart()
    {

    }

    void OnStage1PreRaceUpdate(){}

    void OnStage1RaceStart(){}
    
    void OnStage1RaceUpdate()
    {
        if(IsRaceOver())
        {
            raceState.m_State = RaceState.RaceStage.Stage1PostRace;
        }
    }

    void OnStage1PostRaceStart()
    {
        DoRaceSummary();
    }

    void OnStage1PostRaceUpdate(){}

    void OnStage1CompletedStart(){}
    void OnStage1CompletedUpdate(){}

    void OnStage2PreRaceStart(){}
    void OnStage2PreRaceUpdate(){}

    void OnStage2RaceStart(){}
    void OnStage2RaceUpdate(){}

    void OnStage2PostRaceStart(){}
    void OnStage2PostRaceUpdate(){}

    void OnStage2CompleteStart(){}
    void OnStage2CompleteUpdate(){}

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

        raceCamSwitcher.gameObject.SetActive(true);
        ResetRaceCams();

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



    bool IsRaceOver()
    {
        if(!bRaceActive)
            return false;

        LapTimer[] timers = GameObject.FindObjectsOfType<LapTimer>();

        if(timers is null || timers.Length == 0)
            return false;

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

        // DropRacers();
    }

    public void DropRacers()
    {
        tk.JsonTcpClient[] clients = GameObject.FindObjectsOfType<tk.JsonTcpClient>();

        foreach(tk.JsonTcpClient client in clients)
        {
            client.Drop();
        }
    }

    internal void OnRacerInfo(Competitor competitor)
    {
        bool foundDuplicate = false;

        foreach(Competitor c in raceState.m_Competitors)
        {
            if (c == competitor)
                continue;

            if(c.racer_name == competitor.racer_name)
            {
                c.SetClient(competitor.client);
                raceState.m_Competitors.Remove(competitor);
                foundDuplicate = true;
                Debug.Log("removing previous client.");
                break;
            }
        }

        if(!foundDuplicate)
        {
            AddCompetitorDisplay(competitor);
        }       
    }
}
