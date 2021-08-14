using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.IO;

public class DomeController : MonoBehaviour
{
    // The DomeController is a convenience class that allows users of the Dome Projection prefab to tweak
    // the parameters of the dome projection without having to drill down to the DomeProjection script that
    // is attached to the projection camera.

    //----------------------------------------------------------------------------------------------------
    // DEFAULTS AND LIMITS
    //----------------------------------------------------------------------------------------------------

    public const float WorldCameraDefPitch = -80.0f;
    public const float WorldCameraMinPitch = -120.0f;
    public const float WorldCameraMaxPitch = +120.0f;

    public const float WorldCameraDefRoll  =    0.0f;
    public const float WorldCameraMinRoll  = -180.0f;
    public const float WorldCameraMaxRoll  = +180.0f;

    public const int DefFOV = 270;
    public const int MinFOV = 0;
    public const int MaxFOV = 360;

    public const float DefBackFadeIntensity     =  0.1f;
    public const float DefCrescentFadeIntensity =  0.5f;
    public const float DefCrescentFadeRadius    =  0.8f;
    public const float DefCrescentFadeOffset    = -0.2f;

	public const int FOVIncrement = 5;
	public const float RotationIncrement = 5.0f;
	public const float FadeIntensityIncrement = 0.1f;
	public const float FadeRadiusIncrement = 0.1f;
	public const float FadeOffsetIncrement = 0.1f;

	//----------------------------------------------------------------------------------------------------
	// ERROR MESSAGES
	//----------------------------------------------------------------------------------------------------

	private const string ProjectionCameraMissingError = "DomeController: required component \"Projection Camera\" is missing from Dome Projector asset! Reverting the Dome Projector prefab will probably fix this error.";
    private const string ProjectionCameraNotMainCamera = "DomeController: \"Projection Camera\" is not the main camera! Most likely, you still need to delete the \"Main Camera\" that was added when Unity created the scene.";
    private const string WorldCameraMissingError = "DomeController: required component \"World Camera\" is missing from the Dome Projector asset! Reverting the Dome Projector prefab will probably fix this error.";
	private const string FPSTextMissingWarning = "DomeController: \"FPS Text\" not set! Please drag a Text into the \"FPS Text\" slot of the DomeController to enable FPS display!";
	private const string FPSCanvasMissingError = "DomeController: required component \"FPS Canvas\" is missing! Reverting the Dome Projector prefab will probably fix this error.";

    //----------------------------------------------------------------------------------------------------
    // ENUMS
    //----------------------------------------------------------------------------------------------------

    // Anti-aliasing types supported by the dome projection renderer.
    public enum AntiAliasingType
    {
        Off,                    // No anti-aliasing
        SSAA_2X,                // 2X super sampling
        SSAA_4X,                // 4X super sampling
    }

    // Cubemap sizes supported by the dome projection renderer.
    public enum CubeMapType
    {
        Cube512    = 512,       // Cubemap with 512x512 faces
        Cube1024   = 1024,      // Cubemap with 1024x1024 faces
        Cube2048   = 2048,      // etc.
        Cube4096   = 4096,
        Cube8192   = 8192,
    }

    //----------------------------------------------------------------------------------------------------
    // EDITOR PROPERTIES
    //----------------------------------------------------------------------------------------------------

    [Range(WorldCameraMinPitch, WorldCameraMaxPitch)]
    [Tooltip("Controls the pitch (x-axis rotation) of the world camera.")] 
    public float worldCameraPitch = WorldCameraDefPitch;

    [Range(WorldCameraMinRoll, WorldCameraMaxRoll)]
    [Tooltip("Controls the roll (z-axis rotation) of the world camera.")]
    public float worldCameraRoll = WorldCameraDefRoll;

    // Field of view of the fisheye 'lens' used during the dome projection.
    [Range(MinFOV, MaxFOV)]
    [Tooltip("Controls the field of view angle of the fisheye lens.")]
    public int FOV = DefFOV;

    // Size of the cubemaps captured from the scene.
    // Larger cubemaps == higher quality
    // Smaller cubemaps == better performance
    [Tooltip("Sets the size of the cubemap captured from the world camera. Increase the cubemap size to improve the quality of the final image; decrease the cubemap size for more performance.")]
    public CubeMapType cubeMapType = CubeMapType.Cube1024;

    // Anti-aliasing type used when rendering the dome projection
    [Tooltip("Sets the anti-aliasing level used while rendering the fisheye image. Supports 2X and 4X super-sampling.")]
    public AntiAliasingType antiAliasingType = AntiAliasingType.SSAA_2X;

    // Linear front-to-back fade.
    [Range(0.0f, 1.0f)]
    [Tooltip("Controls the intensity of the linear front-to-back fade overlay (where 'back' is the top of the image).")]
    public float backFadeIntensity = DefBackFadeIntensity;

    // Crescent fade.
    [Range(0.0f, 1.0f)]
    [Tooltip("Controls the intensity of the crescent-shaped fade overlay.")]
    public float crescentFadeIntensity = DefCrescentFadeIntensity;

    [Range(0.0f, 1.0f)]
    [Tooltip("Controls the inner radius of the crescent-shaped fade overlay. The crescent fades from the inner radius to the edge of the fisheye lens.")]
    public float crescentFadeRadius = DefCrescentFadeRadius;

    [Range(-1.0f, +1.0f)]
    [Tooltip("Controls the position of the inner circle of the crescent fade overlay. -1 is all the way at the front (bottom) of the fisheye, +1 is all the way at the back (top)")]
    public float crescentFadeOffset = DefCrescentFadeOffset;

	[Tooltip("FPS Display Field")]
	public Text fpsText;

	//----------------------------------------------------------------------------------------------------
	// PUBLIC
	//----------------------------------------------------------------------------------------------------

	public Camera projectionCamera { get { return m_projectionCamera; } }

    public Camera worldCamera { get { return m_worldCamera; } }

    //----------------------------------------------------------------------------------------------------
    // UNITY EVENTS
    //----------------------------------------------------------------------------------------------------
       
	void Awake()
    {
        // One-time initialization of the dome controller.
        //
        // The below code has been designed to be 'automatic'; i.e. the user doesn't have to do anything
        // to correctly setup the dome projection asset. However, for the code to work correctly, it is
        // required that the Dome Projection game object (to which this MonoBehaviour is attached) has
        // two child cameras:
        //
        // - A camera named "Projection Camera", which should have the MainCamera tag
        // - A camera named "World Camera", which should NOT have the MainCamera taf
        //
        // Both cameras are present in the default Dome Projection prefab.

        // Fetch projection camera from child.
        Transform projectionCameraTrans = transform.Find("Projection Camera");
        if (projectionCameraTrans == null)
            Debug.LogError(ProjectionCameraMissingError);
        m_projectionCamera = projectionCameraTrans.GetComponent<Camera>();
        if (m_projectionCamera == null)
            Debug.LogError(ProjectionCameraMissingError);

        // // Check if projection camera is the main camera.
        // if (Camera.main != m_projectionCamera)
        //     Debug.LogError(ProjectionCameraNotMainCamera);

        // Fetch world camera from child.
        Transform worldCameraTrans = transform.Find("World Camera");
        if (worldCameraTrans == null)
            Debug.LogError(WorldCameraMissingError);
        m_worldCamera = worldCameraTrans.GetComponent<Camera>();
        if (m_worldCamera == null)
            Debug.LogError(WorldCameraMissingError);

		if (fpsText != null)
		{
			fpsText.gameObject.SetActive(false);
		}
		else
			Debug.LogError(FPSTextMissingWarning);

		// Save initial pitch and roll.
		m_initialWorldCameraPitch = worldCameraPitch;
		m_initialWorldCameraRoll = worldCameraRoll;

		// Load settings (if available).
		bool haveProductSettings = File.Exists(ProductSettingsFilename);
		bool haveDefaultSettings = File.Exists(DefaultSettingsFilename);
		if (!haveDefaultSettings)
		{
			SaveDefaultSettings();
		}
		if (haveProductSettings)
		{
			LoadProductSettings();
		}
		else
		{
			if (haveDefaultSettings)
				LoadDefaultSettings();
			SaveProductSettings();
		}
	}

	void Update()
	{
		// Keeps the FOV and orientation of the world camera up-to-date.

		ProcessOperatorControls();
		UpdateFPSCounter();

		if (m_worldCamera == null)
            return;

        // Ensure FOV stays within a valid range.
        FOV = Mathf.Clamp(FOV, MinFOV, MaxFOV);

        // Apply world camera pitch and roll.
        worldCameraPitch = Mathf.Clamp(worldCameraPitch, WorldCameraMinPitch, WorldCameraMaxPitch);
        worldCameraRoll  = Mathf.Clamp(worldCameraRoll,  WorldCameraMinRoll,  WorldCameraMaxRoll);
        m_worldCamera.transform.localRotation = Quaternion.Euler(new Vector3(worldCameraPitch, 0, worldCameraRoll));
	}

	//----------------------------------------------------------------------------------------------------
	// PRIVATE
	//----------------------------------------------------------------------------------------------------

	private void ProcessOperatorControls()
	{
		// Operator controls!

		bool shiftDown = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
		if (shiftDown)
		{
			// [F1] and [F2] increase/decrease FOV
			if (Input.GetKeyUp(KeyCode.F1))
				FOV = Mathf.Max(FOV - FOVIncrement, MinFOV);
			else if (Input.GetKeyUp(KeyCode.F2))
				FOV = Mathf.Min(FOV + FOVIncrement, MaxFOV);
			// [F3] and [F4] control world camera pitch.
			else if (Input.GetKeyUp(KeyCode.F3))
				worldCameraPitch += RotationIncrement;
			else if (Input.GetKeyUp(KeyCode.F4))
				worldCameraPitch -= RotationIncrement;
			// [F5] and [F6] control world camera roll.
			else if (Input.GetKeyUp(KeyCode.F5))
				worldCameraRoll += RotationIncrement;
			else if (Input.GetKeyUp(KeyCode.F6))
				worldCameraRoll -= RotationIncrement;
			// [F7] and [F8] control cube map size.
			else if (Input.GetKeyUp(KeyCode.F7))
			{
				if (cubeMapType > CubeMapType.Cube512)
					cubeMapType = (CubeMapType)((int)cubeMapType / 2);
			}
			else if (Input.GetKeyUp(KeyCode.F8))
			{
				if (cubeMapType < CubeMapType.Cube8192)
					cubeMapType = (CubeMapType)((int)cubeMapType * 2);
			}
			// [F9] cycles through AA modes.
			else if (Input.GetKeyUp(KeyCode.F9))
			{
				switch (antiAliasingType)
				{
					case AntiAliasingType.Off:
						antiAliasingType = AntiAliasingType.SSAA_2X;
						break;
					case AntiAliasingType.SSAA_2X:
						antiAliasingType = AntiAliasingType.SSAA_4X;
						break;
					default:
						antiAliasingType = AntiAliasingType.Off;
						break;
				}
			}
			// [F10] toggles V-sync.
			else if (Input.GetKeyUp(KeyCode.F10))
			{
				if (QualitySettings.vSyncCount == 0)
					QualitySettings.vSyncCount = 1;
				else
					QualitySettings.vSyncCount = 0;

			}
			// [F11] save current dome controller parameters as the default settings.
			else if (Input.GetKeyUp(KeyCode.F11))
			{
				SaveDefaultSettings();
			}
			// [F12] resets all dome controller parameters to default settings.
			else if (Input.GetKeyUp(KeyCode.F12))
			{
				LoadDefaultSettings();
			}
		}

		bool ctrlDown = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl) || Input.GetKey(KeyCode.LeftCommand) || Input.GetKey(KeyCode.RightCommand);
		if (ctrlDown)
		{
			// [F1] and [F2] control back fade intensity.
			if (Input.GetKeyUp(KeyCode.F1))
				backFadeIntensity = Mathf.Clamp01(backFadeIntensity - FadeIntensityIncrement);
			else if (Input.GetKeyUp(KeyCode.F2))
				backFadeIntensity = Mathf.Clamp01(backFadeIntensity + FadeIntensityIncrement);
			// [F3] and [F4] control crescent fade intensity.
			else if (Input.GetKeyUp(KeyCode.F3))
				crescentFadeIntensity = Mathf.Clamp01(crescentFadeIntensity - FadeIntensityIncrement);
			else if (Input.GetKeyUp(KeyCode.F4))
				crescentFadeIntensity = Mathf.Clamp01(crescentFadeIntensity + FadeIntensityIncrement);
			// [F5] and [F6] control crescent fade radius.
			else if (Input.GetKeyUp(KeyCode.F5))
				crescentFadeRadius = Mathf.Clamp01(crescentFadeRadius - FadeRadiusIncrement);
			else if (Input.GetKeyUp(KeyCode.F6))
				crescentFadeRadius = Mathf.Clamp01(crescentFadeRadius + FadeRadiusIncrement);
			// [F7] and [F8] control crescent fade offset.
			else if (Input.GetKeyUp(KeyCode.F7))
				crescentFadeOffset = Mathf.Clamp(crescentFadeOffset - FadeOffsetIncrement, -1, +1);
			else if (Input.GetKeyUp(KeyCode.F8))
				crescentFadeOffset = Mathf.Clamp(crescentFadeOffset + FadeOffsetIncrement, -1, +1);
			else if (Input.GetKeyUp(KeyCode.F9))
			{
				if (fpsText != null)
					fpsText.gameObject.SetActive(!fpsText.gameObject.activeSelf);
			}
			else if (Input.GetKeyUp(KeyCode.F11))
			{
				SaveProductSettings();
			}
			else if (Input.GetKeyUp(KeyCode.F12))
			{
				LoadProductSettings();
			}
		}
	}

	private void UpdateFPSCounter()
	{
		// Update FPS counter
		if (fpsText != null && fpsText.gameObject.activeSelf)
		{
			// Maintain a running average of the last N frame deltas, for a more stable frame counter.
			m_frameDeltas[m_curFrameDelta++] = Time.deltaTime;
			if (m_curFrameDelta >= 10)
				m_curFrameDelta = 0;
			float totalFrameDelta = 0.0f;
			for (int i = 0; i < 10; i++)
				totalFrameDelta += m_frameDeltas[i];
			fpsText.text = string.Format("{0}", Mathf.Round(totalFrameDelta != 0.0f ? (float)NumFrameDeltas / totalFrameDelta : 0));
		}
	}

	private void ResetToDefaults()
	{
		worldCameraPitch = m_initialWorldCameraPitch;
		worldCameraRoll = m_initialWorldCameraRoll;
		cubeMapType = CubeMapType.Cube1024;
		antiAliasingType = AntiAliasingType.SSAA_2X;
		QualitySettings.vSyncCount = 1;
		backFadeIntensity = DefBackFadeIntensity;
		crescentFadeIntensity = DefCrescentFadeIntensity;
		crescentFadeRadius = DefCrescentFadeRadius;
		crescentFadeOffset = DefCrescentFadeOffset;
	}

	private void SaveSettingsToFile(string filename)
	{
		using (StreamWriter stream = new StreamWriter(filename))
		{
			stream.WriteLine(string.Format("worldCameraPitch = {0}", worldCameraPitch));
			stream.WriteLine(string.Format("worldCameraRoll = {0}", worldCameraRoll));
			stream.WriteLine(string.Format("FOV = {0}", FOV));
			stream.WriteLine(string.Format("cubeMapType = {0}", (int) cubeMapType));
			stream.WriteLine(string.Format("antiAliasingType = {0}", antiAliasingType.ToString()));
			stream.WriteLine(string.Format("vSync = {0}", QualitySettings.vSyncCount));
			stream.WriteLine(string.Format("backFadeIntensity = {0}", backFadeIntensity));
			stream.WriteLine(string.Format("crescentFadeIntensity = {0}", crescentFadeIntensity));
			stream.WriteLine(string.Format("crescentFadeRadius = {0}", crescentFadeRadius));
			stream.WriteLine(string.Format("crescentFadeOffset = {0}", crescentFadeOffset));
		}
	}

	private void SaveDefaultSettings() { SaveSettingsToFile(DefaultSettingsFilename);  }

	private void SaveProductSettings() { SaveSettingsToFile(ProductSettingsFilename);  }

	private void LoadSettingsFromFile(string filename)
	{
		ResetToDefaults();

		if (File.Exists(filename))
		{
			string[] lines = File.ReadAllLines(filename);
			foreach (string line in lines)
			{
				if (line.IndexOf('=') == -1)
					continue;
				string[] parts = line.Split('=');
				if (parts.Length < 2)
					continue;
				string setting = parts[0].Trim();
				string value = parts[1].Trim();

				if (string.Compare(setting, "worldCameraPitch", true) == 0)
				{
					if (!float.TryParse(value, out worldCameraPitch))
						worldCameraPitch = WorldCameraDefPitch;
					worldCameraPitch = Mathf.Clamp(worldCameraPitch, WorldCameraMinPitch, WorldCameraMaxPitch);
				}
				else if (string.Compare(setting, "worldCameraRoll", true) == 0)
				{
					if (!float.TryParse(setting, out worldCameraRoll))
						worldCameraRoll = WorldCameraDefRoll;
					worldCameraRoll = Mathf.Clamp(worldCameraRoll, WorldCameraMinRoll, WorldCameraMaxRoll);
				}
				else if (string.Compare(setting, "cubeMapType", true) == 0)
				{
					int cubeMapSize;
					if (int.TryParse(value, out cubeMapSize))
					{
						switch (cubeMapSize)
						{
							case 512: cubeMapType = CubeMapType.Cube512; break;
							case 1024: cubeMapType = CubeMapType.Cube1024; break;
							case 2048: cubeMapType = CubeMapType.Cube2048; break;
							case 4096: cubeMapType = CubeMapType.Cube4096; break;
							case 8192: cubeMapType = CubeMapType.Cube8192; break;
							default: goto case 1024;
						}
					}
					else
						cubeMapType = CubeMapType.Cube1024;
				}
				else if (string.Compare(setting, "antiAliasingType", true) == 0)
				{
					if (string.Compare(value, "Off", true) == 0)
						antiAliasingType = AntiAliasingType.Off;
					else if (string.Compare(value, "SSAA_4X", true) == 0)
						antiAliasingType = AntiAliasingType.SSAA_4X;
					else
						antiAliasingType = AntiAliasingType.SSAA_2X;
				}
				else if (string.Compare(setting, "vSync", true) == 0)
				{
					int vSyncCount = 0;
					if (!int.TryParse(value, out vSyncCount))
						vSyncCount = 1;
					if (vSyncCount != 0 && vSyncCount != 1)
						vSyncCount = 1;
					QualitySettings.vSyncCount = vSyncCount;
				}
				else if (string.Compare(setting, "backFadeIntensity", true) == 0)
				{
					if (!float.TryParse(value, out backFadeIntensity))
						backFadeIntensity = DefBackFadeIntensity;
					backFadeIntensity = Mathf.Clamp01(backFadeIntensity);
				}
				else if (string.Compare(setting, "crescentFadeIntensity", true) == 0)
				{
					if (!float.TryParse(value, out crescentFadeIntensity))
						crescentFadeIntensity = DefCrescentFadeIntensity;
					crescentFadeIntensity = Mathf.Clamp01(crescentFadeIntensity);
				}
				else if (string.Compare(setting, "crescentFadeRadius", true) == 0)
				{
					if (!float.TryParse(value, out crescentFadeRadius))
						crescentFadeRadius = DefCrescentFadeRadius;
					crescentFadeRadius = Mathf.Clamp01(crescentFadeRadius);
				}
				else if (string.Compare(setting, "crescentFadeOffset", true) == 0)
				{
					if (!float.TryParse(value, out crescentFadeOffset))
						crescentFadeOffset = DefCrescentFadeOffset;
					crescentFadeOffset = Mathf.Clamp(crescentFadeOffset, -1.0f, +1.0f);
				}
			}
		}
	}

	private void LoadDefaultSettings() { LoadSettingsFromFile(DefaultSettingsFilename);  }

	private void LoadProductSettings() { LoadSettingsFromFile(ProductSettingsFilename);  }

	private string ProductSettingsFilename
	{
		get
		{
			string baseName = Application.productName.Replace(' ', '_').ToLower();
			foreach (char c in System.IO.Path.GetInvalidFileNameChars())
			{
				baseName = baseName.Replace(c, '_');
			}
			return string.Format("{0}\\{1}_settings.txt", Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), baseName);
		}
	}

	private string DefaultSettingsFilename
	{
		get { return string.Format("{0}\\dome_default_settings.txt", Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)); }
	}

	// The projection camera is the camera that is used to render the final fisheye image.
	private Camera m_projectionCamera;

    // The world camera is the camera that is used to capture a cubemap of the scene every frame.
    private Camera m_worldCamera;

	// Camera properties
	private float m_initialWorldCameraPitch = 0.0f;
	private float m_initialWorldCameraRoll = 0.0f;

	// FPS counter data
	private const int NumFrameDeltas = 10;
	private float[] m_frameDeltas = new float[NumFrameDeltas];
	private int m_curFrameDelta;
}
