using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine;

public class VersionCheck : MonoBehaviour
{

    private string uri = "https://github.com/tawnkramer/gym-donkeycar/releases/latest";
    public string latest;

    void Awake()
    {
        StartCoroutine(GetRequest(uri));
    }


    IEnumerator GetRequest(string _uri)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(_uri))
        {
            yield return webRequest.SendWebRequest();
            uri = webRequest.uri.ToString();
            string[] split = uri.Split('/');
            latest = split[split.Length - 1];
            // Debug.Log(latest);
        }
    }


    public void GetLatestVersion()
    {
        // open the URL in a web browser
        Application.OpenURL(uri);
    }
}
