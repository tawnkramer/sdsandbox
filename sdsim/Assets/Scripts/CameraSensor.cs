using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSensor : MonoBehaviour {

	public Camera sensorCam;
	public int width = 256;
	public int height = 256;

	Texture2D tex;
	RenderTexture ren;

	void Awake()
	{
		tex = new Texture2D(width, height, TextureFormat.RGB24, false);
		ren = new RenderTexture(width, height, 16, RenderTextureFormat.ARGB32);
		sensorCam.targetTexture = ren;
	}

	Texture2D RTImage(Camera cam) 
	{
		RenderTexture currentRT = RenderTexture.active;
		RenderTexture.active = cam.targetTexture;
		cam.Render();
		tex.ReadPixels(new Rect(0, 0, cam.targetTexture.width, cam.targetTexture.height), 0, 0);
		tex.Apply();
		RenderTexture.active = currentRT;
		return tex;
	}

	public Texture2D GetImage()
	{
		return RTImage(sensorCam);
	}
}
