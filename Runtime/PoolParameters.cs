using System;
using UnityEngine;

namespace Rayleigh.PrefabPool
{
	/// <summary>
	/// Pool parameters specific for prefab of the type <see cref="T"/>.
	/// </summary>
	/// <typeparam name="T">Type of the prefab.</typeparam>
	public readonly struct PoolParameters<T> : IPoolParameters where T : Component
	{
		private readonly int maxCapacity;

		private readonly bool activateOnGet;

		private readonly Action<T> onCreate;

		private readonly Action<T> onGet;

		private readonly Action<T> onRelease;

		private readonly Action<T> onDestroy;

		/// <summary>
		/// The max capacity of the pool. When the pool reaches its max capacity,
		/// any instances returned to the pool will be destroyed and get methods will fail.
		/// </summary>
		public int MaxCapacity => this.maxCapacity;
		
		/// <summary>
		/// If set to true, instances taken from the pool will be active even if the prefab is inactive; otherwise,
		/// they will be left as is.
		/// </summary>
		public bool ActivateOnGet => this.activateOnGet;
		
		/// <summary>
		/// Creates new pool parameters.
		/// </summary>
		/// <param name="maxCapacity">The max capacity of the pool. When the pool reaches its max capacity,
		/// any instances returned to the pool will be destroyed and get methods will fail.</param>
		/// <param name="activateOnGet">If set to true, instances taken from the pool will be active even if the prefab
		/// is inactive; otherwise, they will be left as is.</param>
		/// <param name="onCreate">The callback method to be called when the pool creates a new instance.</param>
		/// <param name="onGet">The callback method to be called when an instance is taken from the pool.</param>
		/// <param name="onRelease">The callback method to be called when an instance is returned to the pool.</param>
		/// <param name="onDestroy">The callback method to be called when an instance is destroyed.</param>
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