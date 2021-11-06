using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gamnet.Async
{
    public class AsyncOperation
    {
        public Session session { get; private set; }
        public IEnumerator enumerator { get; private set; }
        public System.Exception Exception { get; protected set; }

        public AsyncOperation(Session session)
        {
            this.session = session;
            this.enumerator = session.enumerator;
        }
    }
}
