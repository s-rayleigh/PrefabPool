namespace Rayleigh.PrefabPool
{
	/// <summary>
	/// Interface to implement for the component used as prefab reference
	/// if you wish to receive <see cref="OnPoolDestroy"/> callbacks.
	/// </summary>
	public interface IPoolDestroyHandler
	{
		void OnPoolDestroy();
	}
}