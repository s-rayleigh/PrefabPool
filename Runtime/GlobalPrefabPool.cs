using System;
using UnityEngine;

namespace Rayleigh.PrefabPool
{
	/// <summary>
	/// Global prefab pool. It's lazy, so if you don't need it, just don't use it and create
	/// an object of <see cref="PrefabPool"/>.
	/// </summary>
	public static class GlobalPrefabPool
	{
		private static PrefabPool _instance;

		/// <summary>
		/// An instance of the global prefab pool.
		/// </summary>
		public static PrefabPool Instance => _instance ??= new();

		/// <summary>
		/// Initializes a global pool by creating a prefab pool instance. 
		/// </summary>
		/// <param name="factory">Factory method to create an instance of the <see cref="PrefabPool"/>.</param>
		/// <exception cref="InvalidOperationException">If global pool already initialized.</exception>
		/// <exception cref="ArgumentNullException">If the factory method is null.</exception>
		public static void Init(Func<PrefabPool> factory)
		{
			if (_instance is not null)
				throw new InvalidOperationException("The instance already created.");
			if (factory is null)
				throw new ArgumentNullException(nameof(factory));
			_instance = factory();
		}
		
		/// <summary>
		/// Sets specified parameters for the global pool.
		/// </summary>
		/// <param name="prefab">The prefab for which to configure a pool.</param>
		/// <param name="parameters">Parameters to set for the pool.</param>
		/// <typeparam name="T">Type of the prefab.</typeparam>
		/// <exception cref="ArgumentException">Specified max capacity is lower or equal to zero.</exception>
		public static void Configure<T>(T prefab, PoolParameters<T> parameters) where T : Component =>
			Instance.Configure(prefab, parameters);

		/// <summary>
		/// Creates a specified number of prefab instances in the global pool. 
		/// </summary>
		/// <param name="prefab">Prefab for which to create instances.</param>
		/// <param name="number">The number of instances to create.</param>
		public static void Prewarm<T>(T prefab, int number) where T : Component => Instance.Prewarm(prefab, number);

		/// <summary>
		/// Returns the total number of active (taken from the pool) and inactive (in the pool) instances
		/// for specified prefab.
		/// </summary>
		/// <param name="prefab">Prefab for which to return the total number of instances.</param>
		/// <returns>The total number of active and inactive instances.</returns>
		public static int CountAll<T>(T prefab) where T : Component => Instance.CountAll(prefab);

		/// <summary>
		/// Returns the number of instances created by the global pool but are currently in use.
		/// </summary>
		/// <param name="prefab">Prefab for which to return the number of active instances.</param>
		/// <returns>The number of available instances.</returns>
		public static int CountActive<T>(T prefab) where T : Component => Instance.CountActive(prefab);

		/// <summary>
		/// Returns the number of instances that are available in the global pool.
		/// </summary>
		/// <param name="prefab">Prefab for which to return the number of inactive instances.</param>
		/// <returns>The number of instances available in the pool.</returns>
		public static int CountInactive<T>(T prefab) where T : Component => Instance.CountInactive(prefab);

		/// <summary>
		/// Gets an instance of specified prefab from the global pool.
		/// </summary>
		/// <param name="prefab">Prefab of the instance to get.</param>
		/// <param name="obj">When the method returns, contains the instance of the specified prefab if the max capacity
		/// is not exceeded; otherwise, null. This parameter is passed uninitialized.</param>
		/// <typeparam name="T">Type of the prefab.</typeparam>
		/// <returns>True if the max capacity is not exceeded; otherwise, false.</returns>
		public static bool TryGet<T>(T prefab, out T obj) where T : Component => Instance.TryGet(prefab, out obj);

		/// <summary>
		/// Gets an instance of specified prefab from the global pool.
		/// </summary>
		/// <param name="prefab">Prefab of the instance to get.</param>
		/// <typeparam name="T">Type of the prefab.</typeparam>
		/// <returns>Instance of specified prefab from the pool</returns>
		/// <exception cref="InvalidOperationException">If the max capacity is reached.</exception>
		public static T Get<T>(T prefab) where T : Component => Instance.Get(prefab);

		/// <summary>
		/// Releases the specified prefab instance back to the global pool.
		/// </summary>
		/// <param name="obj">The instance to release.</param>
		/// <exception cref="InvalidOperationException">The specified instance was not created by this pool.</exception>
		public static void Release<T>(T obj) where T : Component => Instance.Release(obj);

		/// <summary>
		/// Destroys all instances in the global pool for specified prefab.
		/// </summary>
		/// <param name="prefab">Prefab which instances to destroy.</param>
		public static void ClearInactive<T>(T prefab) where T : Component => Instance.ClearInactive(prefab);

		/// <summary>
		/// Destroys all instances in the global pool.
		/// </summary>
		public static void ClearInactive() => Instance.ClearInactive();
	}
}