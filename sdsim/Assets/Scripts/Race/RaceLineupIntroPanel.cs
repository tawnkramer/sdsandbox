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
            Competitor a = rm.GetCompetitor(p.guid1);
            Competitor b = rm.GetCompetitor(p.guid2);

            if(b == null)
            {
                ui.SetRacers(a.car_name, "solo", a.qual_place, 0);
            }
            else
                ui.SetRacers(a.car_name, b.car_name, a.qual_place, b.qual_place);

            pair_ui.Add(ui);
        }
    }

    public void SetResult(Pairing p)
    {
        RaceManager rm = GameObject.FindObjectOfType<RaceManager>();
        string carName1 = rm.GetCompetitorCarName(p.guid1);
        foreach(RacePairUI ui in pair_ui)
        {
            if(ui.racer1.text == carName1)
            {
                ui.SetWinner(p.time1 < p.time2);
                ui.SetTimes(p.time1, p.time2);
            }
        }
    }
}
