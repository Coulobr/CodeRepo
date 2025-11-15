using UnityEngine;
using DG.Tweening;

public class XPOrb : MonoBehaviour
{
	public int xpAmount = 1;
	public float attractDistance = 6f;
	public float speed = 10f;

	Transform player;

	void Start()
	{
		player = GameObject.FindWithTag("Player")?.transform;
		// subtle idle bob
		transform.DOLocalMoveY(transform.localPosition.y + 0.15f, 1.2f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
	}

	void Update()
	{
		if (!player) return;
		float d = Vector2.Distance(player.position, transform.position);
		if (d < attractDistance)
		{
			transform.position = Vector2.MoveTowards(transform.position, player.position, speed * Time.deltaTime);
		}
	}

	void OnTriggerEnter2D(Collider2D col)
	{
		if (col.CompareTag("Player"))
		{
			col.GetComponent<PlayerLevel>()?.AddXP(xpAmount);
			Destroy(gameObject);
		}
	}
}
