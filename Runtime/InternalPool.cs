using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Rayleigh.PrefabPool
{
	internal sealed class InternalPool
	{
		private readonly Component prefab;
		
		private IPoolParameters parameters;
		
		private readonly Stack<Component> stack;

		/// <inheritdoc cref="PrefabPool.itemsParent"/>
		[MaybeNull]
		private readonly Transform itemsParent;

		/// <summary>
		/// An object used for grouping of returned items if that's enabled in the pool configuration.
		/// </summary>
		[MaybeNull]
		private Transform groupParent;
		
		public int CountAll { get; private set; }

		public int CountInactive => this.stack.Count;

		public int CountActive => this.CountAll - this.CountInactive;
		
		public InternalPool(Component prefab, IPoolParameters parameters, [MaybeNull] Transform itemsParent)
		{
			this.prefab = prefab;
			this.parameters = parameters;
			this.stack = new();
			this.itemsParent = itemsParent;
		}

		/// <summary>
		/// Sets parameters for this pool.
		/// </summary>
		/// <param name="parameters">Parameters to set.</param>
		/// <exception cref="ArgumentException">Specified max capacity is lower or equal to zero.</exception>
		public void SetParameters(IPoolParameters parameters)
		{
			if(parameters.MaxCapacity <= 0) throw new ArgumentException("The max capacity must be greater than zero.");
			
			var prevGroupReturned = this.parameters.GroupReturnedItems;
			this.parameters = parameters;
			
			if(parameters.GroupReturnedItems && !prevGroupReturned)
			{
				var name = string.IsNullOrEmpty(this.prefab.name)
					? Guid.NewGuid().ToString()
					: this.prefab.name + "_Group";
				var groupObject = new GameObject(name);
				this.groupParent = groupObject.transform;
				if (this.itemsParent is not null)
					Object.DontDestroyOnLoad(groupObject);
				this.groupParent.SetParent(this.itemsParent);
				groupObject.SetActive(false);
				
				// Re-parent all returned items.
				foreach (var returned in this.stack)
					returned.transform.SetParent(this.groupParent, false);
			}
			
			if(!parameters.GroupReturnedItems && this.groupParent is not null)
			{
				// Unparent all returned items if grouping was enabled and now disabled.
				while(this.groupParent.childCount > 0)
					this.groupParent.GetChild(0).SetParent(itemsParent, false);
				Object.Destroy(this.groupParent.gameObject);
			}
		}

		public void Prewarm(int number)
		{
			number = Mathf.Min(number, this.parameters.MaxCapacity - this.CountAll);
			this.CountAll += number;

			for(var i = 0; i < number; i++)
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

			var go = obj.gameObject;
			var parent = this.groupParent ?? this.itemsParent;
			if (parent is not null)
				Object.DontDestroyOnLoad(go);
			obj.transform.SetParent(parent, false);
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