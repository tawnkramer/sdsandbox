using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CarConfig : MonoBehaviour
{
    public GameObject bodyStyles;
    public GameObject car_name_base;
    public TextMesh car_name_text;
    public Timer timer;

    public void SetStyle(string body_style, int r, int g, int b, string car_name, int font_size)
    {
        Debug.Log("Setting car config.");
        
        if(car_name.Length > 1)
        {
            car_name_base.SetActive(true);
            car_name_text.text = car_name;
            car_name_text.fontSize = font_size;
            transform.name = car_name; // rename the gameobject name
        }
        
        if(timer != null)
        {
            timer.racerName = car_name;
        }
        else
        {
            Debug.LogError("timer not found while mapping racer name");
        }

        Color col = new Color();
        col.r = r / 255.0f;
        col.g = g / 255.0f;
        col.b = b / 255.0f;

        GameObject bodyStyle;
        for(int i = 0; i < bodyStyles.transform.childCount; i++) // go through each bodyStyles to find the desired body style
        {   
            bodyStyle = bodyStyles.transform.GetChild(i).gameObject;
            if (bodyStyle.name == body_style) // check if it's the requested body style
            {   
                bodyStyle.SetActive(true);
                var carBodyStyle = bodyStyle.GetComponent<CarBodyStyle>();
                if (carBodyStyle != null)
                {
                    carBodyStyle.ApplyColor(col);
                }
                else
                {
                    Debug.LogWarning(bodyStyle.name+" doesn't have a CarBodyStyle component");
                }
                return;
            }
            else // else just make sure it's disabled
            {
                bodyStyle.SetActive(false);
            }
        }
        Debug.LogWarning("body_style not in list of available bodyStyles, using default one instead");

        GameObject defaultBodyStyle = bodyStyles.transform.GetChild(0).gameObject;
        var defaultCarBodyStyle = defaultBodyStyle.GetComponent<CarBodyStyle>();
        if (defaultCarBodyStyle != null)
        {
            defaultBodyStyle.SetActive(true);
            defaultCarBodyStyle.ApplyColor(col);
        }
    }
}
