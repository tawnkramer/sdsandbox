using UnityEngine;
using System.Collections;

public class Rotator : MonoBehaviour 
{
    public float Speed = 5.0f;
	
	void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
            m_paused = !m_paused;

        if (!m_paused)
        {
            m_angle += Time.deltaTime * Speed;
            if (m_angle < 0.0f)
                m_angle += 360.0f;
            else if (m_angle > 360.0f)
                m_angle -= 360.0f;

            transform.localRotation = Quaternion.AngleAxis(m_angle, Vector3.up);
        }
	}

    private float m_angle = 0.0f;
    private bool m_paused = false;
}
