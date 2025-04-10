using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Rayleigh.PrefabPool.Tests
{
	public class PrefabPoolTests
	{
		private const string ParentName = "Parent";
		private const string FirstPrefabName = "FirstPrefab";
		private const string SecondPrefabName = "SecondPrefab";
		
		private Transform _firstPrefab;
		private TestComponent _secondPrefab;
		private int _parentCount;

		[OneTimeSetUp]
		public void Setup()
		{
			_firstPrefab = new GameObject(FirstPrefabName).transform;
			_secondPrefab = new GameObject(SecondPrefabName).AddComponent<TestComponent>();
		}

		[Test]
		public void GlobalPool()
		{
			var instance = GlobalPrefabPool.Get(_firstPrefab);
			AssertCount(GlobalPrefabPool.Instance, _firstPrefab, 1, 0, 1);
			GlobalPrefabPool.Release(instance);
			AssertCount(GlobalPrefabPool.Instance, _firstPrefab, 1, 1, 0);
		}

		[Test]
		public void GetReleaseOneInstance()
		{
			var pool = new PrefabPool();
			var instance = pool.Get(_firstPrefab);
			AssertCount(pool, _firstPrefab, 1, 0, 1);
			pool.Release(instance);
			AssertCount(pool, _firstPrefab, 1, 1, 0);
		}

		[Test]
		public void GetReleaseMultipleInstances()
		{
			var pool = new PrefabPool();
			var takenInstances = new Queue<Transform>();

			const int toTake = 5000;
			const int toRelease = 3739;

			for (var j = 0; j < toTake; j++)
				takenInstances.Enqueue(pool.Get(this._firstPrefab));
			for (var j = 0; j < toRelease; j++)
				pool.Release(takenInstances.Dequeue());

			AssertCount(pool, _firstPrefab, toTake, toRelease, toTake - toRelease);
		}

		[Test]
		public void GetReleaseMultipleInstancesBatches()
		{
			var pool = new PrefabPool();
			var takenInstances = new Queue<Transform>();

			const int toTake = 500;
			const int toRelease = 270;
			const int batches = 5;
			const int toTakePerBatch = toTake / batches;
			const int toReleasePerBatch = toRelease / batches;

			for (var i = 0; i < batches; i++)
			{
				for (var j = 0; j < toTakePerBatch; j++)
					takenInstances.Enqueue(pool.Get(_firstPrefab));
				for (var j = 0; j < toReleasePerBatch; j++)
					pool.Release(takenInstances.Dequeue());
			}

			Assert.That(pool.CountActive(_firstPrefab), Is.EqualTo(toTake - toRelease));
		}

		[Test]
		public void PrewarmAndGet()
		{
			var pool = new PrefabPool();
			pool.Prewarm(this._firstPrefab, 100);
			AssertCount(pool, this._firstPrefab, 100, 100, 0);
			for (var i = 0; i < 50; i++)
				pool.Get(this._firstPrefab);
			AssertCount(pool, this._firstPrefab, 100, 50, 50);
		}

		[Test]
		public void MaxCapacityLimit()
		{
			var pool = new PrefabPool();
			pool.Configure(_firstPrefab, new(10));

			for (var i = 0; i < 10; i++)
				pool.Get(_firstPrefab);

			Assert.Throws<InvalidOperationException>(() => pool.Get(_firstPrefab));
			Assert.That(pool.TryGet(_firstPrefab, out _), Is.False);
		}

		[Test]
		public void GetReleaseMultipleInstancesTwoPrefabs()
		{
			var pool = new PrefabPool();
			var firstTaken = new Queue<Transform>();
			var secondTaken = new Queue<TestComponent>();

			for (var i = 0; i < 70; i++)
			{
				firstTaken.Enqueue(pool.Get(_firstPrefab));
				secondTaken.Enqueue(pool.Get(_secondPrefab));
			}

			AssertCount(pool, _firstPrefab, 70, 0, 70);
			AssertCount(pool, _secondPrefab, 70, 0, 70);

			for (var i = 0; i < 30; i++)
				pool.Release(firstTaken.Dequeue());
			for (var i = 0; i < 55; i++)
				pool.Release(secondTaken.Dequeue());

			AssertCount(pool, this._firstPrefab, 70, 30, 40);
			AssertCount(pool, this._secondPrefab, 70, 55, 15);
		}

		[Test]
		public void InstanceStaysInactive()
		{
			var pool = new PrefabPool();
			var inactivePrefab = new GameObject().transform;
			inactivePrefab.gameObject.SetActive(false);

			pool.Configure(inactivePrefab, new(activateOnGet: false));
			var inactiveInstance = pool.Get(inactivePrefab);
			Assert.That(inactiveInstance.gameObject.activeSelf, Is.False);

			pool.Configure(inactivePrefab, new(activateOnGet: true));
			var activeInstance = pool.Get(inactivePrefab);
			Assert.That(activeInstance.gameObject.activeSelf, Is.True);
		}

		[UnityTest]
		public IEnumerator DestructionOnRelease()
		{
			var pool = new PrefabPool();
			pool.Prewarm(_firstPrefab, 10);
			var instance = pool.Get(_firstPrefab);
			pool.Configure(_firstPrefab, new(5));
			pool.Release(instance);

			// Required because Unity destroys the object in the end of frame, so we do the check on the next frame.
			yield return null;

			Assert.That((bool)instance, Is.False);
		}

		[Test]
		public void ClearInactive()
		{
			var pool = new PrefabPool();
			pool.Prewarm(_firstPrefab, 100);
			pool.Get(_firstPrefab);
			pool.ClearInactive(_firstPrefab);
			AssertCount(pool, _firstPrefab, 1, 0, 1);
		}

		[Test]
		public void ClearInactiveTwoPrefabs()
		{
			var pool = new PrefabPool();
			pool.Prewarm(_firstPrefab, 100);
			pool.Prewarm(_secondPrefab, 100);
			pool.Get(_firstPrefab);
			pool.Get(_secondPrefab);
			pool.ClearInactive();
			AssertCount(pool, _firstPrefab, 1, 0, 1);
			AssertCount(pool, _secondPrefab, 1, 0, 1);
		}

		[Test]
		public void ParametersCallbacks()
		{
			var pool = new PrefabPool();
			Transform created = null, gotten = null, released = null, destroyed = null;

			pool.Configure(_firstPrefab, new(onCreate: t => created = t, onGet: t => gotten = t,
				onRelease: t => released = t, onDestroy: t => destroyed = t));

			var instance = pool.Get(_firstPrefab);
			pool.Release(instance);
			pool.ClearInactive(_firstPrefab);

			Assert.That(created, Is.EqualTo(instance));
			Assert.That(gotten, Is.EqualTo(instance));
			Assert.That(released, Is.EqualTo(instance));
			Assert.That(destroyed, Is.EqualTo(instance));
		}

		[Test]
		public void InterfaceEventListeners()
		{
			var pool = new PrefabPool();
			var instance = pool.Get(_secondPrefab);
			pool.Release(instance);
			pool.ClearInactive(_secondPrefab);

			Assert.That(instance.GetCalled, Is.True);
			Assert.That(instance.ReleaseCalled, Is.True);
			Assert.That(instance.DestroyCalled, Is.True);
		}

		[Test]
		public void ParentingReturnedItems()
		{
			var parentName = GetUniqueParentName();
			var pool = new PrefabPool(parentName);
			var parentObject = FindObjectByName(parentName);
			Assert.That(parentObject, Is.Not.Null);
			var instance = pool.Get(_firstPrefab);
			Assert.That(instance.parent, Is.Null);
			pool.Release(instance);
			Assert.That(instance.parent, Is.EqualTo(parentObject.transform));
		}

		[Test]
		public void GroupingReturnedItems()
		{
			var parentName = GetUniqueParentName();
			var pool = new PrefabPool(parentName);
			var parentObject = FindObjectByName(parentName);

			Assert.That(parentObject, Is.Not.Null);

			pool.Configure(_firstPrefab, new(groupReturned: true));
			pool.Configure(_secondPrefab, new(groupReturned: true));

			var firstInstance = pool.Get(_firstPrefab);
			var secondInstance = pool.Get(_secondPrefab);

			Assert.That(firstInstance.parent, Is.Null);
			Assert.That(secondInstance.transform.parent, Is.Null);

			pool.Release(firstInstance);
			pool.Release(secondInstance);

			var firstParent = FindObjectByName(FirstPrefabName + "_Group")?.transform;
			var secondParent = FindObjectByName(SecondPrefabName + "_Group")?.transform;

			Assert.That(firstParent, Is.Not.Null);
			Assert.That(secondParent, Is.Not.Null);
			Assert.That(firstParent.parent, Is.EqualTo(parentObject.transform));
			Assert.That(secondParent.parent, Is.EqualTo(parentObject.transform));
			Assert.That(firstInstance.parent, Is.EqualTo(firstParent));
			Assert.That(secondInstance.transform.parent, Is.EqualTo(secondParent));
		}

		private string GetUniqueParentName()
		{
			var name = ParentName + _parentCount;
			_parentCount++;
			return name;
		}

		private Transform FindObjectByName(string name)
		{
			var transforms = Resources.FindObjectsOfTypeAll<Transform>();

			for (var i = 0; i < transforms.Length; i++)
			{
				var transform = transforms[i];
				if (transform.hideFlags is not HideFlags.None)
					continue;
				if (transforms[i].name == name)
					return transforms[i];
			}

			return null;
		}

		private static void AssertCount(PrefabPool pool, Component prefab, int all, int inactive, int active)
		{
			Assert.That(pool.CountAll(prefab), Is.EqualTo(all));
			Assert.That(pool.CountInactive(prefab), Is.EqualTo(inactive));
			Assert.That(pool.CountActive(prefab), Is.EqualTo(active));
		}
	}
}