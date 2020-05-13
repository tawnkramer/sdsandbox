using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RacePairUI : MonoBehaviour
{
    public Text racer1;
    public Text racer2;

    public Text place1;
    public Text place2;

    public Text racer1time;
    public Text racer2time;

    public Color win_color;
    public Color loss_color;

    public void Start()
    {
        racer1time.text = "";
        racer2time.text = "";
    }

    public void SetRacers(string r1, string r2, int p1, int p2)
    {
        racer1.text = r1;
        racer2.text = r2;
        place1.text = System.String.Format("{0}.", p1);
        place2.text = System.String.Format("{0}.", p2);
    }

    public void SetTimes(float t1, float t2)
    {
        racer1time.text = System.String.Format("{0:F2}", t1);
        racer2time.text = System.String.Format("{0:F2}", t2);
    }

    public void SetWinner(bool firstWon)
    {
        racer1.color = firstWon ? win_color : loss_color;
        racer1time.color = firstWon ? win_color : loss_color;

        racer2.color = !firstWon ? win_color : loss_color;
        racer2time.color = !firstWon ? win_color : loss_color;
    }
}
