using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//Create an interface class that the sdsim will expect. We can use this to wrap other car implementations
//like the Unity standard asset car.
public interface ICar
{
	//all inputs require 0-1 input except steering which is in degrees, where 0 is center.
	void RequestThrottle(float val);

	void RequestSteering(float val);

	void RequestFootBrake(float val);

	void RequestHandBrake(float val);


	//query last input given.
	float GetSteering();

	float GetThrottle();

	float GetFootBrake();

	float GetHandBrake();


	//query state.
	Transform GetTransform();

	Vector3 GetVelocity();

	Vector3 GetAccel();


	//mark the current activity for partial selections when creating training sets later.
	string GetActivity();

	void SetActivity(string act);


	//Save and restore State
	void SavePosRot();

	void RestorePosRot();
}
