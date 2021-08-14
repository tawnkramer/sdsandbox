using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Camera))]
public class DomeProjection : MonoBehaviour
{
    // The DomeProjection is an image effect script that takes care of rendering the fisheye image from a
    // cubemap capture of the scene.
    //
    // Note that the DomeProjection image effect will overwrite everything that is rendered *before* the 
    // DomeProjection image effect itself. It is possible, however, to render additional image effects
    // and/or a UI *after* the DomeProjection image effect itself.
    //
    // If you wish to apply image effects to the scene, you should *not* attach those image effects to
    // the projection camera (to which this DomeProjection script is attached), but rather to the "World Camera"
    // that is used to capture a cubemap of the scene every frame. Beware of expensive image effects to,
    // as every image effect attached to the "World Camera" will run 6 times *per frame* at the resolution
    // of the cubemap!

    //----------------------------------------------------------------------------------------------------
    // UNITY EVENTS
    //----------------------------------------------------------------------------------------------------

    void Awake()
    {
        // One-time initialization of the dome projection component during startup.

        m_controller = GetComponentInParent<DomeController>();
        if (m_controller == null)
            Debug.LogError("DomeProjection: Cannot find controller! Reverting the Dome Projector prefab will probably fix this error."); 

        // Configure camera for optimal performance:
        // - Don't clear (dome projection will render all pixels)
        // - Cull everything (dome projection will cover entire view)
        Camera camera = GetComponent<Camera>();
        camera.clearFlags = CameraClearFlags.Nothing;
        camera.cullingMask = 0;

		// if (Camera.main != camera)
		// {
		// 	Debug.LogError("DomeProjection: Dome Projection Camera is not the main camera! Remove all other cameras tagged with 'MainCamera' to resolve this problem.");
		// }

        // Load DomeProjection shader and create dome projection material.
        Shader shader = Shader.Find("Hidden/ZubrVR/DomeProjection");
        if (shader != null)
        {
            m_material = new Material(shader);
        }
        else
        {
            Debug.LogError("DomeProjection: Cannot find DomeProjection.shader! Please make sure that DomeProjection.shader is included in your project and located in a Resources directory.");
        }
    }

    void LateUpdate()
    {
        // Triggers rendering of the cubemap from the World Camera.
        // This is done in the LateUpdate() to allow (most) scripts to run *before* the cubemap capture.

        if (m_controller.worldCamera == null || m_material == null)
            return;

        // Ensure that the cubemap render target is properly initialized and has the correct size.
        int cubeMapSize = (int) m_controller.cubeMapType;
        if (m_cubeRT == null || m_cubeRT.width != cubeMapSize)
        {
            if (m_cubeRT != null)
                Destroy(m_cubeRT);

            m_cubeRT = new RenderTexture(cubeMapSize, cubeMapSize, 24, RenderTextureFormat.ARGB32);
            m_cubeRT.isCubemap = true;
            m_cubeRT.Create();            
        }  
            
        // Every frame, render a new cubemap from the cubeMapCamera.
        m_controller.worldCamera.RenderToCubemap(m_cubeRT);
    }

    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        // Renders the fisheye image from the cubemap.
        // NOTE: src is ignore (i.e. anything that was rendered before this image effect is effectively discarded).

        if (m_controller.worldCamera == null || m_material == null)
            return;

        // Since Unity will always render axis-aligned cubemaps, we have to mimic the orientation of the capture camera
        // in the shader in order to generate the correct view within the cubemap.
        Quaternion rot = Quaternion.Inverse(m_controller.worldCamera.transform.rotation);
        m_material.SetVector("_Rotation", new Vector4(rot.x, rot.y, rot.z, rot.w));

        // Set fisheye FOV.
        m_material.SetFloat("_HalfFOV", ((float) m_controller.FOV) * 0.5f * Mathf.Deg2Rad);

        // Fade parameters
        m_material.SetVector("_FadeParams", new Vector4(m_controller.backFadeIntensity, m_controller.crescentFadeIntensity, m_controller.crescentFadeRadius, m_controller.crescentFadeOffset));

        // Render the fisheye image with the appropriate amount of anti-aliasing.
        switch (m_controller.antiAliasingType)
        {
        case DomeController.AntiAliasingType.SSAA_2X:
            DoSSAA(dest, 1.414f);
            break;
        case DomeController.AntiAliasingType.SSAA_4X:
            DoSSAA(dest, 2.0f);
            break;
        default:
            Graphics.Blit(m_cubeRT, dest, m_material);
            break;
        }
    }

    //----------------------------------------------------------------------------------------------------
    // ANTI-ALIASING
    //----------------------------------------------------------------------------------------------------

    private void DoSSAA(RenderTexture dest, float factor)
    {
        int w = Screen.width;
        int h = Screen.height;
        int d = 24;
        RenderTextureFormat f = RenderTextureFormat.ARGB32;

        if (dest != null)
        {
            w = dest.width;
            h = dest.height;
            d = dest.depth;
            f = dest.format;
        }
            
        // Hi-res render
        w = Mathf.CeilToInt(factor * (float) w);  
        h = Mathf.CeilToInt(factor * (float) h);
        RenderTexture rt = RenderTexture.GetTemporary(w, h, d, f, RenderTextureReadWrite.Default, 1);
        Graphics.Blit(m_cubeRT, rt, m_material);

        // SSAA blit
        Graphics.Blit(rt, dest);

        RenderTexture.ReleaseTemporary(rt);
    }

    //----------------------------------------------------------------------------------------------------
    // PRIVATE MEMBERS
    //----------------------------------------------------------------------------------------------------

    private DomeController m_controller;
    private Material m_material;
    private RenderTexture m_cubeRT;
}
