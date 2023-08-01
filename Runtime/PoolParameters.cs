using System;
using UnityEngine;

namespace Rayleigh.PrefabPool
{
	public readonly struct PoolParameters<T> : IPoolParameters where T : Component
	{
		public readonly int maxCapacity;

		public readonly bool activateOnGet;
		
		public readonly Action<T> onCreate;

		public readonly Action<T> onGet;

		public readonly Action<T> onRelease;

		public readonly Action<T> onDestroy;

		public int MaxCapacity => this.maxCapacity;
		
		public bool ActivateOnGet => this.activateOnGet;
		
		public PoolParameters(int maxCapacity = int.MaxValue, bool activateOnGet = true, Action<T> onCreate = null,
			Action<T> onGet = null, Action<T> onRelease = null, Action<T> onDestroy = null)
		{
			this.maxCapacity = maxCapacity;
			this.activateOnGet = activateOnGet;
			this.onCreate = onCreate;
			this.onGet = onGet;
			this.onRelease = onRelease;
			this.onDestroy = onDestroy;
		}

		public void InvokeOnCreate(Component obj) => this.onCreate?.Invoke((T)obj);

		public void InvokeOnGet(Component obj) => this.onGet?.Invoke((T)obj);

		public void InvokeOnRelease(Component obj) => this.onRelease?.Invoke((T)obj);

		public void InvokeOnDestroy(Component obj) => this.onDestroy?.Invoke((T)obj);
	}
}