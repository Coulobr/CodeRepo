using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;     // DOTween for quick hit flash

public class Health : MonoBehaviour
{
	public float maxHP = 100f;
	public UnityEvent onDeath;
	public UnityEvent<float, float> onHealthChanged; // current, max

	[Header("Hit FX (Optional)")]
	public SpriteRenderer bodySprite;
	public Color hitColor = new Color(1f, 0.4f, 0.2f, 1f);
	public float hitFlashTime = 0.08f;

	float hp;
	Color _origColor = Color.white;

	void Awake()
	{
		hp = maxHP;
		if (!bodySprite) bodySprite = GetComponentInChildren<SpriteRenderer>();
		if (bodySprite) _origColor = bodySprite.color;
	}

	public void Damage(float amount)
	{
		if (hp <= 0) return;
		hp -= amount;
		onHealthChanged?.Invoke(hp, maxHP);

		// DOTween hit flash
		if (bodySprite)
		{
			bodySprite.DOKill();
			bodySprite.color = hitColor;
			bodySprite.DOColor(_origColor, hitFlashTime);
			bodySprite.transform.DOPunchScale(Vector3.one * 0.08f, 0.12f, 6, 0.7f);
		}

		if (hp <= 0) { onDeath?.Invoke(); }
	}

	public void Heal(float amount)
	{
		hp = Mathf.Min(maxHP, hp + amount);
		onHealthChanged?.Invoke(hp, maxHP);
	}

	public float Current => hp;
}