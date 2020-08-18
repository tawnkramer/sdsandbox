using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This just doesn't work. The tires spin too fast for this reading to mean anything.

public class Odometry : MonoBehaviour
{
    public GameObject tire;
    Quaternion lastRotation;
    float angle_since_last_reading = 0;
    public string label = "tire";
    float time_last_read = 0.0f;
    float deg_per_second = 0.0f;

    private void Start()
    {
        time_last_read = Time.realtimeSinceStartup;
        lastRotation = GetTireRotation();
    }

    Quaternion GetTireRotation()
    {
        return tire.transform.localRotation;
    }

    public JSONObject GetOutputAsJson()
    {
        float deg_change = angle_since_last_reading;
        angle_since_last_reading = 0.0f;

        float now = Time.realtimeSinceStartup;
        float delta_t = now - time_last_read;
        time_last_read = now;

        deg_per_second = deg_change / delta_t;

        JSONObject json = JSONObject.Create();
        json.AddField("label", label);
        json.AddField("deg_change", deg_change);
        json.AddField("deg_p_sec", deg_per_second);
        return json;
    }

    public float Read()
    {
        float delta_change = 0.0f;

        Quaternion new_rot = GetTireRotation();
        delta_change = Quaternion.Angle(new_rot, lastRotation);
        lastRotation = new_rot;
        return delta_change;
    }

    public void FixedUpdate()
    {
        //read as fast as we can
        angle_since_last_reading += Read();
    }
}
