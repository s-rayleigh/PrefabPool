using UnityEngine;

namespace Rayleigh.PrefabPool
{
	public struct DefaultPoolParameters : IPoolParameters
	{
		public int MaxCapacity => int.MaxValue;

		public bool ActivateOnGet => true;
		
		public void InvokeOnCreate(Component obj)
		{
			
		}

		public void InvokeOnGet(Component obj)
		{
			
		}

		public void InvokeOnRelease(Component obj)
		{
			
		}

		public void InvokeOnDestroy(Component obj)
		{
			
		}
	}
}