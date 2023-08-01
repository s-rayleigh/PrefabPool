using UnityEngine;

namespace Rayleigh.PrefabPool
{
	internal interface IPoolParameters
	{
		int MaxCapacity { get; }
		
		bool ActivateOnGet { get; }
		
		void InvokeOnCreate(Component obj);

		void InvokeOnGet(Component obj);

		void InvokeOnRelease(Component obj);

		void InvokeOnDestroy(Component obj);
	}
}