using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UIHelper;


public class DayNightCycleManagerUI : MonoBehaviour
{
    public DayNightCycleManager manager;

    [Header("UI Settings")]
    public Toggle enableCycleToggle;
    public SliderWithText currentTime;
    public SliderWithText startTime;
    public SliderWithText endTime;
    public SliderWithText speed;
    public SliderWithText angle;
    public SliderWithText sunStrengthMultiplier;
    public SliderWithText moonStrengthMultiplier;

    private void OnEnable()
    {
        if (manager != null)
        {
            // Add listeners for each UI items
            enableCycleToggle.onValueChanged.AddListener(delegate { UIHandler(0); });
            currentTime.slider.onValueChanged.AddListener(delegate { UIHandler(1); });
            startTime.slider.onValueChanged.AddListener(delegate { UIHandler(2); });
            endTime.slider.onValueChanged.AddListener(delegate { UIHandler(3); });
            speed.slider.onValueChanged.AddListener(delegate { UIHandler(4); });
            angle.slider.onValueChanged.AddListener(delegate { UIHandler(5); });
            sunStrengthMultiplier.slider.onValueChanged.AddListener(delegate { UIHandler(6); });
            moonStrengthMultiplier.slider.onValueChanged.AddListener(delegate { UIHandler(7); });
        }
        else
        {
            Debug.LogWarning("No day night cycle manager in scene!");
        }
    }

    private void UIHandler(int idx_)
    {
        switch (idx_)
        {
            case 0:
                manager.enable = enableCycleToggle.isOn;
                break;
            case 1:
                manager.currentTime = currentTime.Value;
                break;
            case 2:
                manager.startTime = startTime.Value;
                break;
            case 3:
                manager.endTime = endTime.Value;
                break;
            case 4:
                manager.speed = speed.Value;
                break;
            case 5:
                manager.angle = angle.Value;
                break;
            case 6:
                manager.sun.intensityMultiplier = sunStrengthMultiplier.Value;
                break;
            case 7:
                manager.moon.intensityMultiplier = moonStrengthMultiplier.Value;
                break;
        }
    }

    private void Update()
    {
        UpdateUI();
    }

    public void UpdateUI()
    {
        // Update current time
        currentTime.Value = manager.currentTime;

        // Update text
        currentTime.Text = "Current Time: " + manager.currentTime.ToString("0.00");
        startTime.Text = "Start Time: " + manager.startTime.ToString("0.00");
        endTime.Text = "End Time: " + manager.endTime.ToString("0.00");
        speed.Text = "Speed: " + manager.speed.ToString("0.00");
        angle.Text = "Angle: " + manager.angle.ToString("0.00");
        sunStrengthMultiplier.Text = "Sun Strength: " + manager.sun.intensityMultiplier.ToString("0.00");
        moonStrengthMultiplier.Text = "Moon Strength: " + manager.moon.intensityMultiplier.ToString("0.00");
    }
}
