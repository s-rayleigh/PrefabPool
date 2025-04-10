using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Rayleigh.PrefabPool
{
	internal sealed class InternalPool
	{
		private readonly Component _prefab;
		
		private IPoolParameters _parameters;
		
		private readonly Stack<Component> _stack;

		/// <inheritdoc cref="PrefabPool._itemsParent"/>
		[MaybeNull]
		private readonly Transform _itemsParent;

		/// <summary>
		/// An object used for grouping of returned items if that's enabled in the pool configuration.
		/// </summary>
		[MaybeNull]
		private Transform _groupParent;
		
		public int CountAll { get; private set; }

		public int CountInactive => _stack.Count;

		public int CountActive => CountAll - CountInactive;
		
		public InternalPool(Component prefab, IPoolParameters parameters, [MaybeNull] Transform itemsParent)
		{
			_prefab = prefab;
			_parameters = parameters;
			_stack = new();
			_itemsParent = itemsParent;
		}

		/// <summary>
		/// Sets parameters for this pool.
		/// </summary>
		/// <param name="parameters">Parameters to set.</param>
		/// <exception cref="ArgumentException">Specified max capacity is lower or equal to zero.</exception>
		public void SetParameters(IPoolParameters parameters)
		{
			if (parameters.MaxCapacity <= 0)
				throw new ArgumentException("The max capacity must be greater than zero.");

			var prevGroupReturned = _parameters.GroupReturnedItems;
			_parameters = parameters;

			if (parameters.GroupReturnedItems && !prevGroupReturned)
			{
				var name = string.IsNullOrEmpty(_prefab.name) ? Guid.NewGuid().ToString() : _prefab.name + "_Group";
				var groupObject = new GameObject(name);
				_groupParent = groupObject.transform;
				if (_itemsParent is not null)
					Object.DontDestroyOnLoad(groupObject);
				_groupParent.SetParent(_itemsParent);
				groupObject.SetActive(false);

				// Re-parent all returned items.
				foreach (var returned in _stack)
					returned.transform.SetParent(_groupParent, false);
			}

			if (!parameters.GroupReturnedItems && _groupParent is not null)
			{
				// Unparent all returned items if grouping was enabled and now disabled.
				while (_groupParent.childCount > 0)
					_groupParent.GetChild(0).SetParent(_itemsParent, false);
				Object.Destroy(_groupParent.gameObject);
			}
		}

		public void Prewarm(int number)
		{
			number = Mathf.Min(number, _parameters.MaxCapacity - CountAll);
			CountAll += number;

			for (var i = 0; i < number; i++)
			{
				var instance = CreateNewInstance();
				instance.gameObject.SetActive(false);
				_stack.Push(instance);
			}
		}

		public bool TryGet(out Component item)
		{
			if (_stack.Count > 0)
			{
				item = _stack.Pop();
			}
			else
			{
				item = null;
				if (CountAll >= _parameters.MaxCapacity)
					return false;
				CountAll += 1;
				item = CreateNewInstance();
			}

			if (_parameters.ActivateOnGet)
				item.gameObject.SetActive(true);
			_parameters.InvokeOnGet(item);
			if (item is IPoolGetHandler gh)
				gh.OnPoolGet();

			return true;
		}

		public void Release(Component obj)
		{
			if (!obj)
			{
				// Decrement instances count and return if the releasing object is null or destroyed.
				CountAll -= 1;
				return;
			}

			_parameters.InvokeOnRelease(obj);
			if (obj is IPoolReleaseHandler h)
				h.OnPoolRelease();

			if (CountAll >= _parameters.MaxCapacity)
			{
				CountAll -= 1;
				DestroyObject(obj);
			}

			var go = obj.gameObject;
			var parent = _groupParent ?? _itemsParent;
			if (parent is not null)
				Object.DontDestroyOnLoad(go);
			obj.transform.SetParent(parent, false);
			go.SetActive(false);

			_stack.Push(obj);
		}

		public void ClearInactive()
		{
			while (_stack.TryPop(out var item))
			{
				DestroyObject(item);
				CountAll -= 1;
			}
		}

		private Component CreateNewInstance()
		{
			var instance = Object.Instantiate(_prefab);
			Object.DontDestroyOnLoad(instance.gameObject);
			_parameters.InvokeOnCreate(instance);
			return instance;
		}
		
		private void DestroyObject(Component obj)
		{
			if(obj is IPoolDestroyHandler dh)
				dh.OnPoolDestroy();
			_parameters.InvokeOnDestroy(obj);
			Object.Destroy(obj.gameObject);
		}
	}
}