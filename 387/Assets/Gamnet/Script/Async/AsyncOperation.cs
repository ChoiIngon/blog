using System.Collections;

namespace Gamnet.Async
{
    public class AsyncOperation
    {
        public Session session { get; private set; }
        public IEnumerator coroutine { get; private set; }
        public System.Exception Exception { get; protected set; }

        public AsyncOperation(Session session)
        {
            this.session = session;
            this.coroutine = session.current_coroutine;
        }
    }
}
