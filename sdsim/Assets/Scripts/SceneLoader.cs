using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using SimpleFileBrowser;
public class SceneLoader : MonoBehaviour {

    public void LoadMenuScene()
    {
        SceneManager.LoadSceneAsync("menu");
    }

    public void LoadScene(string scene_name)
    {
        SceneManager.LoadSceneAsync(scene_name);
    }

    public void QuitApplication()
    {
        Application.Quit();
    }

    public void SetLogDir()
    {
        // Show a select folder dialog 
        // onSuccess event: print the selected folder's path
        // onCancel event: print "Canceled"
        // Load file/folder: folder, Initial path: default (Documents), Title: "Select Folder", submit button text: "Select"
        FileBrowser.ShowLoadDialog( (path) => { OnSetLogDir(path); }, 
                                        () => { Debug.Log( "Canceled" ); }, 
                                        true, null, "Select Log Folder", "Select" );
    }

    public void OnSetLogDir(string path)
    {
        Debug.Log( "Selected: " + path );
        GlobalState.log_path = path;
    }

}
