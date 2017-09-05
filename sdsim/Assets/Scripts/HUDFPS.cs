using UnityEngine;
using UnityEngine.UI;

//Credit Poless on http://wiki.unity3d.com/index.php?title=FramesPerSecond

public class HUDFPS : MonoBehaviour 
{
	
	// Attach this to a GUIText to make a frames/second indicator.
	//
	// It calculates frames/second over each updateInterval,
	// so the display does not keep changing wildly.
	//
	// It is also fairly accurate at very low FPS counts (<10).
	// We do this not by simply counting frames per interval, but
	// by accumulating FPS for each frame. This way we end up with
	// correct overall FPS even if the interval renders something like
	// 5.5 frames.
	
	public float updateInterval = 3.0F;
	
	private float accum   = 0; // time accumulated over the interval
	private int   frames  = 0; // frames drawn over the interval

	public Text status;
	
	void Start()
	{
		if( !status)
		{
			Debug.Log("HUDFPS needs a Text component!");
			enabled = false;
			return;
		}
	}
	
	void Update()
	{
		accum += Time.deltaTime;
		++frames;
		
		// Interval ended - update GUI text and start new interval
		if( accum > updateInterval  && status != null)
		{
			// display two fractional digits (f2 format)
			float fps = frames / accum;
			string format = System.String.Format("FPS: {0:F1}",fps);

            status.text = format;
			
			accum = 0.0F;
			frames = 0;			
		}
	}
}