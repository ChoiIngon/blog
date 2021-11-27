using UnityEngine;

namespace UnityServer
{
    public class Agent : MonoBehaviour
    {
        public Server.Session session;
        public GameObject sphere;

        private void Update()
        {
            if (null != sphere)
            {
                Packet.Packet.MsgSvrCli_SyncPosition_Ntf ntf = new Packet.Packet.MsgSvrCli_SyncPosition_Ntf();
                ntf.y = sphere.transform.position.y;
                session.Send<Packet.Packet.MsgSvrCli_SyncPosition_Ntf>(ntf);
            }
        }
    }
}
