using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class CameraTrigger : MonoBehaviour
{
    RaceCamera raceCamera;
    BoxCollider boxCollider;
    string target = "body";

    void Awake()
    {
        boxCollider = transform.GetComponent<BoxCollider>();
        boxCollider.isTrigger = true;
    }

    public void setBoxCollider(Vector3 position, Quaternion rotation, Vector3 scale)
    {
        boxCollider.transform.position = position;
        boxCollider.transform.localScale = scale;
        boxCollider.transform.rotation = rotation;
    }

    void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.name != target) { return; }
        if (raceCamera == null) { raceCamera = transform.GetComponentInParent<RaceCamera>(); }
        raceCamera.CameraTriggered(col);
    }
}