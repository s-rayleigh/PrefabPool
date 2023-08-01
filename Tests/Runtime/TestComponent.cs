using UnityEngine;

namespace Rayleigh.PrefabPool.Tests
{
	public sealed class TestComponent : MonoBehaviour, IPoolGetHandler, IPoolReleaseHandler, IPoolDestroyHandler
	{
		public bool GetCalled { get; private set; }
		
		public bool ReleaseCalled { get; private set; }
		
		public bool DestroyCalled { get; private set; }
		
		public void OnPoolGet() => this.GetCalled = true;

		public void OnPoolRelease() => this.ReleaseCalled = true;

		public void OnPoolDestroy() => this.DestroyCalled = true;
	}
}