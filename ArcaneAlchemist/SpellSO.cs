using UnityEngine;
using DG.Tweening;

public abstract class SpellSO : ScriptableObject
{
	[Header("Common")]
	public string spellName;
	public Sprite icon;
	public float baseCooldown = 2f;

	public abstract SpellRuntime AttachTo(GameObject owner);
}