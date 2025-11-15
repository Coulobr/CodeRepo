// SpellRuntime.cs
using UnityEngine;

public abstract class SpellRuntime : MonoBehaviour
{
	public SpellSO Definition { get; private set; }   // <-- link to the SpellSO
	public float cooldown = 1f;
	float cd;

	public void BindDefinition(SpellSO def) => Definition = def;

	public virtual void Init(Transform owner, float cooldown) { this.cooldown = cooldown; this.owner = owner; }
	protected Transform owner;
	public virtual void Tick(float dt) { cd -= dt; }
	public bool Ready => cd <= 0f;
	public void ResetCD() { cd = cooldown; }
	public abstract void Cast();
}