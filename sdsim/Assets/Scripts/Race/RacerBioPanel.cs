using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RacerBioPanel : MonoBehaviour
{
    public TMP_Text racer_name;
    public TMP_Text car_name;
    public Text bio;
    public Text country;

    public void SetBio(JSONObject b)
    {
        if(b == null)
        {
            gameObject.SetActive(false);
            return;
        }

        racer_name.text = b.GetField("racer_name").str;
        car_name.text = b.GetField("car_name").str;
        bio.text = b.GetField("bio").str;
        country.text = b.GetField("country").str;
    }
}
