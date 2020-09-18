using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RaceCompetitor : MonoBehaviour
{
    public Text racer_name; //Not car name  
    public Text info;
    public GameObject infoPanel;
    public Color online_color;
    public Color offline_color;

    public RaceState raceState;

    public Competitor comp;

    public void Init(Competitor c, RaceState r)
    {
        comp = c;
        raceState = r;
    }

    public void Update()
    {
        if (comp.racer_name.Length > 1)
        {
            racer_name.text = comp.racer_name;

            if (comp.IsOnline())
            {
                racer_name.color = online_color;
            }
            else
            {
                racer_name.color = offline_color;
            }
        }
    }
}
