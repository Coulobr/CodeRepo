using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

/// <summary>
/// Generic pool keyed by prefab (UnityEngine.Object).
/// Works with any Component prefab.
/// </summary>
public static class Pool
{
	private class PoolStack
	{
		public readonly Stack<Component> stack = new Stack<Component>(64);
		public readonly Transform root;
		public PoolStack(string name)
		{
			var go = new GameObject($"[POOL] {name}");
			Object.DontDestroyOnLoad(go);
			root = go.transform;
		}
	}

	// Key: prefab (UnityEngine.Object). Value: stack container.
	private static readonly Dictionary<Object, PoolStack> _pools = new();

	/// <summary>
	/// Preload a number of instances of a prefab.
	/// </summary>
	public static void Preload<T>(T prefab, int count) where T : Component
	{
		EnsurePool(prefab);
		for (int i = 0; i < count; i++)
		{
			var inst = Object.Instantiate(prefab, Vector3.one * 99999f, Quaternion.identity, _pools[prefab].root);
			EnsurePooledObject(inst, prefab);
			inst.gameObject.SetActive(false);
			_pools[prefab].stack.Push(inst);
		}
	}

	/// <summary>
	/// Get an instance of the prefab at position/rotation.
	/// </summary>
	public static T Get<T>(T prefab, Vector3 pos, Quaternion rot) where T : Component
	{
		EnsurePool(prefab);

		T inst;
		if (_pools[prefab].stack.Count > 0)
		{
			inst = (T)_pools[prefab].stack.Pop();
		}
		else
		{
			inst = Object.Instantiate(prefab);
			EnsurePooledObject(inst, prefab);
		}

		var go = inst.gameObject;
		go.transform.SetPositionAndRotation(pos, rot);
		go.transform.SetParent(null, false);
		go.SetActive(true);

		// Reset tweens if any
		go.transform.DOKill(true);
		var po = go.GetComponent<PooledObject>();
		po?.OnSpawned();

		return inst;
	}

	/// <summary>
	/// Return an instance to its prefab pool.
	/// </summary>
	public static void Release(Component instance)
	{
		if (!instance) return;
		var go = instance.gameObject;

		var po = go.GetComponent<PooledObject>();
		if (!po || po.originPrefab == null)
		{
			// If the instance wasn't created by Pool, just destroy it safely.
			Object.Destroy(go);
			return;
		}

		// Clean up
		po.OnDespawned();
		go.transform.DOKill(true);
		go.SetActive(false);
		go.transform.SetParent(_pools[po.originPrefab].root, false);

		_pools[po.originPrefab].stack.Push(instance);
	}

	private static void EnsurePool(Object prefab)
	{
		if (!_pools.ContainsKey(prefab))
			_pools[prefab] = new PoolStack(prefab.name);
	}

	private static void EnsurePooledObject(Component inst, Object prefabKey)
	{
		var po = inst.GetComponent<PooledObject>();
		if (!po) po = inst.gameObject.AddComponent<PooledObject>();
		po.originPrefab = prefabKey;
	}
}
