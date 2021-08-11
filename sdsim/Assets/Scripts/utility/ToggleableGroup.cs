using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Allow for toggling a group of objects when called ToggleGroup()
/// </summary>
public class ToggleableGroup : MonoBehaviour
{
   public bool toggle = true;
   public List<GameObject> group = new List<GameObject>();

   public void ToggleGroup()
   {
       toggle = !toggle;

       foreach (GameObject item in group)
       {
           item.SetActive(toggle);
       }
   }
}