using System.Collections;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;

namespace Assets.Scripts.Packet
{
    class HelloWorld : Gamnet.PacketHandler<Gamnet.ServerSession>
    {
        public override uint Id()
        {
            return 1;
        }

        public override IEnumerator OnReceive(Gamnet.ServerSession session, Gamnet.Packet req)
        {
            Assets.Scripts.Message message = req.Deserialize<Assets.Scripts.Message>();

            Gamnet.Log.Write(Gamnet.Log.LogLevel.DEV, message.greeting);

            yield return new Gamnet.AsyncAction(session, () =>
            {
                Thread.Sleep(1000);
            });

            message.greeting = "Thanks";

            Gamnet.Packet ans = new Gamnet.Packet();
            ans.Id = 1;
            ans.Serialize(message);
            session.AsyncSend(ans);
            yield break;
        }
    }
}
