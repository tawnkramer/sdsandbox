using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Set a random HDRI background if enabled
/// </summary>
public class RandomBackground : MonoBehaviour
{
    [Tooltip("Enable or disable the random background")]
    public bool enable = true;
    [Tooltip("Refrash rate for the random background")]
    public float speed = 0.1f;
    [Tooltip("Background material if not enabled")]
    public Material defaultSky;
    [Tooltip("Background material if enabled")]
    public Material HDRISky;
    [Tooltip("HDRI skys to be used for the random background")]
    public List<Texture> Skys = new List<Texture>();

    private void Awake()
    {
        StartCoroutine(SetRandomBackground());
    }

    IEnumerator SetRandomBackground()
    {
        while (true)
        {
            if (enable)
            {
                if (RenderSettings.skybox != HDRISky)
                    RenderSettings.skybox = HDRISky;

                // Set random HDRI and a random rotation
                RenderSettings.skybox.mainTexture = Skys[Random.Range(0, Skys.Count)];
                RenderSettings.skybox.SetFloat("_Rotation", Random.Range(0, 360));
                
                yield return new WaitForSeconds(speed);
            }
            else
            {
                if (RenderSettings.skybox != defaultSky)
                    RenderSettings.skybox = defaultSky;

                yield return null;
            }
        }
    }
}
