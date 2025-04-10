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
		private readonly Action<T> _onCreate;
		private readonly Action<T> _onGet;
		private readonly Action<T> _onRelease;
		private readonly Action<T> _onDestroy;

		/// <inheritdoc cref="IPoolParameters.MaxCapacity"/>
		public int MaxCapacity { get; }

		/// <inheritdoc cref="IPoolParameters.ActivateOnGet"/>
		public bool ActivateOnGet { get; }

		/// <inheritdoc cref="IPoolParameters.GroupReturnedItems"/>
		public bool GroupReturnedItems { get; }

		/// <summary>
		/// Creates new pool parameters.
		/// </summary>
		/// <param name="maxCapacity">The max capacity of the pool. When the pool reaches its max capacity,
		/// any instances returned to the pool will be destroyed and get methods will fail.</param>
		/// <param name="activateOnGet">If set to true, instances taken from the pool will be active even if the prefab
		/// is inactive; otherwise, they will be left as is.</param>
		/// <param name="groupReturned">
		/// If set to true, returned items are parented into an object with a name that matches the prefab name.
		/// </param>
		/// <param name="onCreate">The callback method to be called when the pool creates a new instance.</param>
		/// <param name="onGet">The callback method to be called when an instance is taken from the pool.</param>
		/// <param name="onRelease">The callback method to be called when an instance is returned to the pool.</param>
		/// <param name="onDestroy">The callback method to be called when an instance is destroyed.</param>
		public PoolParameters(int maxCapacity = int.MaxValue, bool activateOnGet = true, bool groupReturned = false,
			Action<T> onCreate = null, Action<T> onGet = null, Action<T> onRelease = null, Action<T> onDestroy = null)
		{
			MaxCapacity = maxCapacity;
			ActivateOnGet = activateOnGet;
			GroupReturnedItems = groupReturned;
			_onCreate = onCreate;
			_onGet = onGet;
			_onRelease = onRelease;
			_onDestroy = onDestroy;
		}

		public void InvokeOnCreate(Component obj) => _onCreate?.Invoke((T)obj);

		public void InvokeOnGet(Component obj) => _onGet?.Invoke((T)obj);

		public void InvokeOnRelease(Component obj) => _onRelease?.Invoke((T)obj);

		public void InvokeOnDestroy(Component obj) => _onDestroy?.Invoke((T)obj);
	}
}