using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

public class TrackParams
{
    public enum State
    {
        Straight,
        CurveX,
        CurveY,
        CurveZ,
        SpeedLimit,
        AngleDX, //when changing the dx curve setting
        AngleDY, //when changing the dy curve setting
        AngleDZ, //when changing the dz curve setting
        ForkA,   //when laying a track element that splits in two.
        ForkB,   //when starting the second line that splits from the fork
        MergeA,  //when we should look for the nearest track to merge from a fork
        MergeB,  //when we should look for the nearest track to merge from a fork
        End     //terminate current line
    }

    public State state;
    public int numToSet;
    public Quaternion rotCur;
    public Quaternion dRot;
    public Vector3 lastPos;
}

[System.Serializable]
public class TrackScriptElem
{
    public TrackParams.State state;
    public int numToSet;
    public float value;

    public TrackScriptElem(TrackParams.State s = TrackParams.State.Straight, float si = 1.0f, int num = 1)
    {
        state = s;
        numToSet = num;
        value = si;
    }
}

public class TrackScript
{
    public List<TrackScriptElem> track;

    public void Build(TrackScriptElem el)
    {
        if(track.Count == 0)
        {
            track.Add(el);
        }
        else
        {
            TrackScriptElem lastElem = track[track.Count - 1];

            if(lastElem.state == el.state && lastElem.value == el.value)
            {
                lastElem.numToSet += 1;
            }
            else
            {
                track.Add(el);
            }
        }
    }

    public bool Write(string filename)
    {
        StringBuilder sb = new StringBuilder();

        System.IO.File.WriteAllText(filename, sb.ToString());

        return true;
    }

    public bool Read(string filename)
    {
        track = new List<TrackScriptElem>();

        Debug.Log("loading: " + filename);

        TextAsset bindata = Resources.Load(filename) as TextAsset;

		if(bindata == null)
			return false;

        string[] lines = bindata.text.Split('\n');

        foreach(string line in lines)
        {
            string[] tokens = line.Split(' ');

            if (tokens.Length < 2)
                continue;

            string command = tokens[0];
            string args = tokens[1];

            if (command.StartsWith("//"))
                continue;

            TrackScriptElem tse = new TrackScriptElem();

            if (command == "U")
            {
                tse.state = TrackParams.State.CurveZ;
                tse.value = 1f;
                tse.numToSet = int.Parse(args);
            }
            else if(command == "S")
            {
                tse.state = TrackParams.State.Straight;
                tse.value = 1f;
                tse.numToSet = int.Parse(args);
            }
            else if (command == "D")
            {
                tse.state = TrackParams.State.CurveZ;
                tse.value = -1f;
                tse.numToSet = int.Parse(args);
            }
            else if (command == "L")
            {
                tse.state = TrackParams.State.CurveY;
                tse.value = -1f;
                tse.numToSet = int.Parse(args);
            }
            else if (command == "R")
            {
                tse.state = TrackParams.State.CurveY;
                tse.value = 1f;
                tse.numToSet = int.Parse(args);
            }
            else if (command == "RL")
            {
                tse.state = TrackParams.State.CurveX;
                tse.value = 1f;
                tse.numToSet = int.Parse(args);
            }
            else if (command == "RR")
            {
                tse.state = TrackParams.State.CurveX;
                tse.value = -1f;
                tse.numToSet = int.Parse(args);
            }
            else if (command == "SPEED_LIMIT")
            {
                tse.state = TrackParams.State.SpeedLimit;
                tse.value = float.Parse(args);
                tse.numToSet = 0;
            }
            else if (command == "DX")
            {
                tse.state = TrackParams.State.AngleDX;
                tse.value = float.Parse(args);
                tse.numToSet = 0;
            }
            else if (command == "DY")
            {
                tse.state = TrackParams.State.AngleDY;
                tse.value = float.Parse(args);
                tse.numToSet = 0;
            }
            else if (command == "DZ")
            {
                tse.state = TrackParams.State.AngleDZ;
                tse.value = float.Parse(args);
                tse.numToSet = 0;
            }
            else if (command == "FORK_A")
            {
                tse.state = TrackParams.State.ForkA;
                tse.value = float.Parse(args);
                tse.numToSet = 0;
            }
            else if (command == "FORK_B")
            {
                tse.state = TrackParams.State.ForkB;
                tse.value = float.Parse(args);
                tse.numToSet = 0;
            }
            else if (command == "MERGE_A")
            {
                tse.state = TrackParams.State.MergeA;
                tse.value = float.Parse(args);
                tse.numToSet = 0;
            }
            else if (command == "MERGE_B")
            {
                tse.state = TrackParams.State.MergeB;
                tse.value = float.Parse(args);
                tse.numToSet = 0;
            }
            else if (command == "END")
            {
                tse.state = TrackParams.State.End;
                tse.value = float.Parse(args);
                tse.numToSet = 0;
            }
            else
            {
                Debug.Log("unknown command: " + command);
                continue;
            }

            track.Add(tse);
        }

		return track.Count > 0;
    }
}