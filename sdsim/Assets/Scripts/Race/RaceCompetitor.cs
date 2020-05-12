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

    Competitor comp;

    public void Init(Competitor c)
    {
        comp = c;
    }

    public void Update()
    {
        if (comp.racer_name.Length > 1)
        {
            racer_name.text = comp.racer_name;

            if (comp.is_online)
            {
                racer_name.color = online_color;
            }
            else
            {
                racer_name.color = offline_color;
            }
        }

        if (comp.qual_time != 0.0f && !infoPanel.activeInHierarchy)
        {
            infoPanel.SetActive(true);

//             int minutes = Mathf.FloorToInt(comp.qual_time / 60F);
//             int seconds = Mathf.FloorToInt(comp.qual_time - minutes * 60);
//             string niceTime = string.Format("{0:0}:{1:00}", minutes, seconds);

            info.text = System.String.Format("{0:F2}", comp.qual_time);
        }
    }
}
