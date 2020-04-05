using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class CameraSensor : MonoBehaviour {

	public Camera sensorCam;
	public int width = 256;
	public int height = 256;
	public int depth = 3;
	public string img_enc = "JPG"; //accepts JPG, PNG, TGA
	Texture2D tex;
	RenderTexture ren;

	public void SetConfig(float fov, float offset_x, float offset_y, float offset_z, float rot_x, int img_w, int img_h, int img_d, string _img_enc)
	{
		if (img_d != 0)
		{
			depth = img_d;
		}

		if (img_w != 0 && img_h != 0)
		{
			width = img_w;
			height = img_h;
			
			Awake();
		}

		if(_img_enc.Length == 3)
			img_enc = _img_enc;

		if(offset_x != 0.0f || offset_y != 0.0f || offset_z != 0.0f)
			transform.localPosition = new Vector3(offset_x, offset_y, offset_z);
	
		if(rot_x != 0.0f)
			transform.eulerAngles = new Vector3(rot_x, 0.0f, 0.0f);

		if(fov != 0.0f)
			sensorCam.fieldOfView = fov;
	}

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

		if(depth == 1)
		{
			//assumes TextureFormat.RGB24. Convert to grey scale image
			NativeArray<byte> bytes = tex.GetRawTextureData<byte>();
			for (int i=0; i<bytes.Length; i+=3)
			{
				byte gray = (byte)(0.2126f * bytes[i+0] + 0.7152f * bytes[i+1] + 0.0722f * bytes[i+2]);
				bytes[i+2] = bytes[i+1] = bytes[i+0] = gray;
			}
			tex.Apply();
		}

		return tex;
	}

	public Texture2D GetImage()
	{
		return RTImage(sensorCam);
	}

	public byte[] GetImageBytes()
	{
		if(img_enc == "PNG")
			return GetImage().EncodeToPNG();

		if(img_enc == "TGA")
			return GetImage().EncodeToTGA();
			
		return GetImage().EncodeToJPG();
	}
}
