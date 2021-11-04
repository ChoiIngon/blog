using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets
{
    public class Client : MonoBehaviour
    {
        // Start is called before the first frame update
        public Gamnet.ClientSession session = new Gamnet.ClientSession();
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            Gamnet.SessionEventQueue.Instance.Update();
        }
    }
}