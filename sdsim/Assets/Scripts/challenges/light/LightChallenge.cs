using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightChallenge : MonoBehaviour, IChallenge
{
    public GameObject lightSource;
    public Color minLightColorRange;
    public Color maxLightColorRange;

    public void InitChallenge()
    {
        Randomize();
    }

    public void ResetChallenge()
    {
        InitChallenge();
    }

    public void Randomize()
    {
        if (lightSource != null)
        {
            Light lightComp = lightSource.GetComponent<Light>();
            if (lightComp != null)
            {    
                // calculate the interpolation between the two colors for a random T
                lightComp.color = Color.Lerp(minLightColorRange, maxLightColorRange, Random.Range(0.0f, 1.0f));
            }

            lightSource.transform.localRotation = Quaternion.Euler(90 + Random.Range(-45, 45), Random.Range(0, 180), 0);
        }
    }
}
