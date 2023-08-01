using UnityEngine;

namespace Rayleigh.PrefabPool
{
	public static class GlobalPrefabPool
	{
		private static PrefabPool instance;

		public static PrefabPool Instance => instance ??= new();

		public static void Configure<T>(T prefab, PoolParameters<T> parameters) where T : Component =>
			Instance.Configure(prefab, parameters);

		public static void Prewarm<T>(T prefab, int amount) where T : Component => Instance.Prewarm(prefab, amount);

		public static int CountAll<T>(T prefab) where T : Component => Instance.CountAll(prefab);

		public static int CountActive<T>(T prefab) where T : Component => Instance.CountActive(prefab);

		public static int CountInactive<T>(T prefab) where T : Component => Instance.CountInactive(prefab);

		public static bool TryGet<T>(T prefab, out T obj) where T : Component => Instance.TryGet(prefab, out obj);

		public static T Get<T>(T prefab) where T : Component => Instance.Get(prefab);

		public static void Release<T>(T obj) where T : Component => Instance.Release(obj);

		public static void ClearInactive<T>(T prefab) where T : Component => Instance.ClearInactive(prefab);

		public static void ClearInactive() => Instance.ClearInactive();
	}
}