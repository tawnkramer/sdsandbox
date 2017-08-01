using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour {

	public GameObject ground;
	public Camera zoomCam;

	public GameObject explodePrefab; 
	public GameObject goodJobPrefab; 
	public GameObject almostPrefab; 
	public GameObject logo;
	public GameObject lander;
	public HiScorePoster poster;
	//public GameObject levelSummaryPanel;
	//public GameObject hiScorePanel;
	public GameObject OceanLevel;
	public GameObject GroundLevel;


	int fontSize = 40;
	float fElevation = 1.0f;
	float zoom = 10.0f;
	//bool completed = false;
	public bool gameStarted = false; 
	int level = 1;
	int pointsThisLevel = 0;
	int totalPoints = 0;
	int distPoints = 0;
	int fuelPoints = 0;
	int lives = 3;

	GameObject finishObj;

	public enum GameState
	{
		Logo,
		GetUsername,
		LevelStart,
		InPlay,
		LevelCompleted,
		GameOver,
		GameConquered,
	}

	public GameState state = GameState.Logo;

	void Awake()
	{
		//Application.targetFrameRate = 60;

		zoom = zoomCam.orthographicSize;

		if(logo)
			logo.SetActive(true);

		//rocket.gameObject.SetActive(false);

		DontDestroyOnLoad(this);
	}

	void OnGUI() {

		GUI.skin.label.fontSize = fontSize;

		//fElevation = (rocket.transform.position.y - ground.transform.position.y) - 2;
		int iElevation = (int)fElevation;
		string msg = string.Format("Elev: {0}", iElevation);

        Vector2 vel = Vector2.one; // rocket.gameObject.GetComponent<Rigidbody2D>().velocity;

		string msgVel = string.Format("Vel: {0}", (int)vel.y);

        float fuel = 0.0f;

		string msgFuel = string.Format("Fuel: {0}", (int)(fuel / 100.0f));

		GUI.contentColor = Color.white;

		if(state == GameState.InPlay)
		{
			GUI.Label(new Rect(10f, 10f, 200f, 50f), msg);
			GUI.Label(new Rect(10f, 60f, 200f, 50f), msgFuel);

			string livesMsg = string.Format("Lives: {0}", lives);
			GUI.Label(new Rect(Screen.width - 150, 10f, 300f, 50f), livesMsg);

			if(totalPoints > 0)
			{
				string scoreMsg = string.Format("Score: {0}", totalPoints);
				GUI.Label(new Rect((Screen.width/2) - 150, 10f, 300f, 50f), scoreMsg);
			}

			GUI.contentColor = Color.white;

			//if(vel.magnitude > rocket.crashVel)
			//	GUI.contentColor = Color.red;

			GUI.Label(new Rect(10f, 110f, 200f, 50f), msgVel);

			GUI.contentColor = Color.white;
			
			GUI.skin.label.fontSize = 12;

			GUI.Label(new Rect(10.0f, Screen.height - 50.0f, 100f, 50f), "spacebar to reset");
		}
		else if (state == GameState.LevelCompleted)
		{
			GUI.contentColor = Color.white;

			GUI.skin.label.fontSize = 40;

			int y = 25;
			int x = 25;

			if(totalPoints > pointsThisLevel)
			{
				string tscoreMsg = string.Format("Total Points: {0}", totalPoints);
				GUI.Label(new Rect(x, y, 500f, 50f), tscoreMsg);
				y += 50;
			}

			string scoreMsg = string.Format("Lvl Points: {0}", pointsThisLevel);
			GUI.Label(new Rect(x, y, 500f, 50f), scoreMsg);
			y += 50;

			GUI.skin.label.fontSize = 30;

			string landingMsg = string.Format("Landing Points: {0}", distPoints);
			GUI.Label(new Rect(x, y, 400f, 50f), landingMsg);
			y += 30;

			string fuelMsg = string.Format("Fuel Points: {0}", fuelPoints);
			GUI.Label(new Rect(x, y, 400f, 50f), fuelMsg);

			GUI.skin.label.fontSize = 12;

			GUI.Label(new Rect(10.0f, Screen.height - 50.0f, 100f, 50f), "spacebar to continue");
		}
		else if(state == GameState.Logo)
		{
			GUI.skin.label.fontSize = 20;

			GUI.Label(new Rect((Screen.width / 2) - 80, Screen.height - 50.0f, 200f, 50f), "spacebar to start");
			GUI.Label(new Rect((Screen.width / 2) - 95, Screen.height - 80.0f, 200f, 50f), "arrow keys to thrust");
			GUI.Label(new Rect((Screen.width / 2) - 100, Screen.height - 110.0f, 200f, 50f), "Try to land the rocket!");
		}
		else if(state == GameState.LevelStart)
		{
			GUI.skin.label.fontSize = 40;

			//int iStart = level - 1;
            //RocketStart rs = rocketStarts[iStart];
            string levelName = "test";

			string levelMsg = string.Format("Level {0}", level);
			GUI.Label(new Rect((Screen.width / 2) - 200, (Screen.height / 2) - 200.0f, 500f, 50f), levelMsg);

			GUI.Label(new Rect((Screen.width / 2) - 200, (Screen.height / 2) - 150.0f, 500f, 50f), levelName);

			GUI.skin.label.fontSize = 20;

			GUI.Label(new Rect((Screen.width / 2) - 80, Screen.height - 50.0f, 200f, 50f), "spacebar to start");

		}
		else if(state == GameState.GameOver)
		{
			GUI.skin.label.fontSize = 40;			
			GUI.Label(new Rect((Screen.width / 2) - 100, (Screen.height / 2) - 200.0f, 300f, 50f), "Game Over");

			int y = 25;
			int x = 25;
			
			if(totalPoints > 0)
			{
				string tscoreMsg = string.Format("Total Points: {0}", totalPoints);
				GUI.Label(new Rect(x, y, 500f, 50f), tscoreMsg);
				y += 50;
			}
		}
		else if(state == GameState.GameConquered)
		{
			GUI.skin.label.fontSize = 50;			
			GUI.Label(new Rect((Screen.width / 2) - 200, (Screen.height / 2) - 200.0f, 500f, 50f), "You are a bad ass!");

			int y = 25;
			int x = 25;

			if(totalPoints > 0)
			{
				string tscoreMsg = string.Format("Total Points: {0}", totalPoints);
				GUI.Label(new Rect(x, y, 500f, 50f), tscoreMsg);
				y += 50;
			}
		}
	}

	void LastLevelCompleted()
	{
		state = GameState.GameConquered;
		poster.GetOverallLeaderBoard();
	}

	void GameOver()
	{
		state = GameState.GameOver;
		poster.GetOverallLeaderBoard();
	}

	void StartNextLevel()
	{
// 		if(level + 1 > rocketStarts.Length)
// 			LastLevelCompleted();
// 		else
// 		{
// 			lives += 3;
// 			level += 1;
// 			ResetLevel();
// 			state = GameState.LevelStart;
// 			rocket.gameObject.SetActive(false);
// 			ShowLevelStart();
// 		}
	}

	void ResetLevel()
	{
// 		pointsThisLevel = 0;
// 
// 		int iStart = level - 1;
// 		RocketStart rs = rocketStarts[iStart];
// 		LanderStart ls = landerStarts[iStart];
// 
// 		lander.transform.position = ls.transform.position;
// 		lander.transform.rotation = ls.transform.rotation;
// 
// 		if(level == 5 || level == 6)
// 		{
// 			rocket.stationaryVel = 2.0f;
// 			lander.SetActive(false);
// 			OceanLevel.SetActive(true);
// 			GroundLevel.SetActive(false);
// 		}
// 		else
// 		{
// 			rocket.stationaryVel = 1.0f;
// 			lander.SetActive(true);
// 			OceanLevel.SetActive(false);
// 			GroundLevel.SetActive(true);
// 		}
// 
// 		//ResetGame
// 		rocket.gameObject.SetActive(true);
// 		rocket.Reset(rs);
// 		completed = false;
// 
// 		if(finishObj != null)
// 		{
// 			Destroy(finishObj);
// 			finishObj = null;
// 		}
	}

	void ShowLevelStart()
	{
		poster.SetHighlighUserScore(0, totalPoints);
		poster.GetOverallLeaderBoard();
		string lvl = "Lvl_" + level;
		poster.GetLevelLeaderBoard(lvl);
		//levelSummaryPanel.SetActive(true);
		//hiScorePanel.SetActive(true);
	}

	void StartLevelPlay()
	{
		poster.StopShowing();
		//levelSummaryPanel.SetActive(false);
		//hiScorePanel.SetActive(false);
		state = GameState.InPlay;			
		ResetLevel();
	}

 	IEnumerator CalcPoints()
 	{
        // 		int iStart = level - 1;
        // 		RocketStart rs = rocketStarts[iStart];
        // 		float distToLanding = Vector3.Distance(rocket.transform.position, lander.transform.position);
        // 		float fuel = rocket.fuel / 100.0f;
        // 		int minPoints = 100;
        // 		float maxDist = 100.0f;
        // 		float alpha = 1.0f - Mathf.Clamp(distToLanding / maxDist, 0.0f, 1.0f);
        // 		distPoints = (int)Mathf.Lerp(minPoints, rs.levelPoints, alpha);
        // 		fuelPoints = (int)fuel * 10;
        // 		pointsThisLevel = (int)(distPoints + fuelPoints);
        // 		totalPoints += pointsThisLevel;
        // 
        int level = 1;
 		string lvl = "Lvl_" + level;
// 		poster.SetHighlighUserScore(pointsThisLevel, totalPoints);
// 		poster.SaveScore(pointsThisLevel, lvl);
// 		poster.SaveOverallScore(totalPoints);
// 		state = GameState.LevelCompleted;
// 
 		yield return new WaitForSeconds(3.0f);
 		poster.GetOverallLeaderBoard();
 		poster.GetLevelLeaderBoard(lvl);
	}


	void Update()
	{
		if(state == GameState.GetUsername)
		{
			if(poster.finishedEnteringName)
			{
				state = GameState.LevelStart;
				ShowLevelStart();
			}
		}

		if(Input.GetButtonDown("Jump"))
		{
			if(state == GameState.Logo)
			{
				if(logo)
				{
					Destroy(logo);
					logo = null;
				}

//#if !UNITY_EDITOR
				state = GameState.GetUsername;
				poster.ShowGetUsername();
//#else
//				state = GameState.LevelStart;
//#endif
			}
			else if(state == GameState.GetUsername)
			{

			}
			else if(state == GameState.LevelStart)
			{
				StartLevelPlay();
			}
			else if(state == GameState.InPlay)
			{
				ResetLevel();
			}
			else if(state == GameState.LevelCompleted)
			{
				StartNextLevel();
			}
		}

// 		if(rocket.exploded && rocket.gameObject.activeInHierarchy)
// 		{
// 			rocket.gameObject.SetActive(false);
// 			GameObject ob = Instantiate(explodePrefab, rocket.transform.position, Quaternion.identity) as GameObject;
// 			Destroy(ob, 5.0f);
// 			lives -= 1;
// 
// 			if(lives == 0)
// 				GameOver();
// 		}
// 
// 		if(!rocket.exploded && rocket.landed && !completed)
// 		{
// 			Debug.Log("You landed!!");
// 			completed = true;
// 			Vector3 pos = rocket.transform.position;
// 			pos += new Vector3(0.0f, 2.0f, -2.0f);
// 		
// 			if(rocket.upright)
// 			{
// 				finishObj = Instantiate(goodJobPrefab, pos, Quaternion.identity) as GameObject;
// 				StartCoroutine(CalcPoints());
// 			}
// 			else
// 				finishObj = Instantiate(almostPrefab, pos, Quaternion.identity) as GameObject;
// 
// 			Destroy(finishObj, 5.0f);
// 		}
	}

	void LateUpdate()
	{
		float minSize = 10.0f;
		float maxSize = 100.0f;
		float maxElevation = 100.0f;
		float alpha = Mathf.Clamp(fElevation / maxElevation, 0.0f, 1.0f);
		float newZoom = Mathf.Lerp(minSize, maxSize, alpha );
		zoom = Mathf.Lerp(zoom, newZoom, 0.1f);
		zoomCam.orthographicSize = zoom;
	}
}
