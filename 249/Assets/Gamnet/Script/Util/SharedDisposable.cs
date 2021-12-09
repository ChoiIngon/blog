using System;
using System.Collections.Generic;
using System.Linq;

namespace Gamnet.Util
{
	public class SharedDisposable<T> : IDisposable where T : class
	{
		private class Target
		{
			public T item;
			public uint count;
		}

		private Target target = null;
		public SharedDisposable(T item)
		{
			if (null == item)
			{
				return;
			}
			target = new Target() { item = item, count = 1 };
		}
		public SharedDisposable(SharedDisposable<T> shared)
		{
			target = shared.target;
			target.count++;
		}

		public SharedDisposable(object[] ctorArgs = null)
		{
			if (null == ctorArgs)
			{
				ctorArgs = new object[] { };
			}

			var ctor = typeof(SharedDisposable<T>).GetConstructor(ctorArgs.Select(a => a.GetType()).ToArray());
			target = new Target() { item = (T)ctor.Invoke(ctorArgs), count = 1 };
		}

		public SharedDisposable(KeyValuePair<Type, object>[] ctorArgs)
		{
			var ctor = typeof(SharedDisposable<T>).GetConstructor(ctorArgs.Select(a => a.Key).ToArray());
			target = new Target() { item = (T)ctor.Invoke(ctorArgs.Select(a => a.Value).ToArray()), count = 1 };
		}

		~SharedDisposable()
		{
			Dispose();
		}

		public SharedDisposable<T> Share()
		{
			return new SharedDisposable<T>(this);
		}

		public void Dispose()
		{
			if (null == target)
			{
				return;
			}

			if(0 == --target.count)
			{
				target = null;
				UnityEngine.Debug.Log("dispose");
			}
		}

		public T Get()
		{
			return target?.item;
		}

		public static implicit operator T(SharedDisposable<T> shared) => shared.Get();
	}
}
