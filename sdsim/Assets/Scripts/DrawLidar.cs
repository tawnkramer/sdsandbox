using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawLidar : MonoBehaviour
{
    public Lidar lidar;
    public GameObject car;
    static Material lineMaterial;

    void OnPostRender()
    {
        if (GlobalState.drawLidar == true)
        {
            if (car == null) { return; }
            if (lidar == null)
            {
                lidar = car.GetComponentInChildren<Lidar>();
                return;
            }
            else if (lidar.enabled == false) { return; }
            Draw();
        }
    }

    static void CreateLineMaterial()
    {
        if (!lineMaterial)
        {
            // Unity has a built-in shader that is useful for drawing
            // simple colored things.
            Shader shader = Shader.Find("Hidden/Internal-Colored");
            lineMaterial = new Material(shader);
            lineMaterial.hideFlags = HideFlags.HideAndDontSave;
            // Turn on alpha blending
            lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            // Turn backface culling off
            lineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            // Turn off depth writes
            lineMaterial.SetInt("_ZWrite", 0);
        }
    }

    public void Draw()
    {
        CreateLineMaterial();
        lineMaterial.SetPass(0); // Apply the line material

        GL.Begin(GL.LINES); // Draw lines

        if (lidar.pointArr == null)
            return;

        Vector3 lidarPos = lidar.transform.position;
        foreach (LidarPoint p in lidar.pointArr.points)
        {
            if (p == null)
                continue;

            GL.Color(new Color(1, 0, 0, 0.8F));

            Vector3 ppoint = new Vector3(p.x, p.y, p.z) + lidarPos;

            GL.Vertex3(lidarPos.x, lidarPos.y, lidarPos.z);
            GL.Vertex3(ppoint.x, ppoint.y, ppoint.z);
        }

        GL.End();

    }
}
