using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarBodyStyle : MonoBehaviour
{
    public GameObject[] bodyParts;

    // sometimes, object can have multiple material, here we are specifying witch one we want to modify
    public string[] material_name;
    private int mat_idx = 0;

    public void ApplyColor(Color col)
    {
        for (int i = 0; i < bodyParts.Length; i++)
        {
            GameObject part = bodyParts[i];
            Renderer rend = part.GetComponent<Renderer>();
            Material[] materials = rend.materials;

            if (i < material_name.Length && i < materials.Length)
            {
                mat_idx = SearchMaterial(materials, material_name[i]);
            }
            else
            {
                mat_idx = 0;
            }
            // Debug.Log("mat name: " + materials[mat_idx].name);
            materials[mat_idx].SetColor("_Color", col);
        }
    }

    public int SearchMaterial(Material[] materials, string name)
    {
        if (name == "default")
            return 0;

        // search for the material requested name
        for (int idx = 0; idx < materials.Length; idx++)
        {
            // Debug.Log(materials[idx].name + " " + name);
            if (materials[idx].name == name + " (Instance)")
                return idx;
        }
        return 0;
    }
}
