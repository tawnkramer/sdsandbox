using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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

    int raceStatusHeight = 100;

    public int race_num_laps = 1;

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

    void Update()
    {
        if(IsRaceOver())
        {
            bRaceActive = false;
            DoRaceSummary();
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
