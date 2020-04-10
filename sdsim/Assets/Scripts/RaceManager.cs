using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class RaceManager : MonoBehaviour
{
    List<GameObject> cars = new List<GameObject>();
    public GameObject raceBanner;
    public TMP_Text raceBannerText;
    public GameObject[] objToDisable;

    public void OnRaceStarted()
    {
        OnResetRace();
    }

    public void OnResetRace()
    {
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

        LapTimer[] timers = GameObject.FindObjectsOfType<LapTimer>();
        foreach(LapTimer t in timers)
        {
            t.ResetRace();
        }

        raceBanner.SetActive(true);
        
        StartCoroutine(DoRaceBanner());
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
