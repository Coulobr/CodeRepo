using UnityEngine;
using DG.Tweening;

[CreateAssetMenu(menuName = "AA/Spells/Fire Tornado")]
public class FireTornadoSO : SpellSO
{
	public float radius = 3.5f;
	public float dps = 10f;
	public override SpellRuntime AttachTo(GameObject owner)
	{
		var rt = owner.AddComponent<FireTornadoRuntime>();
		rt.BindDefinition(this);
		rt.Setup(radius, dps);
		return rt;
	}
}

public class FireTornadoRuntime : SpellRuntime
{
	float radius, dps;
	LayerMask enemyMask;
	bool firstCast = true;

	public void Setup(float r, float damage) { radius = r; dps = damage; enemyMask = LayerMask.GetMask("Enemy"); }

	public override void Tick(float dt)
	{
		base.Tick(dt);
		var hits = Physics2D.OverlapCircleAll(owner.position, radius, enemyMask);
		float dmg = dps * dt;
		foreach (var h in hits) h.GetComponent<EnemySimple>()?.Damage(dmg);
	}

	public override void Cast()
	{
		// fire tornado is “always on”; treat first cast as activation
		if (firstCast)
		{
			firstCast = false;
			ScreenShake.Shake(0.8f, 0.12f);
			owner.DOPunchScale(Vector3.one * 0.06f, 0.12f, 6, 0.7f);
		}
		ResetCD();
	}
}