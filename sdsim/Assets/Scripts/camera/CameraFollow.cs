using UnityEngine;
using System.Collections;

public class CameraFollow : MonoBehaviour
{

    public Transform target;

    public float approachPosRate = 0.1f;
    public float approachRotRate = 0.05f;

    void FixedUpdate()
    {
        if (target != null)
        {
            float fixedDeltaTimeRate = (Time.fixedDeltaTime / 0.02f);
            transform.position = Vector3.Lerp(transform.position, target.position, approachPosRate * fixedDeltaTimeRate);
            transform.rotation = Quaternion.Lerp(transform.rotation, target.rotation, approachRotRate * fixedDeltaTimeRate);
        }
    }
}
