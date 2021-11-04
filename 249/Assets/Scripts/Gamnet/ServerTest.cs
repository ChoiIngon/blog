using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Gamnet
{
    public class ServerTest : MonoBehaviour
    {
        public string Host;
        public int Port;
        public int SessionCount;
        public int LoopCount;

        private void Start()
        {
            BinaryFormatter bf = new BinaryFormatter();
            Assets.Scripts.Message message = new Assets.Scripts.Message();
            message.greeting = "Hello World";

            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            bf.Serialize(ms, message);
            ms.Position = 0;
            Assets.Scripts.Message message2 = (Assets.Scripts.Message)bf.Deserialize(ms);
            Gamnet.Log.Write(Gamnet.Log.LogLevel.DEV, message2.greeting);
        }

        [EditorGUI(EditorGUIAttribute.GUIType.Button, "테스트 실행")]
        public void Run()
        {
            for (int i = 0; i < SessionCount; i++)
            {
                GameObject go = new GameObject();
                go.transform.SetParent(transform);

                Assets.Client client = go.AddComponent<Assets.Client>();
                client.session = new Gamnet.ClientSession();
                client.session.RegisterHandler<Assets.Scripts.Message>(1, (Assets.Scripts.Message msg) =>
                {
                    Debug.Log(msg.greeting);
                });

                client.session.OnConnectEvent += () =>
                {
                    Assets.Scripts.Message message = new Assets.Scripts.Message();
                    message.Id = 1;
                    message.greeting = "Hello World";
                    client.session.AsyncSend(message);
                };
                client.session.AsyncConnect(Host, Port);
            }
        }

        private void Update()
        {

        }
    }
}
