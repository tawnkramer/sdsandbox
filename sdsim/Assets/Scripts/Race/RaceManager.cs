using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Competitor
{
    public string car_name;
    public string racer_name;
    public string country;
    public string info;

    public int stage1_place;
    public float qual_time;
    public float best_stage1_time;
    public List<float> stage1_lap_times;
    public List<float> stage2_lap_times;
}

public struct Pairing
{
    public string name1;
    public string name2;
    public float time1;
    public float time2;
}

public struct RaceState
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

    public RaceStage   m_State;
    public float m_TimeInState;
    public List<Competitor> m_Competitors;
    public string currentQual;
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
}

public class RaceManager : MonoBehaviour
{
    List<GameObject> cars = new List<GameObject>();
    public GameObject raceBanner;
    public TMP_Text raceBannerText;
    public GameObject[] objToDisable;
    public CameraSwitcher[] camSwitchers;
    public GameObject raceStatusPrefab;
    public RectTransform raceStatusPanel;

    public RaceSummary raceSummary;

    public RaceCamSwitcher raceCamSwitcher;

    public bool bRaceActive = false;
    RaceState raceState;

    int raceStatusHeight = 100;

    public int race_num_laps = 2;

    void Start()
    {
        raceState.m_State = RaceState.RaceStage.None; 
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
    }

    public void SetStatus(string msg)
    {
        //set scrolling text status
    }

    void OnStateChange()
    {
        switch(raceState.m_State)
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
        SetStatus("This is practice time. Each competitor should verify code in working order, and select a startegy for race.");
    }

    void OnPracticeUpdate()
    {
        if(raceState.m_TimeInState > raceState.m_PracticeTime)
        {
            raceState.m_State = RaceState.RaceStage.Qualifying;
        }
    }

    void OnQualStart() 
    {
        SetStatus("This is qualification time. Each competitor must complete one AI lap to qualify for the race.");
    }

    void OnQualUpdate() 
    {
        if(raceState.currentQual == "None")
        {
            Competitor c = GetNextCompetitor();

            if(c != null)
            {
                //put car at start line and let them go.
                raceState.currentQual = c.car_name;
                raceState.m_CurrentQualElapsed = 0.0f;
            }    
        }
        else
        {
            raceState.m_CurrentQualElapsed += Time.deltaTime;

            float timeLimitQual = 60.0f;

            if(raceState.m_CurrentQualElapsed > timeLimitQual)
            {
                //Boot current car.
                raceState.currentQual == "None";
                raceState.m_iQual += 1;
            }
            else if(IsRaceOver())
            {
                Competitor c = GetCompetitor(raceState.currentQual);
                c.qual_time = GetBestTime(raceState.currentQual);
            }
        }
    }

    void OnEventIntroStart(){}
    void OnEventIntroUpdate(){}

    void OnStage1PreRaceStart(){}
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

        Debug.Log("Adding lap timer.");
        GameObject go = Instantiate(raceStatusPrefab) as GameObject;
        RaceStatus rs = go.GetComponent<RaceStatus>();
        rs.Init(t, client);
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
        raceBannerText.text = "Let's Race!";
		yield return new WaitForSeconds(3);

        raceBannerText.text = "Ready?";
		yield return new WaitForSeconds(1);

        raceBannerText.text = "Set?";
		yield return new WaitForSeconds(1);

        raceBannerText.text = "Go!";

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

        DropRacers();
    }

    public void DropRacers()
    {
        tk.JsonTcpClient[] clients = GameObject.FindObjectsOfType<tk.JsonTcpClient>();

        foreach(tk.JsonTcpClient client in clients)
        {
            client.Drop();
        }
    }
}
