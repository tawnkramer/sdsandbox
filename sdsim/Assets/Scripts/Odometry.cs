using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This just doesn't work. The tires spin too fast for this reading to mean anything.

public class Odometry : MonoBehaviour
{
    public string Label = "tire";
    private Quaternion lastRotation;
    private double numRotations = 0.0f;

    private void Start()
    {
        lastRotation = GetTireRotation();
    }

    Quaternion GetTireRotation()
    {
        return transform.localRotation;
    }

    public double GetNumberRotations()
    {
        return numRotations;
    }


    public void FixedUpdate()
    {
        Quaternion currentRotation = GetTireRotation();
        Quaternion deltaRotation = Quaternion.Inverse(lastRotation) * currentRotation;
        float deltaAngle = 2.0f * Mathf.Rad2Deg * Mathf.Atan2(deltaRotation.x, deltaRotation.w);
        numRotations += Mathf.Abs(deltaAngle / 360.0f);
        lastRotation = currentRotation;
    }
}
