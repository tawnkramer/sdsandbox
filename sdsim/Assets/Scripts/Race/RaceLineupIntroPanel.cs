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
        foreach(Pairing p in pairs)
        {
            GameObject go = Instantiate(racePairPrefab) as GameObject;
            RacePairUI ui = go.GetComponent<RacePairUI>();
            ui.transform.SetParent(infoParent.transform);

            ui.SetRacers(p.name1, p.name2);
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
