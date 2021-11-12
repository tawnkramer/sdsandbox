using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PID_UI : MonoBehaviour {

	public PIDController pid;
    public Logger logger;

	public Text maxSpeedText;
	public Text P_Term;
	public Text D_Term;
	public Text steerMax;
    public Slider SpeedSlider;
    public Slider PropSlider;
    public Slider DiffSlider;
    public Slider steerMaxSlider;

	void Start()
	{
		
	}

    public void OnEnable()
    {
        steerMaxSlider.interactable = !logger.isActiveAndEnabled;

        if (pid.car != null)
        {
            steerMaxSlider.value = pid.car.GetMaxSteering();
            OnSteerMaxChanged(steerMaxSlider.value);
        }

		SpeedSlider.value = pid.maxSpeed;
    }

	public void OnMaxSpeedChanged(float val)
	{
		maxSpeedText.text = "Max Speed: " + val;
		pid.maxSpeed = val;
	}

	public void OnPTermChanged(float val)
	{
		P_Term.text = "Prop: " + val;
	}

	public void OnDTermChanged(float val)
	{
		D_Term.text = "Diff: " + val;
	}	
	
    public void OnSteerMaxChanged(float val)
	{
        val = steerMaxSlider.value;
		steerMax.text = "Steer Max: " + val;
	}	
}
