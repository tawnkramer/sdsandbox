using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UIHelper;

public class RandomBackgroundUI : MonoBehaviour
{
    [Header("UI Settings")]
    public Toggle enableRandomBGToggle;
    public SliderWithText speed;

    private RandomBackground _manager;

    private void OnEnable()
    {
        _manager = FindObjectsOfType<RandomBackground>()[0];
        if (_manager != null)
        {
            enableRandomBGToggle.onValueChanged.AddListener(delegate { UIHandler(0); });
            speed.slider.onValueChanged.AddListener(delegate { UIHandler(1); });
            
            UpdateUI();
        }
        else
        {
            Debug.LogWarning("No random background manager in scene!");
        }
    }

    private void UIHandler(int idx_)
    {
        switch (idx_)
        {
            case 0:
                _manager.enable = enableRandomBGToggle.isOn;
                break;
            case 1:
                _manager.speed = speed.Value;
                UpdateUI();
                break;
        }
    }

    private void UpdateUI()
    {
        enableRandomBGToggle.isOn = _manager.enable;
        speed.Text = $"Current Speed: {_manager.speed.ToString("0.00")}";
    }
}
