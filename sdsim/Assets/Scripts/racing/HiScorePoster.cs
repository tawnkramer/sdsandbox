using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using SimpleJSON;

public struct Score
{
	public string username;
	public double score;
}

public class HiScorePoster : MonoBehaviour
{
	public string userName = "User";
	
	public int max = 10;
	public bool showing = false;
	public static HiScorePoster instance;
	public List<Score> lvlHiScores;
	public List<Score> overallHiScores;
	public bool finishedEnteringName = true;

	public GameObject EnterNamePanel;
	public UnityEngine.UI.InputField Name;
	public UnityEngine.UI.Text warningOnNameLength;

	string levelName = "Lvl1";
	public int longUserName = 12;
	public int medUserName = 8;

	double highLightScoreLvl = 0;
	double highLightScoreOverall = 0;

	public void OnLoadCompleteLevel(string url, string res)
	{
		lvlHiScores = new List<Score>();
		AddScoresToList(url, res, lvlHiScores);
	}

	public void OnLoadCompleteOverall(string url, string res)
	{
		overallHiScores = new List<Score>();
		AddScoresToList(url, res, overallHiScores);
	}

	public void AddScoresToList(string url, string res, List<Score> scoreList)
	{
		var jsonResponse = JSON.Parse(res);
		
		JSONNode N = jsonResponse["scores"]["alltime"];
		
		if(N != null && N.AsArray != null)
		{
			foreach(JSONNode node in N.AsArray)
			{
				if(node.Value != null)
				{
					Score s = new Score();
					s.score = node["score"].AsInt;
					s.username = node["nickname"].Value;
					
					scoreList.Add(s);
				}
			}
		}
		
		showing = true;
	}

	public void ShowGetUsername()
	{
		Name.text = userName;
		EnterNamePanel.SetActive(true);
		finishedEnteringName = false;
	}

	bool IsValidChar(char chr)
	{
		if ( (chr < 'a' || chr > 'z') && (chr < 'A' || chr > 'Z') && (chr < '0' || chr > '9') )
			return false;

		return true;
	}

	public bool IsValidUsername(string name)
	{
		if(name.Length > 15)
			return false;

		for(int i = 0; i < name.Length; i++)
		{
			char chr = name[i];

			if(!IsValidChar(chr))
				return false;
		}

		return true;
	}

	public void OnNameEntered()
	{
		userName = "User";

		if(IsValidUsername(Name.text))
		{
			userName = Name.text;
		}
		else
		{
			warningOnNameLength.color = Color.red;
		}

		if(userName != "User")
		{
			EnterNamePanel.SetActive(false);

			PlayerPrefs.SetString("userName", userName);
			PlayerPrefs.Save();
			finishedEnteringName = true;
		}
	}

	public void OnSaveComplete(string url, string res)
	{
		//parse
	}


	public void OnUrlFailed(string key, string res)
	{
		//error msg
	}
		
	void Awake()
	{
		instance = this;

		userName = PlayerPrefs.GetString("userName", userName);
	}

	//string host = "http://localhost:8888";
	string host = "http://inner-legacy-91719.appspot.com";

	public void SaveScore(double score, string level)
	{
		string url = string.Format("{0}/add/RocketLander_{1}/{2}/{3}", host, level, score, userName);	

		IOManager.inst.Get(url, this.OnSaveComplete, this.OnUrlFailed);
	}

	public void SaveOverallScore(double score)
	{
		string url = string.Format("{0}/add/RocketLander/{1}/{2}", host, score, userName);	
		
		IOManager.inst.Get(url, this.OnSaveComplete, this.OnUrlFailed);
	}
	
	public void GetLevelLeaderBoard(string level)
	{
		levelName = level;

		string url = host + "/get/RocketLander_" + levelName;

		IOManager.inst.Get(url, this.OnLoadCompleteLevel, this.OnUrlFailed);
	}

	public void GetOverallLeaderBoard()
	{
		string url = host + "/get/RocketLander";
		
		IOManager.inst.Get(url, this.OnLoadCompleteOverall, this.OnUrlFailed);
	}

	public void SetHighlighUserScore(double scoreLvl, double scoreTot)
	{
		highLightScoreLvl = scoreLvl;
		highLightScoreOverall = scoreTot;
	}

	public void StopShowing()
	{
		showing = false;
	}

	void OnGUI()
	{
		if(showing && lvlHiScores != null)
		{
			GUI.contentColor = Color.white;			
			GUI.skin.label.fontSize = 30;

			int x = 30;
			int y = 200;

			GUI.Label(new Rect(x, y, 300f, 50f), levelName + " Scores");
			y += 40;

			GUI.skin.label.fontSize = 20;
			bool foundUser = false;

			foreach( Score s in lvlHiScores)
			{
				if(highLightScoreLvl == s.score && s.username == userName)
				{
					GUI.contentColor = Color.red;
					foundUser = true;
				}
				else
					GUI.contentColor = Color.white;

				GUI.skin.label.fontSize = 20;
				if(s.username.Length > longUserName)
					GUI.skin.label.fontSize = 12;
				else if(s.username.Length > medUserName)
					GUI.skin.label.fontSize = 16;

				GUI.Label(new Rect(x, y, 100f, 50f), s.username);

				GUI.skin.label.fontSize = 20;
				GUI.Label(new Rect(x + 100, y, 100f, 50f), string.Format("{0}", s.score));
				y += 20;
			}

			if(!foundUser && highLightScoreLvl != 0)
			{
				y += 20;

				GUI.skin.label.fontSize = 20;
				if(userName.Length > longUserName)
					GUI.skin.label.fontSize = 12;
				else if(userName.Length > medUserName)
					GUI.skin.label.fontSize = 16;

				GUI.Label(new Rect(x, y, 100f, 50f), userName);
				GUI.skin.label.fontSize = 20;
				GUI.Label(new Rect(x + 100, y, 100f, 50f), string.Format("{0}", highLightScoreLvl));
			}
		}

		if(showing && overallHiScores != null)
		{
			GUI.contentColor = Color.white;			
			GUI.skin.label.fontSize = 30;
			
			int x = Screen.width - 300;
			int y = 200;
			
			GUI.Label(new Rect(x, y, 300f, 50f), "High Scores");
			y += 40;
			
			GUI.skin.label.fontSize = 20;
			bool foundUser = false;
			
			foreach( Score s in overallHiScores)
			{
				if(highLightScoreOverall == s.score && s.username == userName)
				{
					GUI.contentColor = Color.red;
					foundUser = true;
				}
				else
					GUI.contentColor = Color.white;

				GUI.skin.label.fontSize = 20;
				if(s.username.Length > longUserName)
					GUI.skin.label.fontSize = 12;
				else if(s.username.Length > medUserName)
					GUI.skin.label.fontSize = 16;

				GUI.Label(new Rect(x, y, 100f, 50f), s.username);

				GUI.skin.label.fontSize = 20;
				GUI.Label(new Rect(x + 100, y, 100f, 50f), string.Format("{0}", s.score));
				y += 20;
			}

			if(!foundUser && highLightScoreOverall != 0)
			{
				y += 20;

				GUI.skin.label.fontSize = 20;
				if(userName.Length > longUserName)
					GUI.skin.label.fontSize = 12;
				else if(userName.Length > medUserName)
					GUI.skin.label.fontSize = 16;

				GUI.Label(new Rect(x, y, 100f, 50f), userName);

				GUI.skin.label.fontSize = 20;
				GUI.Label(new Rect(x + 100, y, 100f, 50f), string.Format("{0}", highLightScoreOverall));
			}
		}
	}
}
