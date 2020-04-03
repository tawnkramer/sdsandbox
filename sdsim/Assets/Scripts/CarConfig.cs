using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CarConfig : MonoBehaviour
{
    public GameObject donkey_base_plate;
    public GameObject donkey_top_cage;
    public GameObject rc_car_body;
    public GameObject[] rc_car_parts;
    public GameObject car_name_base;
    public TextMesh car_name_text;

    public void SetStyle(string body_style, int r, int g, int b, string car_name, int font_size)
    {
        Debug.Log("Setting car config.");
        
        if(car_name.Length > 1)
        {
            car_name_base.SetActive(true);
            car_name_text.text = car_name;
            car_name_text.fontSize = font_size;
        }

        Color col = new Color();
        col.r = r / 255.0f;
        col.g = g / 255.0f;
        col.b = b / 255.0f;

        if(body_style == "bare")
        {
            donkey_top_cage.SetActive(false);
            var rend = donkey_base_plate.GetComponent<Renderer>();
            rend.material.SetColor("_Color", col);
        }
        else if(body_style == "car01")
        {
            donkey_base_plate.SetActive(false);
            donkey_top_cage.SetActive(false);
            rc_car_body.SetActive(true);

            foreach( GameObject part in rc_car_parts)
            {
                var rend = part.GetComponent<Renderer>();
                rend.material.SetColor("_Color", col);
            }
        }
        else
        {
            var rend = donkey_base_plate.GetComponent<Renderer>();
            rend.material.SetColor("_Color", col);

            rend = donkey_top_cage.GetComponent<Renderer>();
            rend.material.SetColor("_Color", col);
        }
    }
}
