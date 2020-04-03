using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceTarget : MonoBehaviour
{
    public Transform target;
    public float offset = 90;

    void Update()
    {
        if(target)
        {

            // Get Angle in Radians
             float AngleRad = Mathf.Atan2(target.transform.position.z - transform.position.z, target.transform.position.x - transform.position.x);
             // Get Angle in Degrees
             float AngleDeg = offset + (180 / Mathf.PI) * AngleRad;
             // Rotate Object
             this.transform.localRotation = Quaternion.Euler(0, AngleDeg, 0 );

            //transform.LookAt(target, Vector3.left);
        }
    }
}
