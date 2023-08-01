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
		private readonly Dictionary<int, object> pools;

        /// <summary>
        /// Stores relations between objects taken from the pool and corresponding internal pools.
        /// </summary>
        private readonly Dictionary<int, object> relations;

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

        public void Prewarm<T>(T prefab, int amount) where T : Component =>
            this.GetOrCreatePool(prefab).Prewarm(amount);

        public int CountAll<T>(T prefab) where T : Component => this.GetOrCreatePool(prefab).CountAll;
        
        public int CountActive<T>(T prefab) where T : Component => this.GetOrCreatePool(prefab).CountActive;
        
        public int CountInactive<T>(T prefab) where T : Component => this.GetOrCreatePool(prefab).CountInactive;

        public bool TryGet<T>(T prefab, out T obj) where T : Component
        {
            var pool = this.GetOrCreatePool(prefab);
            var result = pool.TryGet(out obj);
            if(!result) return false;
            this.relations[obj.GetInstanceID()] = pool;
            return true;
        }

        public T Get<T>(T prefab) where T : Component => this.TryGet(prefab, out var obj)
            ? obj
            : throw new InvalidOperationException("The pool for this prefab has reached its max capacity.");

        public void Release<T>(T obj) where T : Component
        {
            if(!this.relations.Remove(obj.GetInstanceID(), out var poolObj))
            {
                throw new InvalidOperationException(
                    "The specified object was not created by this pool, so it cannot be released.");
            }

            ((InternalPool<T>)poolObj).Release(obj);
        }

        public void ClearInactive<T>(T prefab) where T : Component => this.GetOrCreatePool(prefab).ClearInactive();
        
        public void ClearInactive()
        {
            foreach(var pool in this.pools.Values) ((IInactiveCleaner)pool).ClearInactive();
        }

        private InternalPool<T> GetOrCreatePool<T>(T prefab) where T : Component
        {
            var prefabId = prefab.GetInstanceID();
            InternalPool<T> pool;
            
            if(this.pools.TryGetValue(prefabId, out var objPool))
            {
                pool = (InternalPool<T>)objPool;
            }
            else
            {
                pool = new(prefab, new(int.MaxValue));
                this.pools.Add(prefabId, pool);
            }
            
            return pool;
        }
	}
}