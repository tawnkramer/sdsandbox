using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarBodyStyle : MonoBehaviour
{
    public GameObject[] bodyParts;
    public void ApplyColor(Color col)
    {
        foreach (GameObject part in bodyParts)
        {
            var rend = part.GetComponent<Renderer>();
            rend.material.SetColor("_Color", col);
        }
    }
}
