using UnityEngine;
using DG.Tweening;
using System.Collections;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using Random = UnityEngine.Random;

[CreateAssetMenu(menuName = "AA/Spells/Projectile (Unified)")]
public class ProjectileSpellSO : SpellSO
{
	public enum FireMode { Single, Fan }

	[Header("Mode")]
	public FireMode mode = FireMode.Single;

	[Header("Projectile")]
	public Projectile projectilePrefab;

	[Header("Firing")]
	public float shotsPerSecond = 2f;
	public int burstCount = 1;
	public float burstSpacing = 0.06f;

	// Single OR Fan (shared knobs)
	[Header("Pattern")]
	[Tooltip("For Single: adds random spread per shot. For Fan: additional random jitter per projectile.")]
	public float randomSpreadOrJitterDeg = 0f;

	[Tooltip("Fan only: number of projectiles per shot.")]
	public int projectilesPerShot = 1;

	[Tooltip("Fan only: total arc in degrees across all projectiles.")]
	public float arcDegrees = 0f;

	[Header("Ballistics")]
	public float speed = 12f;
	public float lifetime = 3f;
	public float damage = 10f;

	[Header("Targeting")]
	public float maxTargetRange = 18f;
	public bool fireTowardMoveIfNoTarget = true;
	public bool isHeatseaking = false;  // reserved for future

	[Header("VFX Hooks (DOTween)")]
	public float spawnScaleIn = 0.2f;     // seconds
	public float despawnScaleOut = 0.12f; // seconds
	public float spawnScaleAmount = 1.15f;

	public override SpellRuntime AttachTo(GameObject owner)
	{
		var rt = owner.AddComponent<UnifiedProjectileRuntime>();
		rt.BindDefinition(this);
		rt.InitFromSO(this);
		return rt;
	}
}

public class UnifiedProjectileRuntime : SpellRuntime
{
	ProjectileSpellSO data;
	Transform ownerTf;
	public Transform muzzle;   // optional spawn
	public Camera cam;         // optional; falls back to Camera.main

	public void InitFromSO(ProjectileSpellSO so)
	{
		data = so;
		cooldown = 1f / Mathf.Max(0.01f, so.shotsPerSecond);
	}

	public override void Init(Transform owner, float cd)
	{
		base.Init(owner, cd);
		ownerTf = owner;
		if (!cam) cam = Camera.main;
		if (!cam)
		{
			Debug.LogError("[UnifiedProjectileRuntime] No Camera assigned and no Camera.main found. Tag your camera as MainCamera or assign it in the inspector.", this);
		}
	}

	public override void Cast()
	{
		// Feedback
		ScreenShake.Shake(data.mode == ProjectileSpellSO.FireMode.Fan ? 0.9f : 0.5f,
						  data.mode == ProjectileSpellSO.FireMode.Fan ? 0.12f : 0.08f);

		StartCoroutine(BurstRoutine());
		ResetCD();
	}

	IEnumerator BurstRoutine()
	{
		int bursts = Mathf.Max(1, data.burstCount);
		for (int i = 0; i < bursts; i++)
		{
			if (data.mode == ProjectileSpellSO.FireMode.Single)
				FireSingle();
			else
				FireFan();

			if (i < bursts - 1 && data.burstSpacing > 0f)
				yield return new WaitForSeconds(data.burstSpacing);
		}
	}

	void FireSingle()
	{
		Vector2 origin = muzzle ? (Vector2)muzzle.position : (Vector2)owner.position;
		Vector2 aim = GetAimDir(); // normalized

		// optional random per-shot spread
		float angle = (data.randomSpreadOrJitterDeg != 0f)
					  ? Random.Range(-data.randomSpreadOrJitterDeg, data.randomSpreadOrJitterDeg)
					  : 0f;
		Vector2 dir = Quaternion.Euler(0, 0, angle) * aim;

		var proj = Pool.Get(data.projectilePrefab, ownerTf.position, Quaternion.identity);
		proj.transform.position = origin;
		proj.Launch(dir * data.speed, data.lifetime, data.damage, owner, ToLaunchShim());
		ownerTf.DOPunchScale(Vector3.one * 0.05f, 0.08f, 6, 0.9f);
	}

	void FireFan()
	{
		Vector2 origin = muzzle ? (Vector2)muzzle.position : (Vector2)owner.position;
		Vector2 aim = GetAimDir(); // normalized

		int n = Mathf.Max(1, data.projectilesPerShot);
		float arc = Mathf.Max(0f, data.arcDegrees);
		float step = (n > 1) ? (arc / (n - 1)) : 0f;
		float start = -arc * 0.5f;

		for (int i = 0; i < n; i++)
		{
			float baseAngle = start + step * i;
			float jitter = (data.randomSpreadOrJitterDeg != 0f)
						   ? Random.Range(-data.randomSpreadOrJitterDeg, data.randomSpreadOrJitterDeg)
						   : 0f;
			Vector2 dir = Quaternion.Euler(0, 0, baseAngle + jitter) * aim;

			var proj = Pool.Get(data.projectilePrefab, ownerTf.position, Quaternion.identity);
			proj.transform.position = origin;
			proj.Launch(dir.normalized * data.speed, data.lifetime, data.damage, owner, ToLaunchShim());
		}
		ownerTf.DOPunchScale(Vector3.one * 0.07f, 0.1f, 6, 0.9f);
	}

	// Ensures mouse → world is computed on owner’s plane (works with ortho/persp)
	Vector2 GetAimDir()
	{
		if (!owner) { Debug.LogError("[UnifiedProjectileRuntime] 'owner' is null.", this); return Vector2.right; }
		var c = cam ? cam : Camera.main;
		if (!c) { Debug.LogError("[UnifiedProjectileRuntime] 'cam' is null and Camera.main not found.", this); return Vector2.right; }

#if ENABLE_INPUT_SYSTEM
		Vector2 screen = Mouse.current != null ? Mouse.current.position.ReadValue()
											   : (Vector2)Input.mousePosition;
#else
        Vector2 screen = Input.mousePosition;
#endif
		Vector2 origin = muzzle ? (Vector2)muzzle.position : (Vector2)owner.position;

		float depth = c.orthographic
			? Mathf.Abs(owner.position.z - c.transform.position.z)
			: Vector3.Dot(owner.position - c.transform.position, c.transform.forward);

		Vector3 wp = c.ScreenToWorldPoint(new Vector3(screen.x, screen.y, depth));
		wp.z = owner.position.z;

		Vector2 dir = ((Vector2)wp - origin);
		if (dir.sqrMagnitude < 0.0001f) dir = Vector2.right;
		return dir.normalized;
	}

	// Minimal shim so your Projectile can read spawn/despawn timings just like before
	ProjectileSpellSO ToLaunchShim()
	{
		var shim = ScriptableObject.CreateInstance<ProjectileSpellSO>();
		shim.projectilePrefab = data.projectilePrefab;
		shim.spawnScaleIn = data.spawnScaleIn;
		shim.despawnScaleOut = data.despawnScaleOut;
		shim.spawnScaleAmount = data.spawnScaleAmount;
		return shim;
	}
}
