using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A simple day night cycle. To use it, put the script onto a game object and add light sources as childs, then set the sun and moon as the light sources
/// </summary>
public class DayNightCycleManager : MonoBehaviour
{
    // A wrapper for the light source used for day night cycle
    [System.Serializable]
    public struct Lighting
    {
        [Tooltip("The game object for this light")]
        public GameObject lightObject;
        [Tooltip("Color of the light in gradient form, where 0 is start of day, 1 is end of day")]
        public Gradient lightColor;
        [Tooltip("Intensity of the light in animation curve form, where 0 is start of day, 1 is end of day")]
        public AnimationCurve lightIntensity;
        [Tooltip("The intensity multiplier for the light")]
        public float intensityMultiplier;

        private Light _lightComponent;

        public void Init(bool isMainSource = false)
        {
            _lightComponent = lightObject.GetComponent<Light>();

            // Set the sun source in the environment tab to this light if its set as main source
            if (isMainSource)
                RenderSettings.sun = _lightComponent;
        }

        public void SetLight(float normalizedTime)
        {
            // Get the current color and intensity based on the time
            _lightComponent.color = lightColor.Evaluate(normalizedTime);
            _lightComponent.intensity = lightIntensity.Evaluate(normalizedTime) * intensityMultiplier;
        }
    }

    [Header("Controller Settings")]
    public bool enable = true;
    [Tooltip("Time of day")]
    [Range(0, 24)] public float currentTime = 9.0f;
    [Tooltip("When should the time start")]
    [Range(0, 24)] public float startTime = 0.0f;
    [Tooltip("When should the time end")]
    [Range(0, 24)] public float endTime = 24.0f;
    [Tooltip("Lights' angle")]
    [Range(0, 360)] public float angle = 0;
    [Tooltip("Lights' rotation speed")]
    public float speed = 0.01f;
    [Header("Lights Settings")]
    public Lighting sun;
    public Lighting moon;

    private void Awake()
    {
        sun.Init(true);
        moon.Init();
    }

    private void Update()
    {
        Tick();
        UpdateLight();
    }

    // Update the time
    private void Tick()
    {
        if (currentTime > endTime)
            currentTime = startTime;

        if (enable)
            currentTime += Time.deltaTime * speed;
    }

    private void UpdateLight()
    {
        sun.SetLight(NormalizedTime(currentTime));
        moon.SetLight(NormalizedTime(currentTime));
        SetRotation(currentTime);
    }

    // Caluate the lights' rotation based on time
    private Vector3 newRotation;
    private void SetRotation(float time)
    {
        newRotation.x = ConvertTimeToRotation(time);
        newRotation.y = angle;
        transform.rotation = Quaternion.Euler(newRotation);
    }

    // Convert 0..24 to 0..360
    private float ConvertTimeToRotation(float time)
    {
        return (360 / 24) * time;
    }

    // Convert 0..24 into 0..1
    private float NormalizedTime(float time)
    {
        return time / 24;
    }
}
