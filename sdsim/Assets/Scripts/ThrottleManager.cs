using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ThrottleManager : MonoBehaviour {

	public GameObject carObj;
	public ICar car;

	public bool doControlThrottle = true;

	public Text speedometerUI;
	public Text speedFactorUI;

	public float idealSpeed = 15f;
	public float speedFactor = 1.0f;
	public float brakeThresh = 200.0f;

	public float constThrottleReq = 0.5f;
	public float brakePerc = 0.0001f;

	public float turnSlowFactor = 3.0f;

	//give the network time to connect before turning on the throttle.
	public float delayBeforeStart = 2.0f;

	void Awake()
	{
		car = carObj.GetComponent<ICar>();
	}
	
	// Update is called once per frame
	void Update () 
	{
		if(delayBeforeStart > 0.0f)
		{
			delayBeforeStart -= Time.deltaTime;
			return;
		}
		
		speedFactor = car.GetVelocity().magnitude * (1.0f + Mathf.Abs(car.GetSteering()));
	
		if(speedometerUI != null)
			speedometerUI.text = car.GetVelocity().magnitude.ToString();

		if(speedFactorUI != null)
			speedFactorUI.text = speedFactor.ToString();

		float idealSpeedAdjusted = idealSpeed - (turnSlowFactor * Mathf.Abs(car.GetSteering()));

		if(doControlThrottle)
		{
			if(speedFactor > brakeThresh)
			{
				car.RequestThrottle(0.0f);
				car.RequestFootBrake(1.0f);
			}
			else if(car.GetVelocity().magnitude < idealSpeedAdjusted)
			{
				car.RequestFootBrake(0.0f);
				car.RequestThrottle(constThrottleReq);
			}
			else
			{
				car.RequestThrottle(0.0f);
				car.RequestFootBrake(0.0f);
			}
		}
	}
}
