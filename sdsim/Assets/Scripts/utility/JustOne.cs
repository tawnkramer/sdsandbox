using UnityEngine;

public class JustOne : MonoBehaviour
{
    // Ensure One and only one JustOne Object that is not deleted when scene exits.
    public string label = "NameMe";

    void Awake()
    {
        JustOne[] objs = GameObject.FindObjectsOfType<JustOne>();

        bool oneFound = false;

        foreach (JustOne obj in objs)
        {
            if (obj.label == label && this == obj && objs.Length > 1)
            {
                Debug.Log("JustOne removing instance." + label);
                GameObject.DestroyImmediate(obj.gameObject);
                return;
            }
        }

        DontDestroyOnLoad(this.gameObject);
    }
}