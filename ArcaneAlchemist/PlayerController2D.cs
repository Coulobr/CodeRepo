using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;          // DOTween

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController2D : MonoBehaviour
{
	[Header("Movement")]
	public float moveSpeed = 7f;
	public float dashSpeed = 18f;
	public float dashDuration = 0.18f;
	public float dashCooldown = 1.0f;

	[Header("FX (Optional)")]
	public SpriteRenderer bodySprite;     // assign if you want dash squash/hit flash
	public float dashSquash = 0.85f;
	public float dashSquashTime = 0.08f;

	[Header("Refs")]
	public AutoAttackScheduler autoAttack;
	public Health health;

	Rigidbody2D rb;
	
	Vector2 move;
	bool canDash = true;
	bool dashing;
	float dashTimer;

	private InputSystem_Actions input;

	void Awake()
	{
		rb = GetComponent<Rigidbody2D>();
		rb.gravityScale = 0f; rb.freezeRotation = true;
		input = new InputSystem_Actions();

		if (!bodySprite) bodySprite = GetComponentInChildren<SpriteRenderer>();
	}

	void OnEnable()
	{
		input.Enable();
		input.Player.Move.performed += OnMove;
		input.Player.Move.canceled += OnMove;
		input.Player.Dash.performed += OnDash;
	}

	void OnDisable()
	{
		input.Player.Move.performed -= OnMove;
		input.Player.Move.canceled -= OnMove;
		input.Player.Dash.performed -= OnDash;
		input.Disable();
	}

	void OnMove(InputAction.CallbackContext ctx) => move = ctx.ReadValue<Vector2>();

	void OnDash(InputAction.CallbackContext _)
	{
		if (canDash && !dashing) StartCoroutine(DashRoutine());
	}

	System.Collections.IEnumerator DashRoutine()
	{
		dashing = true; canDash = false; dashTimer = dashDuration;

		// DOTween squash & quick tint for dash
		if (bodySprite)
		{
			bodySprite.transform.DOKill();
			var t = bodySprite.transform;
			Vector3 orig = t.localScale;
			t.DOScale(new Vector3(dashSquash, 1f / dashSquash, 1f), dashSquashTime).SetLoops(2, LoopType.Yoyo);
			bodySprite.DOFade(0.85f, dashSquashTime).SetLoops(2, LoopType.Yoyo);
		}

		while (dashTimer > 0f)
		{
			dashTimer -= Time.deltaTime;
			rb.linearVelocity = move.sqrMagnitude > 0.01f ? move.normalized * dashSpeed : rb.linearVelocity;
			yield return null;
		}
		dashing = false;

		// cooldown (could also be DOVirtual.DelayedCall)
		yield return new WaitForSeconds(dashCooldown);
		canDash = true;
	}

	void FixedUpdate()
	{
		if (dashing) return;
		rb.linearVelocity = move.normalized * moveSpeed;
	}
}
