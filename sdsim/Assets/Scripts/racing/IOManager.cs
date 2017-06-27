using UnityEngine;
using System.Collections;
using SimpleJSON;
using System.Collections.Generic;

public delegate void OnLoadComplete(string url, string res);
public delegate void OnLoadFailed(string url, string res);

public class URLResult
{
	public string res = string.Empty;
	public bool failed = false;
}

public class IOManager : MonoBehaviour {
	
	public static IOManager inst;
	
	// Use this for initialization
	void Start () {
		inst = this;
	}

	public void Get(string url, OnLoadComplete olc, OnLoadFailed olf)
	{
		StartCoroutine(HandleURL(url, olc, olf));
	}

	IEnumerator HandleURL(string url, OnLoadComplete olc, OnLoadFailed olf)
	{
		URLResult result = new URLResult();
		
		yield return StartCoroutine (LoadUrl(url, result));
		
		if(result != null && result.failed)
		{
			if(olf != null)
				olf.Invoke(url, result.res);
		}
		else
		{
			if(olc != null)
				olc.Invoke(url, result.res);
		}
	}	

	IEnumerator LoadUrl(string url, URLResult resultStr = null)
	{
		WWW web = new WWW(url);
		
		yield return web;
		
		if(resultStr != null)
			resultStr.res = web.text;
		
		if(web.error != string.Empty && web.error != null)
		{
			Debug.LogError(web.error);
			
			if(resultStr != null)
			{
				resultStr.failed = true;
				resultStr.res = web.error;
			}
		}
	}	


	public string ParsePayload(string result, string command)
	{
		if(result == string.Empty)
			return null;
		
		var jsonResponse = JSON.Parse(result);
		
		string payload = jsonResponse[command].Value;
		
		return payload;
	}
}

