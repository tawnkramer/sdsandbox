using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Set a random color for the material based on HSV if enabled
/// </summary>
public class RandomMatColor : MonoBehaviour
{
    [Tooltip("Enable or disable the random material color")]
    public bool enable = true;
    [Tooltip("Refrash rate for the random material color")]
    public float speed = 0.01f;

    [Header("Random HSV Settings")]
    [Tooltip("Randomize the hue of HSV")]
    public bool enableHue = true;
    [Range(0, 1)] public float hueMin = 0f;
    [Range(0, 1)] public float hueMax = 1f;
    [Tooltip("Randomize the saturation of HSV")]
    public bool enableSaturation = true;
    [Range(0, 1)]public float saturationMin = 0f;
    [Range(0, 1)]public float saturationMax = 1f;
    [Tooltip("Randomize the value of HSV")]
    public bool enableValue = true;
    [Range(0, 1)]public float valueMin = 0f;
    [Range(0, 1)]public float valueMax = 1f;
    [Tooltip("Randomize the alpha, the material should have transparency enabled")]
    public bool enableAlpha = true;
    [Range(0, 1)]public float alphaMin = 0f;
    [Range(0, 1)]public float alphaMax = 1f;

    private Material _mat;
    private float _startH, _startS, _startV;

    private void Awake()
    {
        // Get the default color
        _mat = GetComponent<Renderer>().material;
        Color.RGBToHSV(_mat.color, out _startH, out _startS, out _startV);

        StartCoroutine(SetRandomMatColor());
    }

    IEnumerator SetRandomMatColor()
    {
        while (true)
        {
            if (enable)
            {
                Color newColor;
                float H = _startH;
                float S = _startS;
                float V = _startV;
                float A = 1f;

                if (enableHue)
                    H = Random.Range(hueMin, hueMax);

                if (enableSaturation)
                    S = Random.Range(saturationMin, saturationMax);

                if (enableValue)
                    V = Random.Range(valueMin, valueMax);

                if (enableAlpha)
                    A = Random.Range(alphaMin, alphaMax);

                newColor = Color.HSVToRGB(H, S, V);
                newColor.a = A;

                _mat.color = newColor;

                yield return new WaitForSeconds(speed);
            }
            else
            {
                yield return null;
            }
        }
    }
}
