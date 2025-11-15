using DG.Tweening;
using TMPro;
using UnityEngine;

public class BannerPopup : MonoBehaviour
{
	public RectTransform rect;
	public CanvasGroup cg;
	public TextMeshProUGUI text;

	[Header("Tuning")]
	public float showDuration = 2.0f;     // total time on screen
	public float punch = 0.25f;           // punch scale magnitude
	public float punchTime = 0.18f;       // punch time
	public float idleFloat = 20f;         // gentle float up
	public float idleTime = 1.8f;         // float duration (overlaps fade)

	void Awake()
	{
		if (!rect) rect = GetComponent<RectTransform>();
		if (!cg) cg = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
		if (!text) text = GetComponentInChildren<TextMeshProUGUI>();
	}

	/// <summary>
	/// Places the banner at upper-center of the parent layer and plays the punch/float/fade.
	/// Assumes this object is already parented under the FX layer with middle-center anchors.
	/// </summary>
	public void Play(string message)
	{
		if (text) text.text = message.ToUpperInvariant();

		cg.alpha = 0f;
		rect.anchoredPosition = new Vector2(0f, rect.rect.height); // start a bit high
		rect.localScale = Vector3.one * 0.9f;

		var seq = DOTween.Sequence();

		// quick appear + punch
		seq.Append(cg.DOFade(1f, 0.08f));
		seq.Join(rect.DOPunchScale(Vector3.one * punch, punchTime, 10, 0.9f));

		// idle float + subtle scale ease (feels like OutQuad settle)
		seq.Append(rect.DOAnchorPos(rect.anchoredPosition + new Vector2(0f, idleFloat), idleTime)
			.SetEase(Ease.OutQuad));
		seq.Join(rect.DOScale(1.0f, idleTime).SetEase(Ease.OutQuad));

		// fade out near the end
		seq.Insert(showDuration - 0.35f, cg.DOFade(0f, 0.35f));

		seq.OnComplete(() => Destroy(gameObject));
	}
}