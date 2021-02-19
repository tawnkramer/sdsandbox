using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightChallenge : MonoBehaviour, IWaitCarPath
{
    public GameObject lightSource;
    public Color minLightColorRange;
    public Color maxLightColorRange;
    public float max_angle = 45;

    public void Init()
    {
        if (!GlobalState.randomLight) { return; }
        Randomize();
    }

    public void ResetChallenge()
    {
        Init();
    }

    public void Randomize()
    {
        if (GlobalState.useSeed) { Random.InitState(GlobalState.seed); };
        if (lightSource != null)
        {
            Light lightComp = lightSource.GetComponent<Light>();
            if (lightComp != null)
            {
                // calculate the interpolation between the two colors for a random T
                lightComp.color = Color.Lerp(minLightColorRange, maxLightColorRange, Random.Range(0.0f, 1.0f));
            }

            lightSource.transform.localRotation = Quaternion.Euler(90 + Random.Range(-max_angle, max_angle), Random.Range(0, 180), 0);
        }
    }
}
