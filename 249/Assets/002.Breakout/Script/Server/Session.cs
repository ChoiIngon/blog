using Gamnet;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Breakout.Server
{
    public class Session : Gamnet.Server.Session
    {
        protected override void OnConnect()
        {
            Debug.Log("onConnect");
        }

        protected override void OnPause()
        {
        }

        protected override void OnResume()
        {
        }

        protected override void OnClose()
        {
            Debug.Log("server onclose");
        }

        public Room room;
    }
}