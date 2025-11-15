using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemySimple : MonoBehaviour
{
	public float moveSpeed = 3.2f;
	public float touchDamage = 10f;
	public float hp = 20f;
	public GameObject xpOrbPrefab;

	Rigidbody2D rb; Transform player;
	SpriteRenderer sr; Color orig;

	void Awake()
	{
		rb = GetComponent<Rigidbody2D>(); rb.gravityScale = 0;
		sr = GetComponentInChildren<SpriteRenderer>();
		if (sr) orig = sr.color;
	}

	void Start() { player = GameObject.FindWithTag("Player")?.transform; }

	void FixedUpdate()
	{
		if (!player) return;
		Vector2 dir = (player.position - transform.position).normalized;
		rb.linearVelocity = dir * moveSpeed;
	}

	public void Damage(float amount)
	{
		hp -= amount;

		// quick red flash
		if (sr)
		{
			sr.DOKill();
			sr.color = new Color(1f, 0.35f, 0.35f, orig.a);
			sr.DOColor(orig, 0.08f);
		}

		if (hp <= 0f) Die();
	}

	void Die()
	{
		if (xpOrbPrefab) Instantiate(xpOrbPrefab, transform.position, Quaternion.identity);
		Destroy(gameObject);
	}

	void OnCollisionEnter2D(Collision2D col)
	{
		if (col.collider.CompareTag("Player"))
		{
			col.collider.GetComponent<Health>()?.Damage(touchDamage);
		}
	}
}