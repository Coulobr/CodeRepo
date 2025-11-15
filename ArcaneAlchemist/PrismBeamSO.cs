using UnityEngine;
using DG.Tweening;

[CreateAssetMenu(menuName = "AA/Spells/Prism Beam")]
public class PrismBeamSO : SpellSO
{
	public float range = 10f;
	public float width = 0.5f;
	public float damage = 30f;

	public override SpellRuntime AttachTo(GameObject owner)
	{
		var rt = owner.AddComponent<PrismBeamRuntime>();
		rt.BindDefinition(this);
		rt.Setup(range, width, damage);
		return rt;
	}
}

public class PrismBeamRuntime : SpellRuntime
{
	float range, width, dmg;
	LayerMask enemyMask;

	public void Setup(float r, float w, float d) { range = r; width = w; dmg = d; enemyMask = LayerMask.GetMask("Enemy"); }

	public override void Cast()
	{
		var target = FindClosestEnemy();
		if (!target) { ResetCD(); return; }

		Vector2 dir = ((Vector2)target.position - (Vector2)owner.position).normalized;

		var hits = Physics2D.BoxCastAll(owner.position, new Vector2(range, width), 0f, dir, 0f, enemyMask);
		foreach (var h in hits)
			h.collider.GetComponent<EnemySimple>()?.Damage(dmg);

		// big cast: impulse
		ScreenShake.Shake(1.2f, 0.15f);

		// quick recoil nudge
		owner.DOMove((Vector2)owner.position - dir * 0.12f, 0.07f).SetLoops(2, LoopType.Yoyo);

		ResetCD();
	}

	Transform FindClosestEnemy()
	{
		var enemies = GameObject.FindGameObjectsWithTag("Enemy");
		Transform best = null;
		float bd = Mathf.Infinity;
		foreach (var e in enemies)
		{
			float d = Vector2.Distance(owner.position, e.transform.position);
			if (d < bd) { bd = d; best = e.transform; }
		}
		return best;
	}
}