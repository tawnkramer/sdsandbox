using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PID_UI : MonoBehaviour {

	public PIDController pid;

	public Text maxSpeedText;
	public Text P_Term;
	public Text D_Term;

	void Start()
	{
		OnMaxSpeedChanged(pid.maxSpeed);
		OnPTermChanged(pid.Kp);
		OnDTermChanged(pid.Kd);
	}

	public void OnMaxSpeedChanged(float val)
	{
		maxSpeedText.text = "Max Speed: " + val;
		pid.maxSpeed = val;
	}

	public void OnPTermChanged(float val)
	{
		P_Term.text = "Prop: " + val;
		pid.Kp = val;
	}

	public void OnDTermChanged(float val)
	{
		D_Term.text = "Diff: " + val;
		pid.Kd = val;
	}	
	
}
