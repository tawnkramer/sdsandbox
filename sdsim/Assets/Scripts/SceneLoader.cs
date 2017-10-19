using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour {

public void LoadGenerateRoadScene()
{
    SceneManager.LoadSceneAsync(1);
}

public void LoadWarehouseScene()
{
    SceneManager.LoadSceneAsync(2);
}

public void LoadAVCScene()
{
    SceneManager.LoadSceneAsync(3);
}

public void LoadMenuScene()
{
    SceneManager.LoadSceneAsync(0);
}

public void QuitApplication()
{
    Application.Quit();
}

}
