using UnityEngine;
using DG.Tweening;


/// <summary>
/// Attached automatically to pooled instances. Stores the prefab they came from,
/// and optional hooks for cleanup.
/// </summary>
public class PooledObject : MonoBehaviour
{
	[HideInInspector] public Object originPrefab;

	// Optional hook points if you need cleanup between uses
	public virtual void OnSpawned() { }
	public virtual void OnDespawned() { }
}