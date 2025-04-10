using UnityEngine;

namespace Rayleigh.PrefabPool
{
	internal interface IPoolParameters
	{
		/// <summary>
		/// The max capacity of the pool. When the pool reaches its max capacity,
		/// any instances returned to the pool will be destroyed and get methods will fail.
		/// </summary>
		int MaxCapacity { get; }
		
		/// <summary>
		/// If set to true, instances taken from the pool will be active even if the prefab is inactive; otherwise,
		/// they will be left as is.
		/// </summary>
		bool ActivateOnGet { get; }
		
		/// <summary>
		/// If set to true, returned items are parented into an object with a name that matches the prefab name.
		/// </summary>
		bool GroupReturnedItems { get; }
		
		void InvokeOnCreate(Component obj);

		void InvokeOnGet(Component obj);

		void InvokeOnRelease(Component obj);

		void InvokeOnDestroy(Component obj);
	}
}