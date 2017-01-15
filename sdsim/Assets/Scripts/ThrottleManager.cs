using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ThrottleManager : MonoBehaviour {

	public Car car;

	public bool doControlThrottle = true;

	public Text speedometerUI;
	public Text speedFactorUI;

	public float idealSpeed = 15f;
	public float speedFactor = 1.0f;
	public float brakeThresh = 200.0f;

	public float constThrottleReq = 0.5f;
	public float brakePerc = 0.0001f;

	public float turnSlowFactor = 3.0f;

	void Start()
	{
		
	}
	
	// Update is called once per frame
	void Update () 
	{
		speedFactor = car.Velocity().magnitude * (1.0f + Mathf.Abs(car.requestSteering));
	
		if(speedometerUI != null)
			speedometerUI.text = car.Velocity().magnitude.ToString();

		if(speedFactorUI != null)
			speedFactorUI.text = speedFactor.ToString();

		float idealSpeedAdjusted = idealSpeed - (turnSlowFactor * Mathf.Abs(car.requestSteering));

		if(doControlThrottle)
		{
			if(speedFactor > brakeThresh)
			{
				car.Brake();
			}
			else if(car.Velocity().magnitude < idealSpeedAdjusted)
			{
				car.RequestThrottle(constThrottleReq);
			}
		}
	}
}
