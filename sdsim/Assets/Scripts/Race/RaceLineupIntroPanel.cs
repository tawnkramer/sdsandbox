using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RaceLineupIntroPanel : MonoBehaviour
{
    public RectTransform infoParent;
    public GameObject racePairPrefab;

    List<RacePairUI> pair_ui = new List<RacePairUI>();
    public void Init(List<Pairing> pairs)
    {
        RaceManager rm = GameObject.FindObjectOfType<RaceManager>();

        foreach (Pairing p in pairs)
        {
            GameObject go = Instantiate(racePairPrefab) as GameObject;
            RacePairUI ui = go.GetComponent<RacePairUI>();
            ui.transform.SetParent(infoParent.transform);
            Competitor a = rm.GetCompetitorbyName(p.name1);
            Competitor b = rm.GetCompetitorbyName(p.name2);

            ui.SetRacers(p.name1, p.name2, a.qual_place, b.qual_place);
            pair_ui.Add(ui);
        }
    }

    public void SetResult(Pairing p)
    {
        foreach(RacePairUI ui in pair_ui)
        {
            if(ui.racer1.text == p.name1)
            {
                ui.SetWinner(p.time1 < p.time2);
                ui.SetTimes(p.time1, p.time2);
            }
        }
    }
}
