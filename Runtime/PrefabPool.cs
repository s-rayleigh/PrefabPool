using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Rayleigh.PrefabPool
{
    /// <summary>
    /// Prefab-oriented object pool.
    /// </summary>
	public class PrefabPool
	{
        /// <summary>
        /// Internal pools, one for each prefab used. The key is the instance ID of the prefab used for the pool.
        /// </summary>
		private readonly Dictionary<int, InternalPool> _pools;

        /// <summary>
        /// Stores relations between objects taken from the pool and corresponding internal pools.
        /// </summary>
        private readonly Dictionary<int, InternalPool> _relations;

        /// <summary>
        /// An object used as a parent for items returned to the pool.
        /// </summary>
        [MaybeNull]
        private readonly Transform _itemsParent;
        
        /// <summary>
        /// Creates new prefab pool.
        /// </summary>
        /// <param name="itemsParentName">
        /// Name of the object to use as a parent for items returned to the pool.
        /// Null or empty string will result in not creating such an object.
        /// </param>
        public PrefabPool(string itemsParentName = null)
        {
            _pools = new();
            _relations = new();
            if (!string.IsNullOrEmpty(itemsParentName))
            {
                var parentObject = new GameObject(itemsParentName);
                Object.DontDestroyOnLoad(parentObject);
                parentObject.SetActive(false);
                _itemsParent = parentObject.transform;
            }
        }

        /// <summary>
        /// Sets specified parameters for the pool.
        /// </summary>
        /// <param name="prefab">The prefab for which to configure a pool.</param>
        /// <param name="parameters">Parameters to set for the pool.</param>
        /// <typeparam name="T">Type of the prefab.</typeparam>
        /// <exception cref="ArgumentException">Specified max capacity is lower or equal to zero.</exception>
        public void Configure<T>(T prefab, PoolParameters<T> parameters) where T : Component =>
            GetOrCreatePool(prefab).SetParameters(parameters);

        /// <summary>
        /// Creates a specified number of prefab instances in the pool. 
        /// </summary>
        /// <param name="prefab">Prefab for which to create instances.</param>
        /// <param name="number">The number of instances to create.</param>
        public void Prewarm(Component prefab, int number) => GetOrCreatePool(prefab).Prewarm(number);

        /// <summary>
        /// Returns the total number of active (taken from the pool) and inactive (in the pool) instances
        /// for specified prefab.
        /// </summary>
        /// <param name="prefab">Prefab for which to return the total number of instances.</param>
        /// <returns>The total number of active and inactive instances.</returns>
        public int CountAll(Component prefab) => GetOrCreatePool(prefab).CountAll;
        
        /// <summary>
        /// Returns the number of instances created by the pool but are currently in use.
        /// </summary>
        /// <param name="prefab">Prefab for which to return the number of active instances.</param>
        /// <returns>The number of available instances.</returns>
        public int CountActive(Component prefab) => GetOrCreatePool(prefab).CountActive;
        
        /// <summary>
        /// Returns the number of instances that are available in the pool.
        /// </summary>
        /// <param name="prefab">Prefab for which to return the number of inactive instances.</param>
        /// <returns>The number of instances available in the pool.</returns>
        public int CountInactive(Component prefab) => GetOrCreatePool(prefab).CountInactive;

        /// <summary>
        /// Gets an instance of specified prefab from the pool.
        /// </summary>
        /// <param name="prefab">Prefab of the instance to get.</param>
        /// <param name="obj">When the method returns, contains the instance of the specified prefab if the max capacity
        /// is not exceeded; otherwise, null. This parameter is passed uninitialized.</param>
        /// <typeparam name="T">Type of the prefab.</typeparam>
        /// <returns>True if the max capacity is not exceeded; otherwise, false.</returns>
        public bool TryGet<T>(T prefab, out T obj) where T : Component
        {
            var pool = this.GetOrCreatePool(prefab);
            var result = pool.TryGet(out var cmp);
            obj = (T)cmp;
            if (!result)
                return false;
            _relations[obj.GetInstanceID()] = pool;
            return true;
        }

        /// <summary>
        /// Gets an instance of specified prefab from the pool.
        /// </summary>
        /// <param name="prefab">Prefab of the instance to get.</param>
        /// <typeparam name="T">Type of the prefab.</typeparam>
        /// <returns>Instance of a specified prefab from the pool</returns>
        /// <exception cref="InvalidOperationException">If the max capacity is reached.</exception>
        public T Get<T>(T prefab) where T : Component => TryGet(prefab, out var obj)
            ? obj
            : throw new InvalidOperationException("The pool for this prefab has reached its max capacity.");

        /// <summary>
        /// Releases the specified prefab instance back to the pool.
        /// </summary>
        /// <param name="obj">The instance to release.</param>
        /// <exception cref="InvalidOperationException">The specified instance was not created by this pool.</exception>
        public void Release(Component obj)
        {
            if (!_relations.Remove(obj.GetInstanceID(), out var poolObj))
            {
                throw new InvalidOperationException(
                    "The specified object was not created by this pool, so it cannot be released.");
            }

            poolObj.Release(obj);
        }

        /// <summary>
        /// Destroys all instances in the pool for specified prefab.
        /// </summary>
        /// <param name="prefab">Prefab which instances to destroy.</param>
        public void ClearInactive(Component prefab) => GetOrCreatePool(prefab).ClearInactive();

        /// <summary>
        /// Destroys all instances in the pool.
        /// </summary>
        public void ClearInactive()
        {
            foreach (var pool in _pools.Values)
                pool.ClearInactive();
        }

        private InternalPool GetOrCreatePool(Component prefab)
        {
            var prefabId = prefab.GetInstanceID();

            if (_pools.TryGetValue(prefabId, out var objPool))
                return objPool;

            var pool = new InternalPool(prefab, new DefaultPoolParameters(), _itemsParent);
            _pools.Add(prefabId, pool);

            return pool;
        }
    }
}