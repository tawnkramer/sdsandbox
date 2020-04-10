using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaceManager : MonoBehaviour
{
    List<GameObject> cars = new List<GameObject>();

    public void ResetRace()
    {
        //gather up all the cars.
        cars = new List<GameObject>();
        Car[] icars = GameObject.FindObjectsOfType<Car>();
        foreach(Car iC in icars)
        {
            cars.Add(iC.gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
