namespace Rayleigh.PrefabPool
{
	/// <summary>
	/// Interface to implement for the component used as prefab reference
	/// if you wish to receive <see cref="OnPoolRelease"/> callbacks.
	/// </summary>
	public interface IPoolReleaseHandler
	{
		void OnPoolRelease();
	}
}