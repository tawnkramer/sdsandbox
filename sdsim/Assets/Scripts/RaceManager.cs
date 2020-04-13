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

    public bool bRaceActive = false;

    int raceStatusHeight = 100;

    public void OnRaceStarted()
    {
        OnResetRace();
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

        camSwitchers[0].SwitchToThisCam();
    }

    public void OnResetRace()
    {
        bRaceActive = true;

        // disable these things that distract from the race.
        foreach(GameObject obj in objToDisable)
        {
            obj.SetActive(false);
        }

        //gather up all the cars.
        //cars = new List<GameObject>();
        Car[] icars = GameObject.FindObjectsOfType<Car>();
        foreach(Car car in icars)
        {
            car.RestorePosRot();

            //keep them at the start line.
            car.blockControls = true;
        }

        //Reset race status panels
        raceStatusPanel.gameObject.SetActive(false);

        LapTimer[] timers = GameObject.FindObjectsOfType<LapTimer>();
        foreach(LapTimer t in timers)
        {
            t.ResetRace();
        }

        raceBanner.SetActive(true);

        ResetRaceCams();
        
        StartCoroutine(DoRaceBanner());
    }

    public void AddLapTimer(LapTimer t, tk.JsonTcpClient client)
    {
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
		yield return new WaitForSeconds(2);

        raceBannerText.text = "Set?";
		yield return new WaitForSeconds(2);

        raceBannerText.text = "Go!";

        Car[] icars = GameObject.FindObjectsOfType<Car>();
        foreach(Car car in icars)
        {         
            car.blockControls = false;
        }

		yield return new WaitForSeconds(2);

		raceBanner.SetActive(false);
        raceStatusPanel.gameObject.SetActive(true);
	}

    public void OnCarOutOfBounds(GameObject car)
    {
        LapTimer[] status = car.transform.GetComponentsInChildren<LapTimer>();

        foreach(LapTimer t in status)
        {
            t.OnDisqualified();
        }
    }

}
