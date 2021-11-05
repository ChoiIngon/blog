using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gamnet
{
	public class AsyncAction
	{
		public AsyncAction(Session session, Action action)
		{
			Task.Run(() =>
			{
                IEnumerator enumerator = session.enumerator;
				action();
				CoroutineCompleteEvent evt = new CoroutineCompleteEvent(session, enumerator);
				SessionEventQueue.Instance.EnqueuEvent(evt);
			});
		}
	}
}
