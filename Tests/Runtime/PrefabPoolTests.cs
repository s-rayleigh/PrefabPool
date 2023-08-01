using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Rayleigh.PrefabPool.Tests
{
	public class PrefabPoolTests
	{
		private Transform prefab;

		[OneTimeSetUp]
		public void Setup()
		{
			this.prefab = new GameObject().transform;
		}
		
		[Test]
		public void GetReleaseOneInstance()
		{
			var pool = new PrefabPool();
			var instance = pool.Get(this.prefab);
			AssertCount(pool, this.prefab, 1, 0, 1);
			pool.Release(instance);
			AssertCount(pool, this.prefab, 1, 1, 0);
		}

		[Test]
		public void GetReleaseMultipleInstances()
		{
			var pool = new PrefabPool();
			var takenInstances = new Queue<Transform>();

			const int toTake = 5000;
			const int toRelease = 3739;
			
			for(var j = 0; j < toTake; j++) takenInstances.Enqueue(pool.Get(this.prefab));
			for(var j = 0; j < toRelease; j++) pool.Release(takenInstances.Dequeue());
			
			AssertCount(pool, this.prefab, toTake, toRelease, toTake - toRelease);
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

			for(var i = 0; i < batches; i++)
			{
				for(var j = 0; j < toTakePerBatch; j++) takenInstances.Enqueue(pool.Get(this.prefab));
				for(var j = 0; j < toReleasePerBatch; j++) pool.Release(takenInstances.Dequeue());
			}

			Assert.That(pool.CountActive(this.prefab), Is.EqualTo(toTake - toRelease));
		}

		[Test]
		public void PrewarmAndGet()
		{
			var pool = new PrefabPool();
			pool.Prewarm(this.prefab, 100);
			AssertCount(pool, this.prefab, 100, 100, 0);
			for(var i = 0; i < 50; i++) pool.Get(this.prefab);
			AssertCount(pool, this.prefab, 100, 50, 50);
		}

		[Test]
		public void MaxCapacityLimit()
		{
			var pool = new PrefabPool();
			pool.Configure(this.prefab, new(10));

			for(var i = 0; i < 10; i++) pool.Get(this.prefab);

			Assert.Throws<InvalidOperationException>(() => pool.Get(this.prefab));
			Assert.That(pool.TryGet(this.prefab, out _), Is.False);
		}

		[Test]
		public void GetReleaseMultipleInstancesTwoPrefabs()
		{
			var secondPrefab = new GameObject().AddComponent<TestComponent>();
			var pool = new PrefabPool();
			var firstTaken = new Queue<Transform>();
			var secondTaken = new Queue<TestComponent>();
			
			for(var i = 0; i < 70; i++)
			{
				firstTaken.Enqueue(pool.Get(this.prefab));
				secondTaken.Enqueue(pool.Get(secondPrefab));
			}
			
			AssertCount(pool, this.prefab, 70, 0, 70);
			AssertCount(pool, secondPrefab, 70, 0, 70);
			
			for(var i = 0; i < 30; i++) pool.Release(firstTaken.Dequeue());
			for(var i = 0; i < 55; i++) pool.Release(secondTaken.Dequeue());
			
			AssertCount(pool, this.prefab, 70, 30, 40);
			AssertCount(pool, secondPrefab, 70, 55, 15);
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

		private static void AssertCount<T>(PrefabPool pool, T prefab, int all, int inactive, int active) where T : Component
		{
			Assert.That(pool.CountAll(prefab), Is.EqualTo(all));
			Assert.That(pool.CountInactive(prefab), Is.EqualTo(inactive));
			Assert.That(pool.CountActive(prefab), Is.EqualTo(active));
		}
	}
}