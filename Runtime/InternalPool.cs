using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Rayleigh.PrefabPool
{
	internal sealed class InternalPool
	{
		private readonly Component prefab;
		
		private IPoolParameters parameters;
		
		private readonly Stack<Component> stack;

		public int CountAll { get; private set; }

		public int CountInactive => this.stack.Count;

		public int CountActive => this.CountAll - this.CountInactive;
		
		public InternalPool(Component prefab, IPoolParameters parameters)
		{
			this.prefab = prefab;
			this.parameters = parameters;
			this.stack = new();
		}

		/// <summary>
		/// Sets parameters for this pool.
		/// </summary>
		/// <param name="parameters">Parameters to set.</param>
		/// <exception cref="ArgumentException">Specified max capacity is lower or equal to zero.</exception>
		public void SetParameters(IPoolParameters parameters)
		{
			if(parameters.MaxCapacity <= 0) throw new ArgumentException("The max capacity must be greater than zero.");
			this.parameters = parameters;
		}

		public void Prewarm(int amount)
		{
			amount = Mathf.Min(amount, this.parameters.MaxCapacity - this.CountAll);
			this.CountAll += amount;

			for(var i = 0; i < amount; i++)
			{
				var instance = this.CreateNewInstance();
				instance.gameObject.SetActive(false);
				this.stack.Push(instance);
			}
		}

		public bool TryGet(out Component item)
		{
			if(this.stack.Count > 0)
			{
				item = this.stack.Pop();
			}
			else
			{
				item = null;
				
				if(this.CountAll >= this.parameters.MaxCapacity) return false;
				this.CountAll += 1;
				
				item = this.CreateNewInstance();
			}
			
			if(this.parameters.ActivateOnGet) item.gameObject.SetActive(true);
			
			this.parameters.InvokeOnGet(item);
			if(item is IPoolGetHandler gh) gh.OnPoolGet();
			
			return true;
		}

		public void Release(Component obj)
		{
			if(!obj)
			{
				// Decrement instances count and return if the releasing object is null or destroyed.
				this.CountAll -= 1;
				return;
			}
			
			this.parameters.InvokeOnRelease(obj);
			if(obj is IPoolReleaseHandler h) h.OnPoolRelease();

			if(this.CountAll >= this.parameters.MaxCapacity)
			{
				this.CountAll -= 1;
				this.DestroyObject(obj);
			}

			var tr = obj.transform;
			tr.SetParent(null, false);

			var go = obj.gameObject;
			Object.DontDestroyOnLoad(go);
			go.SetActive(false);

			this.stack.Push(obj);
		}
		
		public void ClearInactive()
		{
			while(this.stack.TryPop(out var item))
			{
				this.DestroyObject(item);
				this.CountAll -= 1;
			}
		}

		private Component CreateNewInstance()
		{
			var instance = Object.Instantiate(this.prefab);
			Object.DontDestroyOnLoad(instance.gameObject);
			this.parameters.InvokeOnCreate(instance);
			return instance;
		}
		
		private void DestroyObject(Component obj)
		{
			if(obj is IPoolDestroyHandler dh) dh.OnPoolDestroy();
			this.parameters.InvokeOnDestroy(obj);
			Object.Destroy(obj.gameObject);
		}
	}
}