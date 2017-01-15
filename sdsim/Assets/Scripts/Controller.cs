using UnityEngine;
using System.Collections;

public class Controller : MonoBehaviour {

	public Car car;
	public DistanceSensor[] distSensors;
	public CollisionSensor colSensor;
	public float steeringIncrement = 0.1f;
	public float straightThrottleMag = 0.3f;
	public float turnThrottleMag = 0.1f;
	public float reverseThrottleMag = 0.1f;
	public float reverseSteerTurn = 0.5f;
	public float stalledTime = 0.0f;

	public enum State
	{
		Stopped,
		MovingStraight,
		TurningRight,
		TurningLeft,
		ReverseTurn,
	}

	public State state;

	// Use this for initialization
	void Start () 
	{
		state = State.Stopped;

		if(colSensor)
			colSensor.collideCB += this.OnCollide;
	}

	void OnCollide(string objType)
	{
		if(objType == "goal")
		{
			//yay! we made it.

		}

		ChangeState(State.Stopped);
	}
	
	void Update () 
	{

		DistanceSensor frontSensor = distSensors[0];
		DistanceSensor frontLSensor = distSensors[1];
		DistanceSensor frontRSensor = distSensors[2];

		if(car.Velocity().magnitude < 0.1f)
		{
			stalledTime += Time.deltaTime;
		}
		else
		{
			stalledTime = 0.0f;
		}

		if(state == State.Stopped)
		{
			if(car.Velocity().magnitude < 0.1f)
			{
				if(frontSensor.lastObjectSensed == "none" || frontSensor.lastObjectSensed == "goal")
				{
					ChangeState(State.MovingStraight);
				}
				else if(frontSensor.lastDistSensed < 1.0f)
				{
					ChangeState(State.ReverseTurn);
				}
				else if(frontLSensor.lastObjectSensed == "none")
				{
					ChangeState(State.TurningLeft);
				}
				else if(frontRSensor.lastObjectSensed == "none")
				{
					ChangeState(State.TurningRight);
				}
				else
				{
					ChangeState(State.ReverseTurn);
				}
			}
		}
		else if(state == State.MovingStraight)
		{
			if(stalledTime > 3.0f)
			{
				stalledTime = 0.0f;
				ChangeState(State.ReverseTurn);
			}
			else 
				if(frontSensor.lastObjectSensed == "none" || frontSensor.lastObjectSensed == "goal")
			{
				//Awesome, keep going straight.

				if(frontLSensor.lastDistSensed < 5.0f)
				{
					ChangeState(State.TurningRight);
				}
				else if(frontRSensor.lastDistSensed < 5.0f)
				{
					ChangeState(State.TurningLeft);
				}
			}
			else if(frontSensor.lastDistSensed < 10.0f)
			{
				//turn in which ever direction has no object.
				if(frontLSensor.lastObjectSensed == "none")
				{
					ChangeState(State.TurningLeft);
				}
				else
					if(frontRSensor.lastObjectSensed == "none")
				{
					ChangeState(State.TurningRight);
				}
				else 
					if(frontLSensor.lastDistSensed < frontRSensor.lastDistSensed)
				{
					ChangeState(State.TurningRight);
				}
				else 
					if(frontRSensor.lastDistSensed < frontLSensor.lastDistSensed)
				{
					ChangeState(State.TurningLeft);
				}
			}

		}
		else if(state == State.TurningLeft)
		{
			if(frontLSensor.lastDistSensed < 0.5 || frontRSensor.lastDistSensed < 0.5)
			{
				ChangeState(State.ReverseTurn);
			}
			else if(frontRSensor.lastDistSensed < 5f)
			{
				car.RequestSteering(steeringIncrement * -3f);
			}else if(frontLSensor.lastObjectSensed == "none" || frontSensor.lastObjectSensed == "none")
			{
				ChangeState(State.MovingStraight);
			}
		}
		else if(state == State.TurningRight)
		{
			if(frontLSensor.lastDistSensed < 0.5 || frontRSensor.lastDistSensed < 0.5)
			{
				ChangeState(State.ReverseTurn);
			}
			else if(frontLSensor.lastDistSensed < 5f)
			{
				car.RequestSteering(steeringIncrement * 3);
			}
			else if(frontRSensor.lastObjectSensed == "none" || frontSensor.lastObjectSensed == "none")
			{
				ChangeState(State.MovingStraight);
			}
		}
		else if(state == State.ReverseTurn)
		{
			if(frontSensor.lastDistSensed > 5f && frontLSensor.lastDistSensed > 3f &&
			   frontRSensor.lastDistSensed > 3f)
			{
				ChangeState(State.Stopped);
			}
		}
	}

	void ChangeState(State s)
	{
		if(s == State.ReverseTurn)
		{
			car.RequestSteering(reverseSteerTurn);
			car.RequestThrottle(-reverseThrottleMag);
		}
		else if(s == State.MovingStraight)
		{
			car.RequestSteering(0.0f);
			car.RequestThrottle(straightThrottleMag);
		}
		else if(s == State.TurningLeft)
		{
			car.RequestSteering(-steeringIncrement);
			car.RequestThrottle(turnThrottleMag);
		}
		else if(s == State.TurningRight)
		{
			car.RequestSteering(steeringIncrement);
			car.RequestThrottle(turnThrottleMag);
		}
		else if(s == State.Stopped)
		{
			car.Brake();
		}

		state = s;
	}
}
