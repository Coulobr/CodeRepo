using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(Collider2D))]
public class Projectile : PooledObject
{
	[Header("Setup")]
	public Rigidbody2D rb;
	public SpriteRenderer sr;        // optional; for tween hooks
									 //public TrailRenderer trail;    // optional; auto-cleared on recycle
	public LayerMask hitMask;        // assign to Enemy layer

	[Header("Facing")]
	[Tooltip("Leave null to rotate the whole object, or assign a child (e.g., 'Visual') to rotate only the sprite.")]
	public Transform rotateTarget;
	[Tooltip("0 if sprite points Right (+X). Use -90 if sprite art points Up (+Y).")]
	public float facingOffsetDeg = 0f;
	[Tooltip("If true, keeps rotating to match current velocity each frame.")]
	public bool lockToVelocity = true;

	[Header("Damage")]
	public bool destroyOnHit = true;
	public float hitShake = 0.6f;    // screen shake amplitude on hit

	float life;
	float lifeTimer;
	float dmg;
	Transform owner;
	ProjectileSpellSO data;
	bool active;

	void Awake()
	{
		if (!rb) rb = GetComponent<Rigidbody2D>();
		if (!sr) sr = GetComponentInChildren<SpriteRenderer>();
		if (!rotateTarget) rotateTarget = transform; // default: rotate whole projectile

		var col = GetComponent<Collider2D>();
		col.isTrigger = true;
	}

	public void Launch(Vector2 velocity, float lifetime, float damage, Transform owner, ProjectileSpellSO data)
	{
		this.owner = owner;
		this.data = data;
		life = lifetime;
		lifeTimer = life;
		dmg = damage;
		active = true;

		// Velocity & orientation
		if (rb)
		{
			rb.angularVelocity = 0;
			rb.linearVelocity = velocity;
		}
		ApplyFacingFromVelocity(velocity);

		// VFX
		//if (trail) trail.Clear();
		if (sr && data != null && data.spawnScaleIn > 0f)
		{
			sr.transform.localScale = Vector3.one;
			sr.transform.DOPunchScale(Vector3.one * (data.spawnScaleAmount - 1f), data.spawnScaleIn, 8, 0.9f);
		}
	}

	void Update()
	{
		if (!active) return;

		// Optional continuous alignment if velocity changes mid-flight
		if (lockToVelocity && rb)
		{
			var v = rb.linearVelocity;
			if (v.sqrMagnitude > 0.0001f)
				ApplyFacingFromVelocity(v);
		}

		lifeTimer -= Time.deltaTime;
		if (lifeTimer <= 0f)
			Recycle();
	}

	void ApplyFacingFromVelocity(Vector2 v)
	{
		if (!rotateTarget) return;
		if (v.sqrMagnitude < 0.000001f) return;

		float angle = Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg + facingOffsetDeg;
		rotateTarget.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
	}

	void OnTriggerEnter2D(Collider2D col)
	{
		if (!active) return;
		if (((1 << col.gameObject.layer) & hitMask) == 0) return;

		col.GetComponent<EnemySimple>()?.Damage(dmg);
		ScreenShake.Shake(hitShake, 0.08f);

		if (destroyOnHit) Recycle();
	}

	void Recycle()
	{
		active = false;

		if (sr && data != null && data.despawnScaleOut > 0f)
			sr.transform.DOPunchScale(Vector3.one * -0.1f, data.despawnScaleOut, 6, 0.9f);

		Pool.Release(this); // return to the correct prefab pool
	}

	// Optional hook points for Pool
	public override void OnDespawned()
	{
		if (rb) rb.linearVelocity = Vector2.zero;
		// Optional: reset rotation if your pool expects a default pose
		// if (rotateTarget) rotateTarget.localRotation = Quaternion.identity;
	}
}
