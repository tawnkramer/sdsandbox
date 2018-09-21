using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenSaver : MonoBehaviour 
{
	public bool bRecordOnAwake = true;
	public bool bRecording = false;
	public static int frameCounter = 0;
	public string filenameRoot = "capture";

	void Awake()
	{
		if(bRecordOnAwake)
			bRecording = true;
	}

	// Update is called once per frame
	void Update () {

		if(bRecording)
		{
			string filename = string.Format("{0}{1,8:D8}.png", filenameRoot, frameCounter);
			StartCoroutine(TakeShot(filename));
			frameCounter += 1;
		}
	}

	IEnumerator TakeShot(string filename)
	{
		yield return new WaitForEndOfFrame();

		ScreenCapture.CaptureScreenshot(filename);
	}
}
