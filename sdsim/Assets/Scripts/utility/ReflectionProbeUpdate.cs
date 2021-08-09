using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Custom update rate for reflection probe
/// </summary>
[RequireComponent(typeof(ReflectionProbe))]
public class ReflectionProbeUpdate : MonoBehaviour
{
    [Tooltip("Reflection update rate in second")]
    public float updateRate = 0.1f;

    private ReflectionProbe _reflectionProbe;
    private RenderTexture _targetTexture;
    private bool _isRunning = false;

    private void Awake()
    {
        _reflectionProbe = GetComponent<ReflectionProbe>();

        if (_reflectionProbe.refreshMode != ReflectionProbeRefreshMode.ViaScripting)
            _reflectionProbe.refreshMode = ReflectionProbeRefreshMode.ViaScripting;
    }

    private void OnEnable()
    {
        StartCoroutine(UpdateReflection());
    }

    IEnumerator UpdateReflection()
    {
        while (true)
        {
            _reflectionProbe.RenderProbe(_targetTexture = null);
            yield return new WaitForSeconds(updateRate);
        }
    }
}
