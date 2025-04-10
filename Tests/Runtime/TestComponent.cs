using UnityEngine;

namespace Rayleigh.PrefabPool.Tests
{
	public sealed class TestComponent : MonoBehaviour, IPoolGetHandler, IPoolReleaseHandler, IPoolDestroyHandler
	{
		public bool GetCalled { get; private set; }
		
		public bool ReleaseCalled { get; private set; }
		
		public bool DestroyCalled { get; private set; }
		
		public void OnPoolGet() => GetCalled = true;

		public void OnPoolRelease() => ReleaseCalled = true;

		public void OnPoolDestroy() => DestroyCalled = true;
	}
}