using System;
using System.Collections.Generic;
using UnityEngine;

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
		private readonly Dictionary<int, InternalPool> pools;

        /// <summary>
        /// Stores relations between objects taken from the pool and corresponding internal pools.
        /// </summary>
        private readonly Dictionary<int, InternalPool> relations;

        /// <summary>
        /// Creates new prefab pool.
        /// </summary>
        public PrefabPool()
        {
            this.pools = new();
            this.relations = new();
        }

        /// <summary>
        /// Sets specified parameters for the pool.
        /// </summary>
        /// <param name="prefab">The prefab for which to configure a pool.</param>
        /// <param name="parameters">Parameters to set for the pool.</param>
        /// <typeparam name="T">Type of the prefab.</typeparam>
        /// <exception cref="ArgumentException">Specified max capacity is lower or equal to zero.</exception>
        public void Configure<T>(T prefab, PoolParameters<T> parameters) where T : Component =>
            this.GetOrCreatePool(prefab).SetParameters(parameters);

        /// <summary>
        /// Creates a specified number of prefab instances in the pool. 
        /// </summary>
        /// <param name="prefab">Prefab for which to create instances.</param>
        /// <param name="number">The number of instances to create.</param>
        public void Prewarm(Component prefab, int number) => this.GetOrCreatePool(prefab).Prewarm(number);

        /// <summary>
        /// Returns the total number of active (taken from the pool) and inactive (in the pool) instances
        /// for specified prefab.
        /// </summary>
        /// <param name="prefab">Prefab for which to return the total number of instances.</param>
        /// <returns>The total number of active and inactive instances.</returns>
        public int CountAll(Component prefab) => this.GetOrCreatePool(prefab).CountAll;
        
        /// <summary>
        /// Returns the number of instances created by the pool but are currently in use.
        /// </summary>
        /// <param name="prefab">Prefab for which to return the number of active instances.</param>
        /// <returns>The number of available instances.</returns>
        public int CountActive(Component prefab) => this.GetOrCreatePool(prefab).CountActive;
        
        /// <summary>
        /// Returns the number of instances that are available in the pool.
        /// </summary>
        /// <param name="prefab">Prefab for which to return the number of inactive instances.</param>
        /// <returns>The number of instances available in the pool.</returns>
        public int CountInactive(Component prefab) => this.GetOrCreatePool(prefab).CountInactive;

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
            if(!result) return false;
            this.relations[obj.GetInstanceID()] = pool;
            return true;
        }

        /// <summary>
        /// Gets an instance of specified prefab from the pool.
        /// </summary>
        /// <param name="prefab">Prefab of the instance to get.</param>
        /// <typeparam name="T">Type of the prefab.</typeparam>
        /// <returns>Instance of specified prefab from the pool</returns>
        /// <exception cref="InvalidOperationException">If the max capacity is reached.</exception>
        public T Get<T>(T prefab) where T : Component => this.TryGet(prefab, out var obj)
            ? obj
            : throw new InvalidOperationException("The pool for this prefab has reached its max capacity.");

        /// <summary>
        /// Releases the specified prefab instance back to the pool.
        /// </summary>
        /// <param name="obj">The instance to release.</param>
        /// <exception cref="InvalidOperationException">The specified instance was not created by this pool.</exception>
        public void Release(Component obj)
        {
            if(!this.relations.Remove(obj.GetInstanceID(), out var poolObj))
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
        public void ClearInactive(Component prefab) => this.GetOrCreatePool(prefab).ClearInactive();
        
        /// <summary>
        /// Destroys all instances in the pool.
        /// </summary>
        public void ClearInactive()
        {
            foreach(var pool in this.pools.Values) pool.ClearInactive();
        }

        private InternalPool GetOrCreatePool(Component prefab)
        {
            var prefabId = prefab.GetInstanceID();
            InternalPool pool;
            
            if(this.pools.TryGetValue(prefabId, out var objPool))
            {
                pool = objPool;
            }
            else
            {
                pool = new(prefab, new DefaultPoolParameters());
                this.pools.Add(prefabId, pool);
            }
            
            return pool;
        }
	}
}