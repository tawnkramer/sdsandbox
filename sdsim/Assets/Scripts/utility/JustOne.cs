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
            if (obj.label == label)
            {
                if(oneFound)
                    Destroy(obj.gameObject);
                else
                    oneFound = true;
            }
        }

        DontDestroyOnLoad(this.gameObject);
    }
}