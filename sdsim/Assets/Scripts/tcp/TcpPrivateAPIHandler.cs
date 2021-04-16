using System.Collections;
using UnityEngine;
using System;
using System.Collections.Generic;

namespace tk
{

    public class TcpPrivateAPIHandler : MonoBehaviour
    {
        private tk.JsonTcpClient client;
        public PathManager pathManager;
        private bool isVerified;

        void Awake()
        {
            pathManager = GameObject.FindObjectOfType<PathManager>();
        }

        public void Init(tk.JsonTcpClient _client)
        {
            client = _client;

            _client.dispatchInMainThread = false; //too slow to wait.
            _client.dispatcher.Register("verify", new tk.Delegates.OnMsgRecv(OnVerify));
            _client.dispatcher.Register("set_random_seed", new tk.Delegates.OnMsgRecv(OnSetRandomSeed));
            _client.dispatcher.Register("reset_challenges", new tk.Delegates.OnMsgRecv(OnResetChallenges));
        }

        public tk.JsonTcpClient GetClients()
        {
            return client;
        }

        bool isPrivateKeyCorrect(string privateKey)
        {
            if (privateKey == GlobalState.privateKey) { return true; }
            else { return false; }
        }

        void OnVerify(JSONObject json)
        {
            if (isPrivateKeyCorrect(json.GetField("private_key").str))
            {
                isVerified = true;
                UnityMainThreadDispatcher.Instance().Enqueue(SendIsVerified());
            }
            else
            {
                UnityMainThreadDispatcher.Instance().Enqueue(sendErrorMessage("private_key_error", "private_key doesn't correspond, please ensure you entered the right one"));
            }
        }
        void OnSetRandomSeed(JSONObject json)
        {
            if (isVerified)
            {
                if (pathManager == null) { pathManager = GameObject.FindObjectOfType<PathManager>(); }

                int new_seed;
                int.TryParse(json.GetField("seed").str, out new_seed);

                GlobalState.seed = new_seed;
                UnityMainThreadDispatcher.Instance().Enqueue(savePlayerPrefsInt("seed", GlobalState.seed));

                if (pathManager != null) { UnityMainThreadDispatcher.Instance().Enqueue(pathManager.InitAfterCarPathLoaded(pathManager.challenges)); }
            }
            else { UnityMainThreadDispatcher.Instance().Enqueue(sendErrorMessage("private_key_error", "private_key doesn't correspond, please ensure you entered the right one")); }
        }

        void OnResetChallenges(JSONObject json)
        {
            if (isVerified) { UnityMainThreadDispatcher.Instance().Enqueue(pathManager.InitAfterCarPathLoaded(pathManager.challenges)); }
            else { UnityMainThreadDispatcher.Instance().Enqueue(sendErrorMessage("private_key_error", "private_key doesn't correspond, please ensure you entered the right one")); }
        }

        public IEnumerator SendIsVerified()
        {
            JSONObject json = new JSONObject(JSONObject.Type.OBJECT);
            json.AddField("msg_type", "verified");
            client.SendMsg(json);
            yield return null;
        }

        public IEnumerator SendCollisionWithStartingLine(string name, int startingLineIndex, float timeStamp)
        {
            if (isVerified)
            {
                JSONObject json = new JSONObject(JSONObject.Type.OBJECT);
                json.AddField("msg_type", "collision_with_starting_line");
                json.AddField("car_name", name);
                json.AddField("starting_line_index", startingLineIndex);
                json.AddField("timeStamp", timeStamp);
                client.SendMsg(json);
            }
            yield return null;
        }
        public IEnumerator SendCollisionWithCone(string name, int coneIndex, float timeStamp)
        {
            if (isVerified)
            {
                JSONObject json = new JSONObject(JSONObject.Type.OBJECT);
                json.AddField("msg_type", "collision_with_cone");
                json.AddField("car_name", name);
                json.AddField("cone_index", coneIndex);
                json.AddField("timeStamp", timeStamp);
                client.SendMsg(json);
            }
            yield return null;
        }
        IEnumerator sendErrorMessage(string msgType, string errorMessage)
        {
            JSONObject json = new JSONObject(JSONObject.Type.OBJECT);
            json.AddField("msg_type", msgType);
            json.AddField("error_message", errorMessage);
            client.SendMsg(json);
            yield return null;
        }

        IEnumerator savePlayerPrefsInt(string key, int value)
        {
            PlayerPrefs.SetInt(key, value);
            PlayerPrefs.Save();
            yield return null;
        }
        IEnumerator savePlayerPrefsFloat(string key, float value)
        {
            PlayerPrefs.SetFloat(key, value);
            PlayerPrefs.Save();
            yield return null;
        }
        IEnumerator savePlayerPrefsString(string key, string value)
        {
            PlayerPrefs.SetString(key, value);
            PlayerPrefs.Save();
            yield return null;
        }
    }
}
