# Prefab Pool
Provides efficient and lazy object pool, designed to use with the prefabs referenced by a component.

Features:
- Convenient API
- Simple and efficient
- Highly configurable
- Test coverage

# Why
Unlike Unity's built-in object pool (available since version 2021), this pool aims to minimize the need for
configuration and reduce the amount of code required to make it work. It is not a replacement for traditional
object pools, where a dedicated pool is allocated per object type, but a solution to a specific problem, where there
is a necessity to create a large number of instances of different prefabs.

# Installation
Add the package to your project via [UPM](https://docs.unity3d.com/Manual/upm-ui-giturl.html) using this link:
https://github.com/s-rayleigh/PrefabPool.git

# Usage
You can access the pool in either of these ways:
- Use the static methods of the `GlobalPrefabPool`. They will also create the pool object in the 
`GlobalPrefabPool.Instance` property on the first usage.
- Use the `GlobalPrefabPool.Instance` static property that acts like a Singleton.
- Create an object of the `PrefabPool` class.
- Use your favorite DI framework to inject the `PrefabPool` class.

To get an object from the pool, use the `Get` or `TryGet` methods. The pool itself is prefab-oriented, so you need 
to use prefab as an argument referenced by a component (e.g., `Transform` or any inherited from `MonoBehaviour`).

To return an object to the pool, use the `Release` method. 
You should only return the object to the pool from which it's taken.

If the pool has no available objects, it will create new ones, but you can create them in advance using the `Prewarm` 
method. It also keeps track of the number of objects it has created, which you can access using the `CountAll`, 
`CountInactive`, and `CountActive` methods.

You can also configure the pool **per prefab** using the `Configure` method and providing the `PoolParameters` struct, 
which allows you to specify the following parameters:
- The max capacity of the pool (`int.MaxValue` by default)
- Whether to activate an object when it's taken from the pool (`true` by default)
- Action to perform when the object is:
  - Created by the pool
  - Taken from the pool
  - Returned to the pool
  - Destroyed by the pool

You can adjust these parameters at any time.

When the pool reaches its max capacity, any returned object will be destroyed, the `Get` method will fail with an 
exception, and the `TryGet` method will return false.

To destroy all the inactive objects in the pool, use the `ClearInactive` method. You can also specify prefab as an 
argument to destroy only its instances.

If you want to receive the events in the component through which the prefab is referenced, you can implement these 
interfaces: `IPoolGetHandler`, `IPoolReleaseHandler`, and `IPoolDestroyHandler`.

Please note that the pool is not thread-safe and is intended to be used on the Unity main thread.

# Examples
```csharp
class MyComponent : MonoBehaviour, IPoolReleaseHandler
{
    [SerializeField]
    private Renderer rend;

    public void SetMaterial(Material material) => this.rend.sharedMaterial = material;

    public void OnPoolRelease() => this.rend.sharedMaterial = null;
}

class Spawner : MonoBehaviour
{
    [SerializeField]
    private MyComponent prefab;

    [SerializeField]
    private Material mat;
    
    private List<MyComponent> instances = new();

    private void Awake()
    {   
        for(var i = 0; i < 500; i++)
        {
            var instance = GlobalPrefabPool.Get(this.prefab);
            instance.SetMaterial(this.mat);
            this.instances.Add(instance);
        }
    }

    private void OnDestroy()
    {
        foreach(var instance in this.instances) GlobalPrefabPool.Release(instance);
    }
}
```

Configuration example:
```csharp
GlobalPrefabPool.Configure(this.prefab, new(
    maxCapacity: 500,
    onGet: obj => Debug.Log($"Object {obj.name} is taken from the pool.")
));
```

See the tests for more examples.
