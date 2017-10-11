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
    public Slider steerMaxSlider;

	void Start()
	{
		
	}

    public void OnEnable()
    {
        steerMaxSlider.interactable = !logger.isActiveAndEnabled;

        OnMaxSpeedChanged(pid.maxSpeed);
		OnPTermChanged(pid.Kp);
		OnDTermChanged(pid.Kd);
        steerMaxSlider.value = pid.car.GetMaxSteering();
        OnSteerMaxChanged(steerMaxSlider.value);
    }

	public void OnMaxSpeedChanged(float val)
	{
		maxSpeedText.text = "Max Speed: " + val;
		pid.maxSpeed = val;
        pid.SavePrefs();
	}

	public void OnPTermChanged(float val)
	{
		P_Term.text = "Prop: " + val;
		pid.Kp = val;
        pid.SavePrefs();
	}

	public void OnDTermChanged(float val)
	{
		D_Term.text = "Diff: " + val;
		pid.Kd = val;
        pid.SavePrefs();
	}	
	
    public void OnSteerMaxChanged(float val)
	{
        val = steerMaxSlider.value;
		steerMax.text = "Steer Max: " + val;
		pid.car.SetMaxSteering(val);
	}	
}
