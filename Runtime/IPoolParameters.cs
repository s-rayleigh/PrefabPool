using UnityEngine;

namespace Rayleigh.PrefabPool
{
	internal interface IPoolParameters
	{
		int MaxCapacity { get; }
		
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