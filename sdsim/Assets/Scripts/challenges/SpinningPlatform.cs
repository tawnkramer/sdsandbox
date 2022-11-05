using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SpinningPlatform : MonoBehaviour
{
    public float speed = 1;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }
    
    public void Update()
    {
        rb.angularVelocity = Vector3.up * speed;
    }

}
