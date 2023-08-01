using System;
using System.Collections.Generic;
using UnityEngine;

namespace Rayleigh.PrefabPool
{
	public sealed class PrefabPool
	{
        /// <summary>
        /// Internal pools, one for each prefab used. The key is the instance ID of the prefab used for the pool.
        /// </summary>
		private readonly Dictionary<int, InternalPool> pools;

        /// <summary>
        /// Stores relations between objects taken from the pool and corresponding internal pools.
        /// </summary>
        private readonly Dictionary<int, InternalPool> relations;

        private static PrefabPool instance;

        public static PrefabPool Instance => instance ??= new();

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

        public void Prewarm(Component prefab, int amount) => this.GetOrCreatePool(prefab).Prewarm(amount);

        public int CountAll(Component prefab) => this.GetOrCreatePool(prefab).CountAll;
        
        public int CountActive(Component prefab) => this.GetOrCreatePool(prefab).CountActive;
        
        public int CountInactive(Component prefab) => this.GetOrCreatePool(prefab).CountInactive;

        public bool TryGet<T>(T prefab, out T obj) where T : Component
        {
            var pool = this.GetOrCreatePool(prefab);
            var result = pool.TryGet(out var cmp);
            obj = (T)cmp;
            if(!result) return false;
            this.relations[obj.GetInstanceID()] = pool;
            return true;
        }

        public T Get<T>(T prefab) where T : Component => this.TryGet(prefab, out var obj)
            ? obj
            : throw new InvalidOperationException("The pool for this prefab has reached its max capacity.");

        public void Release(Component obj)
        {
            if(!this.relations.Remove(obj.GetInstanceID(), out var poolObj))
            {
                throw new InvalidOperationException(
                    "The specified object was not created by this pool, so it cannot be released.");
            }

            poolObj.Release(obj);
        }

        public void ClearInactive(Component prefab) => this.GetOrCreatePool(prefab).ClearInactive();
        
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