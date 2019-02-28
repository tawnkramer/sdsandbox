using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System;
using tk;

namespace tk
{
    public class Dispatcher {
        //Name to Message client handling.

        private Dictionary <string, Delegates> eventDictionary;

        public void Init ()
        {
            if (eventDictionary == null)
            {
                eventDictionary = new Dictionary<string, Delegates>();
            }
        }

        public void Register(string msgType, Delegates.OnMsgRecv regCallback)
        {
            Delegates Delegates = null;
        
            if (eventDictionary.TryGetValue (msgType, out Delegates))
            {
                Delegates.onMsgCb += regCallback;
            }
            else
            {
                Delegates newDel = new Delegates();

                newDel.onMsgCb += regCallback;

                eventDictionary.Add(msgType, newDel);
            }
        }

        public void Dipatch(string msgType, JSONObject msgPayload)
        {
            Delegates delegates = null;
        
            if (eventDictionary.TryGetValue (msgType, out delegates))
            {
                delegates.onMsgCb.Invoke(msgPayload);
            }
        }
    }

}