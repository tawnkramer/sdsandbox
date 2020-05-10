using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RaceCompetitor : MonoBehaviour
{
    public Text racer_name; //Not car name  
    public Color online_color;
    public Color offline_color;

    Competitor comp;

    public void Init(Competitor c)
    {
        comp = c;
    }

    public void Update()
    {
        if(comp.racer_name.Length > 1)
        {
            racer_name.text = comp.racer_name;

            if(comp.is_online)
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
