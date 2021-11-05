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
		public AsyncAction(IEnumerator enumerator, Action action)
		{
			Task.Run(() =>
			{
				action();
				CoroutineCompleteEvent evt = new CoroutineCompleteEvent(null, enumerator);
				SessionEventQueue.Instance.EnqueuEvent(evt);
			});
		}
	}
}
