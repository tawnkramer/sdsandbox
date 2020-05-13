using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaceLadderUI : MonoBehaviour
{
    public RectTransform stage2a;
    public RectTransform stage2b;
    public RectTransform stage2c;
    public GameObject racePairPrefab;

    List<RacePairUI> pair_ui0 = new List<RacePairUI>();
    List<RacePairUI> pair_ui1 = new List<RacePairUI>();
    List<RacePairUI> pair_ui2 = new List<RacePairUI>();

    public void Init(List<Pairing> pairs, int stage)
    {
        RectTransform r = stage2a;
        List<RacePairUI> pui = pair_ui0;

        if(stage == 1)
        {
            pui = pair_ui1;
            r = stage2b;
        }
        else if(stage == 2)
        {
            pui = pair_ui2;
            r = stage2c;
        }

        RaceManager rm = GameObject.FindObjectOfType<RaceManager>();

        foreach(Pairing p in pairs)
        {
            GameObject go = Instantiate(racePairPrefab) as GameObject;
            RacePairUI ui = go.GetComponent<RacePairUI>();
            ui.transform.SetParent(r.transform);
            Competitor a = rm.GetCompetitorbyName(p.name1);
            Competitor b = rm.GetCompetitorbyName(p.name2);

            ui.SetRacers(p.name1, p.name2, a.stage1_place, b.stage1_place);
            pui.Add(ui);
        }
    }

    public void SetResult(Pairing p, int stage)
    {
        RectTransform r = stage2a;
        List<RacePairUI> pui = pair_ui0;

        if(stage == 1)
        {
            pui = pair_ui1;
            r = stage2b;
        }
        else if(stage == 2)
        {
            pui = pair_ui2;
            r = stage2c;
        }

        foreach(RacePairUI ui in pui)
        {
            if(ui.racer1.text == p.name1)
            {
                ui.SetWinner(p.time1 < p.time2);
                ui.SetTimes(p.time1, p.time2);
            }
        }
    }
}
